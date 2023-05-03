
using Core.SDKs;
using Kitopia.SDKs.Everything;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitopia.SDKs.Everything
{
    public class Tools
    {
        public static void main(ref ObservableCollection<SearchViewItem> Items, string value)
        {
            if (IntPtr.Size == 8)
            {
                // 64-bit
                amd64(ref Items, value);
            }
            else
            {
                // 32-bit
                amd32(ref Items, value);
            }
        }
        public static void amd32(ref ObservableCollection<SearchViewItem> Items, string value)
        {
            Everything32.Everything_Reset();
            Everything32.Everything_SetSearchW("*.docx|*.doc|*.xls|*.xlsx|*.pdf|*.ppt|*.pptx" + " " + value);
            Everything32.Everything_SetMatchCase(true);
            Everything32.Everything_QueryW(true);
            const int bufsize = 260;
            StringBuilder buf = new StringBuilder(bufsize);
            for (int i = 0; i < Everything32.Everything_GetNumResults(); i++)
            {

                // get the result's full path and file name.
                Everything32.Everything_GetResultFullPathNameW(i, buf, bufsize);
                FileInfo fileInfo = new FileInfo(buf.ToString());
                FileType fileType = FileType.None;
                if (fileInfo.Name.StartsWith("~") || fileInfo.Name.StartsWith("_"))
                    continue;
                if (fileInfo.Extension == ".pdf")
                {
                    fileType = FileType.PDF文档;
                }
                else if (fileInfo.Extension == ".docx" || fileInfo.Extension == ".doc")
                {
                    fileType = FileType.Word文档;
                }
                else if (fileInfo.Extension == ".xlsx" || fileInfo.Extension == ".xls")
                {
                    fileType = FileType.Excel文档;
                }
                else if (fileInfo.Extension == ".ppt" || fileInfo.Extension == ".pptx")
                {
                    fileType = FileType.PPT文档;
                }

                Items.Add(new SearchViewItem { IsVisible = true, fileInfo = fileInfo, fileName = fileInfo.Name, fileType = fileType, icon = GetIconFromFile.GetIcon(fileInfo.FullName) });
            }
        }
        public static void amd64(ref ObservableCollection<SearchViewItem> Items,string value)
        {
            Everything64.Everything_Reset();
            Everything64.Everything_SetSearchW("*.docx|*.doc|*.xls|*.xlsx|*.pdf|*.ppt|*.pptx" + " " + value);
            Everything64.Everything_SetMatchCase(true);
            Everything64.Everything_QueryW(true);
            const int bufsize = 260;
            StringBuilder buf = new StringBuilder(bufsize);
            for (int i = 0; i < Everything64.Everything_GetNumResults(); i++)
            {

                // get the result's full path and file name.
                Everything64.Everything_GetResultFullPathNameW(i, buf, bufsize);
                FileInfo fileInfo = new FileInfo(buf.ToString());
                FileType fileType = FileType.None;
                if (fileInfo.Name.StartsWith("~") || fileInfo.Name.StartsWith("_"))
                    continue;
                if (fileInfo.Extension == ".pdf")
                {
                    fileType = FileType.PDF文档;
                }
                else if (fileInfo.Extension == ".docx" || fileInfo.Extension == ".doc")
                {
                    fileType = FileType.Word文档;
                }
                else if (fileInfo.Extension == ".xlsx" || fileInfo.Extension == ".xls")
                {
                    fileType = FileType.Excel文档;
                }
                else if (fileInfo.Extension == ".ppt" || fileInfo.Extension == ".pptx")
                {
                    fileType = FileType.PPT文档;
                }

                Items.Add(new SearchViewItem { IsVisible = true, fileInfo = fileInfo, fileName = fileInfo.Name, fileType = fileType, icon = GetIconFromFile.GetIcon(fileInfo.FullName) });
            }
        }
    }
}
