using Core.SDKs;

namespace PluginCore;

public interface ISearchItemTool
{
    void OpenSearchItem(SearchViewItem searchViewItem);
    void OpenSearchItemByOnlyKey(string onlyKey);
}