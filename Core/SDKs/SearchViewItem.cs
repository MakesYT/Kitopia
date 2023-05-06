using System.Drawing;

namespace Core.SDKs
{
    public class SearchViewItem
    {
        public int weight { get; set; } = 1;
        public string? fileName { set; get; }
        public bool? IsVisible { set; get; }
        public List<string>? keys { set; get; }
        public FileType fileType { set; get; }
        public FileInfo? fileInfo { set; get; }
        public Icon? icon { set; get; }
    }
    public enum FileType
    {
        App,
        Word文档,
        PPT文档,
        Excel文档,
        PDF文档,
        图像,
        None
    }
}
