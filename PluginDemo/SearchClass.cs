using System;
using Core.SDKs;

namespace PluginDemo;

public class SearchClass
{
    public SearchViewItem a1(string search)
    {
        var action = new Action<SearchViewItem>((e) =>
        {
            Console.WriteLine(e.OnlyKey);
        });
        return new SearchViewItem()
        {
            FileName = "内容来自插件" + search,
            FileType = FileType.自定义,
            OnlyKey = search,
            Icon = null,
            Action = action,
            IconSymbol = 0xF6EC,
            IsVisible = true
        };
    }
}