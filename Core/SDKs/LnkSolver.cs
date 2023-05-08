using System.Runtime.InteropServices;
using System.Text;

namespace Core.SDKs;

public class LnkSolver
{
    [Flags]
    public enum SIGDN : uint
    {
        NORMALDISPLAY = 0,
        PARENTRELATIVEPARSING = 0x80018001,
        PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
        DESKTOPABSOLUTEPARSING = 0x80028000,
        PARENTRELATIVEEDITING = 0x80031001,
        DESKTOPABSOLUTEEDITING = 0x8004c000,
        FILESYSPATH = 0x80058000,
        URL = 0x80068000
    }

    private const uint STGM_READ = 0;
    private const int MAX_PATH = 260;

    [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
    internal static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags,
        StringBuilder lpszPath);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern int SHCreateItemFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        IntPtr pbc,
        ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);

    public  static async Task<string> GetLocalizedName(string path)
    {
        var shellItemType = ShellItemTypeConstants.ShellItemGuid;
        var retCode = SHCreateItemFromParsingName(path, IntPtr.Zero, ref shellItemType, out var shellItem);
        if (retCode != 0) return string.Empty;
        //shellItem.GetAttributes(SIGDN.URL,out  var attributes);
        shellItem.GetDisplayName(SIGDN.NORMALDISPLAY, out var filename);
        return filename;
    }

    public static string ResolveShortcut(string filename)
    {
        var link = new ShellLink();
        ((IPersistFile)link).Load(filename, STGM_READ);
        // TODO: if I can get hold of the hwnd call resolve first. This handles moved and renamed files.  
        // ((IShellLinkW)link).Resolve(hwnd, 0) 
        var sb = new StringBuilder(MAX_PATH);
        var data = new WIN32_FIND_DATAW();
        //((IShellLinkW)link).GetShowCmd
        ((IShellLinkW)link).GetPath(sb, sb.Capacity, out data, 0);
        return string.IsNullOrEmpty(sb.ToString()) ? filename : sb.ToString();
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    public interface IShellItem
    {
        void BindToHandler(
            IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)] Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppv);

        void GetParent(out IShellItem ppsi);

        void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

        void Compare(IShellItem psi, uint hint, out int piOrder);
    }

    [Flags]
    private enum SLGP_FLAGS
    {
        /// <summary>Retrieves the standard short (8.3 format) file name</summary>
        SLGP_SHORTPATH = 0x1,

        /// <summary>Retrieves the Universal Naming Convention (UNC) path name of the file</summary>
        SLGP_UNCPRIORITY = 0x2,

        /// <summary>
        ///     Retrieves the raw path name. A raw path is something that might not exist and may include environment
        ///     variables that need to be expanded
        /// </summary>
        SLGP_RAWPATH = 0x4
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct WIN32_FIND_DATAW
    {
        public readonly uint dwFileAttributes;
        public readonly long ftCreationTime;
        public readonly long ftLastAccessTime;
        public readonly long ftLastWriteTime;
        public readonly uint nFileSizeHigh;
        public readonly uint nFileSizeLow;
        public readonly uint dwReserved0;
        public readonly uint dwReserved1;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string cFileName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public readonly string cAlternateFileName;
    }

    [Flags]
    private enum SLR_FLAGS
    {
        /// <summary>
        ///     Do not display a dialog box if the link cannot be resolved. When SLR_NO_UI is set,
        ///     the high-order word of fFlags can be set to a time-out value that specifies the
        ///     maximum amount of time to be spent resolving the link. The function returns if the
        ///     link cannot be resolved within the time-out duration. If the high-order word is set
        ///     to zero, the time-out duration will be set to the default value of 3,000 milliseconds
        ///     (3 seconds). To specify a value, set the high word of fFlags to the desired time-out
        ///     duration, in milliseconds.
        /// </summary>
        SLR_NO_UI = 0x1,

        /// <summary>Obsolete and no longer used</summary>
        SLR_ANY_MATCH = 0x2,

        /// <summary>
        ///     If the link object has changed, update its path and list of identifiers.
        ///     If SLR_UPDATE is set, you do not need to call IPersistFile::IsDirty to determine
        ///     whether or not the link object has changed.
        /// </summary>
        SLR_UPDATE = 0x4,

        /// <summary>Do not update the link information</summary>
        SLR_NOUPDATE = 0x8,

        /// <summary>Do not execute the search heuristics</summary>
        SLR_NOSEARCH = 0x10,

        /// <summary>Do not use distributed link tracking</summary>
        SLR_NOTRACK = 0x20,

        /// <summary>
        ///     Disable distributed link tracking. By default, distributed link tracking tracks
        ///     removable media across multiple devices based on the volume name. It also uses the
        ///     Universal Naming Convention (UNC) path to track remote file systems whose drive letter
        ///     has changed. Setting SLR_NOLINKINFO disables both types of tracking.
        /// </summary>
        SLR_NOLINKINFO = 0x40,

        /// <summary>Call the Microsoft Windows Installer</summary>
        SLR_INVOKE_MSI = 0x80
    }


    /// <summary>The IShellLink interface allows Shell links to be created, modified, and resolved</summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        /// <summary>Retrieves the path and file name of a Shell link object</summary>
        void GetPath([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath,
            out WIN32_FIND_DATAW pfd, SLGP_FLAGS fFlags);

        /// <summary>Retrieves the list of item identifiers for a Shell link object</summary>
        void GetIDList(out IntPtr ppidl);

        /// <summary>Sets the pointer to an item identifier list (PIDL) for a Shell link object.</summary>
        void SetIDList(IntPtr pidl);

        /// <summary>Retrieves the description string for a Shell link object</summary>
        void GetDescription([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);

        /// <summary>Sets the description for a Shell link object. The description can be any application-defined string</summary>
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

        /// <summary>Retrieves the name of the working directory for a Shell link object</summary>
        void GetWorkingDirectory([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);

        /// <summary>Sets the name of the working directory for a Shell link object</summary>
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

        /// <summary>Retrieves the command-line arguments associated with a Shell link object</summary>
        void GetArguments([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);

        /// <summary>Sets the command-line arguments for a Shell link object</summary>
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

        /// <summary>Retrieves the hot key for a Shell link object</summary>
        void GetHotkey(out short pwHotkey);

        /// <summary>Sets a hot key for a Shell link object</summary>
        void SetHotkey(short wHotkey);

        /// <summary>Retrieves the show command for a Shell link object</summary>
        void GetShowCmd(out int piShowCmd);

        /// <summary>Sets the show command for a Shell link object. The show command sets the initial show state of the window.</summary>
        void SetShowCmd(int iShowCmd);

        /// <summary>Retrieves the location (path and index) of the icon for a Shell link object</summary>
        void GetIconLocation([Out] [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cchIconPath, out int piIcon);

        /// <summary>Sets the location (path and index) of the icon for a Shell link object</summary>
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

        /// <summary>Sets the relative path to the Shell link object</summary>
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);

        /// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed</summary>
        void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

        /// <summary>Sets the path and file name of a Shell link object</summary>
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [Guid("0000010c-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersist
    {
        [PreserveSig]
        void GetClassID(out Guid pClassID);
    }


    [ComImport]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersistFile : IPersist
    {
        new void GetClassID(out Guid pClassID);

        [PreserveSig]
        int IsDirty();

        [PreserveSig]
        void Load([In] [MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

        [PreserveSig]
        void Save([In] [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [In] [MarshalAs(UnmanagedType.Bool)] bool fRemember);

        [PreserveSig]
        void SaveCompleted([In] [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        [PreserveSig]
        void GetCurFile([In] [MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
    }

    // CLSID_ShellLink from ShlGuid.h 
    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    public class ShellLink
    {
    }

    public static class ShellItemTypeConstants
    {
        /// <summary>
        ///     Guid for type IShellItem.
        /// </summary>
        public static readonly Guid ShellItemGuid = new("43826d1e-e718-42ee-bc55-a1e261c37bfe");

        /// <summary>
        ///     Guid for type IShellItem2.
        /// </summary>
        public static readonly Guid ShellItem2Guid = new("7E9FB0D3-919F-4307-AB2E-9B1860310C93");
    }
}