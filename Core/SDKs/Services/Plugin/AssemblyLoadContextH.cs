#region

using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Core.SDKs.Services.Config;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Core.SDKs.Services.Plugin;

public class AssemblyLoadContextH : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private Assembly _assembly;

    public AssemblyLoadContextH(string pluginPath, string name) : base(isCollectible: true, name: name)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
        _assembly = this.LoadFromAssemblyPath(pluginPath);

        Unloading += (sender) => {
            AppDomain.CurrentDomain.GetAssemblies()
                     .FirstOrDefault(x => x.GetName()
                                           .Name == "System.Text.Json")
                    ?.GetType("System.Text.Json.Serialization.Metadata.ReflectionEmitCachingMemberAccessor")
                    ?.GetMethod("Clear")
                    ?.Invoke(null, null);
            // var fieldInfo = ConfigManger.DefaultOptions.GetType().GetField("_cachingContext", BindingFlags.NonPublic | BindingFlags.Instance);
            // fieldInfo.FieldType.GetMethod("Clear")?.Invoke(fieldInfo.GetValue(ConfigManger.DefaultOptions), null);
            ConfigManger.DefaultOptions = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve,
            };
            _assembly = null;
            AvaloniaPropertyRegistry.Instance.UnregisterByModule(sender.Assemblies.First()
                                                                       .DefinedTypes);
            ServiceManager.Services.GetService<IPluginToolService>()!.RequestUninstallPlugin(pluginPath);
        };
    }

    public Assembly Assembly => _assembly;

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            if (assemblyPath.EndsWith("WinRT.Runtime.dll") || assemblyPath.EndsWith("Microsoft.Windows.SDK.NET.dll"))
            {
                return null;
            }

            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}