using System.Drawing;
using System.Runtime.InteropServices;

namespace Core.SDKs
{
    public class GetIconFromFile
    {
        static Dictionary<string, Icon> icons = new System.Collections.Generic.Dictionary<string, Icon>();
        [DllImport("User32.dll")]
        public static extern int PrivateExtractIcons(
             string lpszFile, //file name
             int nIconIndex,  //The zero-based index of the first icon to extract.
             int cxIcon,      //The horizontal icon size wanted.
             int cyIcon,      //The vertical icon size wanted.
             IntPtr[] phicon, //(out) A pointer to the returned array of icon handles.
             int[] piconid,   //(out) A pointer to a returned resource identifier.
             int nIcons,      //The number of icons to extract from the file. Only valid when *.exe and *.dll
             int flags        //Specifies flags that control this function.
         );

        //details:https://msdn.microsoft.com/en-us/library/windows/desktop/ms648063(v=vs.85).aspx
        //Destroys an icon and frees any memory the icon occupied.
        [DllImport("User32.dll")]
        public static extern bool DestroyIcon(
             IntPtr hIcon //A handle to the icon to be destroyed. The icon must not be in use.
         );
        public static Icon GetIcon(string path)
        {
            if (path.ToLower().EndsWith(".exe")|| path.ToLower().EndsWith(".lnk"))
            {
                var iconTotalCount = PrivateExtractIcons((string)path, 0, 0, 0, null, null, 0, 0);

                //用于接收获取到的图标指针
                IntPtr[] hIcons = new IntPtr[iconTotalCount];
                //对应的图标id
                int[] ids = new int[iconTotalCount];
                //成功获取到的图标个数
                var successCount = PrivateExtractIcons((string)path, 0, 64, 64, hIcons, ids, iconTotalCount, 0);

                //遍历并保存图标

                for (var i = 0; i < successCount; i++)
                {
                    //指针为空，跳过
                    if (hIcons[i] == IntPtr.Zero) continue;

                    using (var icon = Icon.FromHandle(hIcons[i]))
                    {
                        return icon;
                    }

                }
            }else if (icons.ContainsKey(path.Split(".").Last()))
            {
                return icons[path.Split(".").Last()];
            }
            else
            {
                Icon icon1 = Icon.ExtractAssociatedIcon((string)path);
                icons.Add(path.Split(".").Last(), icon1);
                return icon1;
            }
            return  Icon.ExtractAssociatedIcon((string)path);



        }
    }
}
