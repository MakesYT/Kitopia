﻿using System.Text;
using Core.SDKs.Tools;

namespace Core.SDKs.Everything;

public class Tools
{
    public static void main(List<SearchViewItem> Items, List<string> _name)
    {
        if (IntPtr.Size == 8)
            // 64-bit
            amd64(Items, _name);
        else
            // 32-bit
            amd32(Items, _name);
    }

    public static void amd32(List<SearchViewItem> Items, List<string> _name)
    {
        Everything32.Everything_Reset();
        Everything32.Everything_SetSearchW("*.docx|*.doc|*.xls|*.xlsx|*.pdf|*.ppt|*.pptx");
        Everything32.Everything_SetMatchCase(true);
        Everything32.Everything_QueryW(true);
        const int bufsize = 260;
        var buf = new StringBuilder(bufsize);
        for (var i = 0; i < Everything32.Everything_GetNumResults(); i++)
        {
            // get the result's full path and file name.
            Everything32.Everything_GetResultFullPathNameW(i, buf, bufsize);
            var filePath = buf.ToString();
            if (_name.Contains(filePath))
            {
                continue;
            }

            _name.Add(filePath);
            var fileInfo = new FileInfo(filePath);
            var fileType = FileType.None;
            if (fileInfo.Name.StartsWith("~") || fileInfo.Name.StartsWith("_"))
                continue;
            if (fileInfo.Extension == ".pdf")
                fileType = FileType.PDF文档;
            else if (fileInfo.Extension == ".docx" || fileInfo.Extension == ".doc")
                fileType = FileType.Word文档;
            else if (fileInfo.Extension == ".xlsx" || fileInfo.Extension == ".xls")
                fileType = FileType.Excel文档;
            else if (fileInfo.Extension == ".ppt" || fileInfo.Extension == ".pptx") fileType = FileType.PPT文档;
            var keys = new HashSet<string>();
            AppTools.NameSolver(keys, fileInfo.Name).Wait();
            var searchViewItem = new SearchViewItem
            {
                IsVisible = true, FileInfo = fileInfo, FileName = fileInfo.Name, FileType = fileType,
                Keys = keys,
                OnlyKey = filePath,
                Icon = null
            };
            Items.Add(searchViewItem);
        }
    }

    public static void amd64(List<SearchViewItem> Items, List<string> _name)
    {
        Everything64.Everything_Reset();
        Everything64.Everything_SetSearchW("*.docx|*.doc|*.xls|*.xlsx|*.pdf|*.ppt|*.pptx");
        Everything64.Everything_SetMatchCase(true);
        Everything64.Everything_QueryW(true);
        const int bufsize = 260;
        var buf = new StringBuilder(bufsize);
        for (var i = 0; i < Everything64.Everything_GetNumResults(); i++)
        {
            // get the result's full path and file name.
            Everything64.Everything_GetResultFullPathNameW(i, buf, bufsize);
            var filePath = buf.ToString();
            if (_name.Contains(filePath))
            {
                continue;
            }

            _name.Add(filePath);
            var fileInfo = new FileInfo(filePath);
            var fileType = FileType.None;
            if (fileInfo.Name.StartsWith("~") || fileInfo.Name.StartsWith("_"))
                continue;
            if (fileInfo.Extension == ".pdf")
                fileType = FileType.PDF文档;
            else if (fileInfo.Extension == ".docx" || fileInfo.Extension == ".doc")
                fileType = FileType.Word文档;
            else if (fileInfo.Extension == ".xlsx" || fileInfo.Extension == ".xls")
                fileType = FileType.Excel文档;
            else if (fileInfo.Extension == ".ppt" || fileInfo.Extension == ".pptx") fileType = FileType.PPT文档;
            var keys = new HashSet<string>();
            AppTools.NameSolver(keys, fileInfo.Name).Wait();
            var searchViewItem = new SearchViewItem
            {
                IsVisible = true, FileInfo = fileInfo, FileName = fileInfo.Name, FileType = fileType,
                Keys = keys,
                OnlyKey = filePath,
                Icon = null
            };
            Items.Add(searchViewItem);
        }
    }
}