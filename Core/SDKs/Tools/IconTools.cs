﻿#region

using System.Drawing;
using System.Runtime.InteropServices;
using Core.SDKs.Services;
using KitopiaAvalonia.Tools;
using log4net;
using PluginCore;
using Vanara.PInvoke;
using Size = System.Drawing.Size;

#endregion

namespace Core.SDKs.Tools;

public class IconTools
{
    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;
    private const uint SHGFI_SMALLICON = 0x000000001;
    private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
    private const uint SHGFI_OPENICON = 0x000000002;
    private static readonly ILog log = LogManager.GetLogger(nameof(IconTools));
    private static readonly Dictionary<string, Avalonia.Media.Imaging.Bitmap> _icons = new(250);


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

    private static Icon? GetIconBase(string path, string cacheKey, bool mscFile = false)
    {
        switch (Path.GetExtension(path))
        {
            case ".png":
            case ".bmp":
            case ".ico":
            case ".jpg":
            {
                using var bm = new Bitmap(path);
                using var iconBm = new Bitmap(bm, new Size(64, 64));
                //如果是windows调用，直接下面一行代码就可以了
                //此代码不能在web程序中调用，会有安全异常抛出
                retry:
                try
                {
                    var icon = Icon.FromHandle(iconBm.GetHicon());
                    return icon;
                }
                catch (Exception e)
                {
                    goto retry;
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

                var icon = Icon.FromHandle(hIcons[i]);

                return icon;
            }
        }

        #endregion


        var hres = Shell32.SHGetImageList(Shell32.SHIL.SHIL_EXTRALARGE, typeof(ComCtl32.IImageList2).GUID, out var iml);
        if (hres.Failed) throw new System.ComponentModel.Win32Exception(hres.Code);

        // Get the icon index for a file
        var shfi = new Shell32.SHFILEINFO();
        var hIcon = Shell32.SHGetFileInfo(path, 0, ref shfi, Shell32.SHFILEINFO.Size,
            Shell32.SHGFI.SHGFI_ICONLOCATION | Shell32.SHGFI.SHGFI_SYSICONINDEX);
        if (hIcon == IntPtr.Zero) return null;

        // Get the icon from the image list
        var safe = ((ComCtl32.IImageList2)iml).GetIcon(shfi.iIcon, ComCtl32.IMAGELISTDRAWFLAGS.ILD_TRANSPARENT);
        if (safe == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();

        try
        {
            return (Icon)Icon.FromHandle(safe);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static void GetIconInItems(SearchViewItem t)
    {
        //Log.Debug($"为{t.OnlyKey}生成Icon");

        {
            switch (t.FileType)
            {
                case FileType.文件夹:
                    IconTools.ExtractFromPath(t.OnlyKey, t);
                    break;
                case FileType.URL:
                    if (t.IconPath is not null)
                    {
                        IconTools.GetIcon(t.IconPath, t);
                        t.IconPath = null;
                    }

                    break;
                case FileType.自定义:
                    if (t.GetIconAction != null)
                    {
                        var icon3 = t.GetIconAction(t);
                        t.Icon = icon3;
                    }

                    break;
                case FileType.UWP应用:
                    IconTools.GetIcon(t.IconPath!, t);
                    t.IconPath = null;
                    break;
                case FileType.应用程序:
                case FileType.Word文档:
                case FileType.PPT文档:
                case FileType.Excel文档:
                case FileType.PDF文档:
                case FileType.图像:
                case FileType.文件:
                    IconTools.GetIcon(t.OnlyKey, t);
                    break;
                case FileType.命令:
                case FileType.自定义情景:
                case FileType.便签:
                case FileType.数学运算:
                case FileType.剪贴板图像:
                case FileType.None:
                    break;

                default:
                    IconTools.GetIcon(t.OnlyKey, t);
                    break;
            }
        }

        //Log.Debug(t.OnlyKey);

        //
    }

    public static void GetIcon(string path, SearchViewItem item)
    {
        //log.Debug(1);

        string cacheKey;
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
                break;
            }
            default:
            {
                cacheKey = path;
                break;
            }
        }

        if (cacheKey.EndsWith("mmc.exe"))
        {
            cacheKey = item.Arguments.Replace("\"", null);
            path = cacheKey;
        }

        //缓存
        if (_icons.TryGetValue(cacheKey, out var icon2))
        {
            item.Icon = icon2;
        }


        retry:
        var iconBase = GetIconBase(path, cacheKey);
        if (iconBase == null)
        {
            goto retry;
        }

        var clone = ((Bitmap)iconBase.ToBitmap()).ToAvaloniaBitmap();
        _icons.TryAdd(cacheKey, clone);
        iconBase.Dispose();
        item.Icon = clone;
    }

    public Avalonia.Media.Imaging.Bitmap? GetFormClipboard()
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
        var independenceIcon12 = icon.ToBitmap().ToAvaloniaBitmap();
        DestroyIcon(icon.Handle);
        //_icons.Add(cacheKey,independenceIcon12);
        return independenceIcon12;
    }


    public static void ExtractFromPath(string path, SearchViewItem item)
    {
        if (_icons.TryGetValue(path, out var fromPath))
        {
            item.Icon = fromPath;
        }


        {
            retry:
            try
            {
                var shinfo = new SHFILEINFO();
                SHGetFileInfo(
                    path,
                    0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                    SHGFI_ICON | SHGFI_LARGEICON);
                var independenceIcon12 = Icon.FromHandle(shinfo.hIcon).ToBitmap().ToAvaloniaBitmap();
                DestroyIcon(shinfo.hIcon);
                _icons.TryAdd(path, independenceIcon12);
                item.Icon = independenceIcon12;
            }
            catch (Exception e)
            {
                goto retry;
            }
        }
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