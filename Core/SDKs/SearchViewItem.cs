﻿using System.Drawing;

namespace Core.SDKs;

public class SearchViewItem: ICloneable,IDisposable
{
    public string? FileName { set; get; }
    public bool? IsVisible { set; get; }
    public HashSet<string>? Keys { set; get; }
    public FileType FileType { set; get; }
    public FileInfo? FileInfo { set; get; }
    public DirectoryInfo? DirectoryInfo { set; get; }
    public Icon? Icon { set; get; }

    public object Clone()
    {
        return new SearchViewItem()
        {
            FileName = FileName,
            IsVisible = IsVisible,
            Keys = new HashSet<string>(Keys!),
            FileType = FileType,
            FileInfo = FileInfo!=null?new FileInfo(FileInfo.FullName):null,
            DirectoryInfo = DirectoryInfo!=null?new DirectoryInfo(DirectoryInfo.FullName):null,
        };
    }

    public void Dispose()
    {
        if (Icon != null)
        {
            Icon.Dispose();
            Icon = null;
        }

        if (FileInfo != null)
        {
            FileInfo = null;
            
        }

        if (Keys!=null)
        {
            Keys.Clear();
            Keys = null;
        }
    }
}

public enum FileType
{
    应用程序,
    Word文档,
    PPT文档,
    Excel文档,
    PDF文档,
    图像,
    文件夹,
    None
}