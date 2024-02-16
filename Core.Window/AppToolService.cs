using Core.SDKs.Services;
using PluginCore;

namespace Core.SDKs.Tools;

public class AppToolService : IAppToolService
{
    public async Task AppSolverA(Dictionary<string, SearchViewItem> _collection, string search, bool isSearch = false)
    {
        await AppTools.AppSolverA(_collection, search, isSearch);
    }

    public void DelNullFile(Dictionary<string, SearchViewItem> _collection)
    {
        AppTools.DelNullFile(_collection);
    }

    public void GetAllApps(Dictionary<string, SearchViewItem> _collection, bool logging)
    {
        AppTools.GetAllApps(_collection, logging);
    }

    public void AutoStartEverything(Dictionary<string, SearchViewItem> _collection, Action action)
    {
        AppTools.AutoStartEverything(_collection, action);
    }

    public async Task GetIconByItemAsync(SearchViewItem item)
    {
        await IconTools.GetIconByItemAsync(item);
    }
}