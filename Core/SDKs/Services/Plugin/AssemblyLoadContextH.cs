#region

using System.Reflection;
using System.Runtime.Loader;
using Avalonia;

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
        var assemblyName = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == "Newtonsoft.Json")?.GetName();
        LoadFromAssemblyName(assemblyName);
        Unloading += (sender) =>
        {
            _assembly = null;
            AvaloniaPropertyRegistry.Instance.UnregisterByModule(sender.Assemblies.First().DefinedTypes);
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