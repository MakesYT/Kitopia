﻿using System.Drawing;

namespace Core.SDKs
{
    public class SearchViewItem
    {
        public string? fileName { set; get; }
        public bool? IsVisible { set; get; }
        public HashSet<string>? keys { set; get; }
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
