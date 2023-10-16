#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

#endregion

namespace Core.SDKs;

public class SearchViewItem : IDisposable
{
    public string? FileName
    {
        set;
        get;
    }

    public bool? IsVisible
    {
        set;
        get;
    }

    public bool IsStared
    {
        set;
        get;
    }

    public bool IsPined
    {
        set;
        get;
    } = false;

    public HashSet<string>? Keys
    {
        set;
        get;
    }

    public FileType FileType
    {
        set;
        get;
    }

    public FileInfo? FileInfo
    {
        set;
        get;
    }

    public DirectoryInfo? DirectoryInfo
    {
        set;
        get;
    }

    public string OnlyKey
    {
        set;
        get;
    } = "";

    public string? Url
    {
        set;
        get;
    }

    public int IconSymbol
    {
        set;
        get;
    }

    public Icon? Icon
    {
        set;
        get;
    }

    public string? IconPath
    {
        set;
        get;
    }

    public Action<SearchViewItem>? Action
    {
        set;
        get;
    }

    public Func<SearchViewItem, Icon>? GetIconAction
    {
        set;
        get;
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}

public enum FileType
{
    应用程序,
    URL,
    Word文档,
    PPT文档,
    Excel文档,
    PDF文档,
    图像,
    剪贴板图像,
    文件夹,
    文件,
    命令,
    数学运算,
    UWP应用,
    自定义情景,
    便签,
    自定义,
    None
}