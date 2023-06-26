using System.Drawing;

namespace Core.SDKs;

public class SearchViewItem : ICloneable, IDisposable
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

    public object Clone()
    {
        return new SearchViewItem()
        {
            FileName = FileName,
            IsVisible = IsVisible,
            Keys = new HashSet<string>(Keys!),
            FileType = FileType,
            IsStared = IsStared,
            OnlyKey = OnlyKey,
            FileInfo = FileInfo != null ? new FileInfo(FileInfo.FullName) : null,
            Icon = (Icon?)Icon?.Clone(),
            DirectoryInfo = DirectoryInfo != null ? new DirectoryInfo(DirectoryInfo.FullName) : null,
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

        if (Keys != null)
        {
            Keys.Clear();
            Keys = null;
        }
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
    None
}