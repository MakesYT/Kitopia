using System.Collections.Concurrent;
using Core.SDKs.Services;
using Pinyin.NET;
using PluginCore;

namespace Core.Linux;

public class AppToolLinuxService : IAppToolService
{
    public Task AppSolverA(ConcurrentDictionary<string, SearchViewItem> _collection, string search,
        bool isSearch = false)
    {
        return Task.CompletedTask;
    }

    public void DelNullFile(ConcurrentDictionary<string, SearchViewItem> _collection)
    {
    }

    public void GetAllApps(ConcurrentDictionary<string, SearchViewItem> _collection, bool logging,
        bool useEverything = false)
    {
    }

    public void AutoStartEverything(ConcurrentDictionary<string, SearchViewItem> _collection, Action action)
    {
    }

    public Task GetIconByItemAsync(SearchViewItem item)
    {
        return Task.CompletedTask;
    }

    public PinyinItem GetPinyin(string input)
    {
        throw new NotImplementedException();
    }
}