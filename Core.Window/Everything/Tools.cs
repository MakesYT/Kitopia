#region

using System.IO;
using System.Text;
using Core.SDKs.Services.Config;
using PluginCore;

#endregion

namespace Core.Window.Everything;

public class Tools
{
    public static void main(List<string> Items)
    {
        
        if (IntPtr.Size == 8)
            // 64-bit
        {
            amd64(Items);
        }
        else
            // 32-bit
        {
            amd32(Items);
        }
        
        
    }

    public static void amd32(List<string> Items)
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
            Items.Add(filePath);
        }
    }

    public static void amd64(List<string> Items)
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
            Items.Add(filePath);
        }
    }
}