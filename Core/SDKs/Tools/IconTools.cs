#region

using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Core.SDKs.Services;

#endregion

namespace Core.SDKs.Tools;

public class IconTools
{
    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
    private const uint SHGFI_OPENICON = 0x000000002;
    private static readonly Dictionary<string, Icon> _icons = new(250);

    public void ClearCache()
    {
        foreach (var iconsValue in _icons.Values)
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

    public Icon GetIcon(string path)
    {
        string cacheKey;
        var mscFile = false;
        switch (path.ToLower().Split(".").Last())
        {
            case "docx":
            case "doc":
            case "xls":
            case "xlsx":
            case "pdf":
            case "ppt":
            case "pptx":
            {
                cacheKey = path.Split(".").Last();
                break;
            }
            case "msc":
            {
                cacheKey = path.Split("\\").Last();
                mscFile = true;
                break;
            }
            default:
            {
                cacheKey = path;
                break;
            }
        }

        //缓存
        if (_icons.TryGetValue(cacheKey, out var icon2))
        {
            return icon2;
        }

        if (mscFile)
        {
            var index = 0;
            string dllPath;
            var xd = new XmlDocument();
            xd.Load(path); //加载xml文档
            var rootNode = xd.SelectSingleNode("MMC_ConsoleFile"); //得到xml文档的根节点
            var BinaryStorage = rootNode.SelectSingleNode("VisualAttributes").SelectSingleNode("Icon");
            index = int.Parse(((XmlElement)BinaryStorage).GetAttribute("Index"));
            dllPath = ((XmlElement)BinaryStorage).GetAttribute("File");

            dllPath = Environment.SystemDirectory + "\\" + dllPath.Split("\\").Last();
            path = dllPath;
            if (cacheKey.Contains("taskschd.msc"))
            {
                index += 1;
            }

            var iconTotalCount = PrivateExtractIcons(dllPath, index, 0, 0, null!, null!, 0, 0);

            //用于接收获取到的图标指针
            var hIcons = new IntPtr[iconTotalCount];
            //对应的图标id
            var ids = new int[iconTotalCount];
            //成功获取到的图标个数
            var successCount = PrivateExtractIcons((string)dllPath, index, 48, 48, hIcons, ids, iconTotalCount, 0);
            for (var i = 0; i < successCount; i++)
            {
                //指针为空，跳过
                if (hIcons[i] == IntPtr.Zero)
                {
                    continue;
                }

                using (var icon = Icon.FromHandle(hIcons[i]))
                {
                    var independenceIcon = (Icon)icon.Clone();
                    DestroyIcon(icon.Handle);
                    _icons.TryAdd(cacheKey, independenceIcon);

                    return independenceIcon;
                }
            }
        }

        #region 获取64*64的尺寸Icon

        {
            var iconTotalCount = PrivateExtractIcons(path, 0, 0, 0, null!, null!, 0, 0);

            //用于接收获取到的图标指针
            var hIcons = new IntPtr[iconTotalCount];
            //对应的图标id
            var ids = new int[iconTotalCount];
            //成功获取到的图标个数
            var successCount = PrivateExtractIcons((string)path, 0, 48, 48, hIcons, ids, iconTotalCount, 0);

            //遍历并保存图标

            for (var i = 0; i < successCount; i++)
            {
                //指针为空，跳过
                if (hIcons[i] == IntPtr.Zero)
                {
                    continue;
                }

                using (var icon = Icon.FromHandle(hIcons[i]))
                {
                    var independenceIcon = (Icon)icon.Clone();
                    DestroyIcon(icon.Handle);
                    _icons.TryAdd(cacheKey, independenceIcon);

                    return independenceIcon;
                }
            }
        }

        #endregion


        var shinfo = new SHFILEINFO();
        SHGetFileInfo(
            path,
            0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
            SHGFI_ICON | SHGFI_LARGEICON | SHGFI_USEFILEATTRIBUTES | SHGFI_OPENICON);
        var independenceIcon12 = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
        DestroyIcon(shinfo.hIcon);
        _icons.TryAdd(cacheKey, independenceIcon12);
        return independenceIcon12;

        #region 32*32的Icon

        var icon1 = Icon.ExtractAssociatedIcon((string)path);
        var independenceIcon1 = (Icon)icon1!.Clone();
        DestroyIcon(icon1.Handle);
        _icons.TryAdd(cacheKey, independenceIcon1);
        return independenceIcon1;

        #endregion
    }

    public Icon? GetFormClipboard()
    {
        var bitmap =
            ((IClipboardService)ServiceManager.Services.GetService(typeof(IClipboardService)))
            .GetBitmap();
        if (bitmap is null)
        {
            return null;
        }

        var image = bitmap.GetThumbnailImage(48, 48, null, IntPtr.Zero); // 获取指定大小的缩略图
        var converter = new IconConverter(); // 创建转换器对象
        var bytes = converter.ConvertTo(image, typeof(byte[])) as byte[]; // 将 image 转换为字节数组
        var icon = converter.ConvertFrom(bytes) as Icon; // 将字节数组转换为 icon 对象
        var ms = new MemoryStream(); // 创建内存流
        icon.Save(ms); // 将 icon 保存到流中
        ms.Position = 0; // 重置流位置
        icon = new Icon(ms); // 从流中创建 icon 对象
        ms.Close(); // 关闭流
        var independenceIcon12 = (Icon)icon.Clone();
        DestroyIcon(icon.Handle);
        //_icons.Add(cacheKey,independenceIcon12);
        return independenceIcon12;
    }


    public Icon ExtractFromPath(string path)
    {
        if (_icons.TryGetValue(path, out var fromPath))
        {
            return fromPath;
        }

        var shinfo = new SHFILEINFO();
        SHGetFileInfo(
            path,
            0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
            SHGFI_ICON | SHGFI_LARGEICON);
        var independenceIcon12 = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
        DestroyIcon(shinfo.hIcon);
        _icons.TryAdd(path, independenceIcon12);
        return independenceIcon12;
    }

    [DllImport("shell32.dll")]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
        uint cbSizeFileInfo, uint uFlags);

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
}