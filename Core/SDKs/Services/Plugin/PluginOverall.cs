using PluginCore;

namespace Core.SDKs.Services.Plugin;

public class PluginOverall
{
    public static readonly Dictionary<string, List<Func<string, SearchViewItem?>>> SearchActions = new();
}