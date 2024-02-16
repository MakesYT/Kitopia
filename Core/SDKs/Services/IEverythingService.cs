using PluginCore;

namespace Core.SDKs.Services;

public interface IEverythingService
{
    public bool isRun();
    public void GetItem(Dictionary<string, SearchViewItem> _collection);
}