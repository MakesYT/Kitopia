using System.Collections.Concurrent;
using Core.SDKs.Services;
using PluginCore;

namespace Core.Window;

public class AppToolService : IAppToolService
{
    public async Task AppSolverA(ConcurrentDictionary<string, SearchViewItem> _collection, string search,
        bool isSearch = false)
    {
        await Window.AppTools.AppSolverA(_collection, search, isSearch);
    }

    public void DelNullFile(ConcurrentDictionary<string, SearchViewItem> _collection)
    {
        Window.AppTools.DelNullFile(_collection);
    }

    public void GetAllApps(ConcurrentDictionary<string, SearchViewItem> _collection, bool logging,
        bool useEverything = false)
    {
        Window.AppTools.GetAllApps(_collection, logging, useEverything);
    }

    public void AutoStartEverything(ConcurrentDictionary<string, SearchViewItem> _collection, Action action)
    {
        Window.AppTools.AutoStartEverything(_collection, action);
    }

    public async Task GetIconByItemAsync(SearchViewItem item)
    {
        await IconTools.GetIconByItemAsync(item);
    }
}