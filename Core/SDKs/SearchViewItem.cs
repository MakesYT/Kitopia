using System.Drawing;

namespace Core.SDKs;

public class SearchViewItem: ICloneable
{
    public string? fileName { set; get; }
    public bool? IsVisible { set; get; }
    public HashSet<string>? keys { set; get; }
    public FileType fileType { set; get; }
    public FileInfo? fileInfo { set; get; }
    public DirectoryInfo? directoryInfo { set; get; }
    public Icon? icon { set; get; }

    public object Clone()
    {
        return new SearchViewItem()
        {
            fileName = fileName,
            IsVisible = IsVisible,
            keys = new HashSet<string>(keys),
            fileType = fileType,
            fileInfo = new FileInfo(fileInfo.FullName),
        };
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