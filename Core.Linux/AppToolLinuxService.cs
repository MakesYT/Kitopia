using Core.SDKs.Services;
using PluginCore;

namespace Core.Linux;

public class AppToolLinuxService: IAppToolService
{
    public Task AppSolverA(Dictionary<string, SearchViewItem> _collection, string search, bool isSearch = false)
    {
        return Task.CompletedTask;
    }

    public void DelNullFile(Dictionary<string, SearchViewItem> _collection)
    {
        
    }

    public void GetAllApps(Dictionary<string, SearchViewItem> _collection, bool logging)
    {
       
    }

    public void AutoStartEverything(Dictionary<string, SearchViewItem> _collection, Action action)
    {
        
    }

    public Task GetIconByItemAsync(SearchViewItem item)
    {
        return Task.CompletedTask;
    }
}