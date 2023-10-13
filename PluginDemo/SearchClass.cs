using System;
using System.Windows;
using Core.SDKs;
using PluginCore.Attribute;

namespace PluginDemo;

public class SearchClass
{
    [SearchMethod]
    public SearchViewItem a1(string search)
    {
        var action = new Action<SearchViewItem>((e) =>
        {
            MessageBox.Show(e.OnlyKey);
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