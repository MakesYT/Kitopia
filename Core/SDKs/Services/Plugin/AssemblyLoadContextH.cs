#region

using System.Reflection;
using System.Runtime.Loader;

#endregion

namespace Core.SDKs.Services.Plugin;

public class AssemblyLoadContextH : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public AssemblyLoadContextH(string pluginPath, string name) : base(isCollectible: true, name: name)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
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