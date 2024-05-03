﻿using System.Collections.Concurrent;
using Pinyin.NET;
using PluginCore;

namespace Core.SDKs.Services;

public interface IAppToolService
{
    public Task AppSolverA(ConcurrentDictionary<string, SearchViewItem> _collection, string search,
        bool isSearch = false);

    public void DelNullFile(ConcurrentDictionary<string, SearchViewItem> _collection);

    public void GetAllApps(ConcurrentDictionary<string, SearchViewItem> _collection, bool logging,
        bool useEverything = false);

    public void AutoStartEverything(ConcurrentDictionary<string, SearchViewItem> _collection, Action action);
    public Task GetIconByItemAsync(SearchViewItem item);
    public PinyinItem GetPinyin(string input);
}