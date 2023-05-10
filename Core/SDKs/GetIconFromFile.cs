using System.Drawing;
using System.Runtime.InteropServices;

namespace Core.SDKs;

public class GetIconFromFile
{
    private readonly Dictionary<string, Icon> _icons = new();

    public  void ClearCache()
    {
        foreach (Icon iconsValue in _icons.Values)
        {
            iconsValue.Dispose();
        }
        _icons.Clear();
    }
    [DllImport("User32.dll")]
    public static extern int PrivateExtractIcons(
        string lpszFile, //file name
        int nIconIndex, //The zero-based index of the first icon to extract.
        int cxIcon, //The horizontal icon size wanted.
        int cyIcon, //The vertical icon size wanted.
        IntPtr[] phicon, //(out) A pointer to the returned array of icon handles.
        int[] piconid, //(out) A pointer to a returned resource identifier.
        int nIcons, //The number of icons to extract from the file. Only valid when *.exe and *.dll
        int flags //Specifies flags that control this function.
    );

    //details:https://msdn.microsoft.com/en-us/library/windows/desktop/ms648063(v=vs.85).aspx
    //Destroys an icon and frees any memory the icon occupied.
    [DllImport("User32.dll")]
    public static extern bool DestroyIcon(
        IntPtr hIcon //A handle to the icon to be destroyed. The icon must not be in use.
    );

    public  Icon GetIcon(string path)
    {
        
        if (path.ToLower().EndsWith(".exe") || path.ToLower().EndsWith(".lnk")|| path.ToLower().EndsWith(".msc")|| path.ToLower().EndsWith(".appref-ms"))
        {
            if (_icons.ContainsKey(path.Split("\\").Last()))
            {
                return _icons[path.Split("\\").Last()];
            }
            var iconTotalCount = PrivateExtractIcons(path, 0, 0, 0, null!, null!, 0, 0);

            //用于接收获取到的图标指针
            var hIcons = new IntPtr[iconTotalCount];
            //对应的图标id
            var ids = new int[iconTotalCount];
            //成功获取到的图标个数
            var successCount = PrivateExtractIcons((string)path, 0, 64, 64, hIcons, ids, iconTotalCount, 0);

            //遍历并保存图标

            for (var i = 0; i < successCount; i++)
            {
                //指针为空，跳过
                if (hIcons[i] == IntPtr.Zero) continue;

                using (var icon = Icon.FromHandle(hIcons[i]))
                {
                    Icon independenceIcon = (Icon)icon.Clone();
                    DestroyIcon(icon.Handle);
                    _icons.Add(path.Split("\\").Last(),independenceIcon);
                    
                    return independenceIcon;
                }
            }
        }
        else if (_icons.ContainsKey(path.Split(".").Last()))
        {
            return _icons[path.Split(".").Last()];
        }
        else
        {
            var icon1 = Icon.ExtractAssociatedIcon((string)path);
            Icon independenceIcon1 = (Icon)icon1!.Clone();
            DestroyIcon(icon1.Handle);
            _icons.Add(path.Split(".").Last(), independenceIcon1);
            return independenceIcon1;
        }
        var icon12 = Icon.ExtractAssociatedIcon((string)path);
        Icon independenceIcon12 = (Icon)icon12!.Clone();
        DestroyIcon(icon12.Handle);
        
        return independenceIcon12;
        
    }
    
    

    

    public Icon ExtractFromPath(string path)
    {
        if (_icons.TryGetValue(path, out var fromPath))
        {
            return fromPath;
        }
        SHFILEINFO shinfo = new SHFILEINFO();
        SHGetFileInfo(
            path,
            0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
            SHGFI_ICON | SHGFI_LARGEICON);
        Icon independenceIcon12 = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
        _icons.Add(path,independenceIcon12);
        return independenceIcon12;
    }

//Struct used by SHGetFileInfo function
    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    [DllImport("shell32.dll")]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;
    private const uint SHGFI_SMALLICON = 0x000000001;
    
}