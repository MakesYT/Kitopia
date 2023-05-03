using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs;
using Kitopia.SDKs.Everything;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Kitopia.Core.ViewModel
{
    public partial class SearchViewModel : ObservableRecipient
    {
        
        public SearchViewModel()
        {


            try
            {
                foreach (string file in Directory.EnumerateFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk"))
                {
                    AppSolver(file);
                }
                foreach (string path in Directory.EnumerateDirectories("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*", System.IO.SearchOption.AllDirectories))
                {
                    foreach (string file in Directory.EnumerateFiles(path, "*.lnk"))
                    {
                        AppSolver(file);
                    }
                    foreach (string file in Directory.EnumerateFiles(path, "*.appref-ms"))
                    {
                        AppSolver(file);
                    }
                }
                foreach (string path in Directory.EnumerateDirectories(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "*", System.IO.SearchOption.AllDirectories))
                {
                    foreach (string file in Directory.EnumerateFiles(path, "*.lnk"))
                    {
                        AppSolver(file);
                    }
                    foreach (string file in Directory.EnumerateFiles(path, "*.appref-ms"))
                    {
                        AppSolver(file);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        private void AppSolver(string file)
        {
            FileInfo fileInfo = new FileInfo(file);

            FileInfo refFileInfo = new FileInfo(LnkSolver.ResolveShortcut(file));
            if (refFileInfo.Exists)
                if (refFileInfo.Extension != ".url" && refFileInfo.Extension != ".txt" && refFileInfo.Extension != ".chm" && !fileInfo.Name.Contains("install") && !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
                {

                    List<string> keys = new List<string>();
                    keys = nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
                    keys = nameSolver(keys, refFileInfo.Name.Replace(".exe", ""));
                    //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                    collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = LnkSolver.GetLocalizedName(file), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });

                }

            //Console.WriteLine();
        }
        private List<string> nameSolver(List<string> keys, string name)
        {
            string pattern = @"([a-zA-Z0-9.]+)|([\u4e00-\u9fa5]+)"; //匹配英文数字或中文
                                                                    //匹配非中文或中文
            MatchCollection matches = Regex.Matches(name, pattern);
            foreach (Match match in matches)
            {
                string a2 = match.Groups[1].Value;
                string a3 = match.Groups[2].Value;
                if (!String.IsNullOrWhiteSpace(a2))
                {

                    keys.Add(a2);//PowerPoint
                    keys.Add(a2.ToLower());//powerpoint
                    keys.Add(a2.ToUpper());//POWERPOINT
                    string a1 = Regex.Replace(a2, "[^A-Z]", "");
                    keys.Add(a1);//PP
                    keys.Add(a1 + a2.Last());//PPt
                    keys.Add(a1 + a2.Last().ToString().ToUpper());//PPT
                    keys.Add((a1 + a2.Last()).ToLower());//ppt
                }
                else if (!String.IsNullOrWhiteSpace(a3))
                {

                    keys.Add(NPinyin.Pinyin.GetInitials(a3).ToLower());
                    keys.Add(NPinyin.Pinyin.GetInitials(a3).ToUpper());
                    keys.Add(NPinyin.Pinyin.GetPinyin(a3).Replace(" ", "").ToUpper());
                    keys.Add(NPinyin.Pinyin.GetPinyin(a3).Replace(" ", "").ToLower());
                }

            }
            return keys;
        }
        [ObservableProperty]
        public bool? everythingIsOK;
        [ObservableProperty]
        public string? search;

        List<SearchViewItem> collection = new List<SearchViewItem>();
        partial void OnSearchChanged(string? value)
        {
            if (value == null)
                return;
            if (EverythingIsOK is null)
            {
                if (IntPtr.Size == 8)
                {
                    // 64-bit
                    Everything64.Everything_SetMax(1);
                    EverythingIsOK = Everything64.Everything_QueryW(true);
                }
                else
                {
                    // 32-bit
                    Everything32.Everything_SetMax(1);
                    EverythingIsOK = Everything32.Everything_QueryW(true);
                }

            }


            Items.Clear();

            System.GC.Collect();

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
            foreach (SearchViewItem searchViewItem in collection)
            {
                bool show = false;
                foreach (string key in searchViewItem.keys)
                {
                    if (key.Contains(value))
                        show = true;
                }
                if (show)
                    Items.Add(searchViewItem);
            }
            var sorted = Items.OrderBy(item => item.fileName);
            Items = new ObservableCollection<SearchViewItem>(sorted);


        }

        [ObservableProperty]
        public ObservableCollection<SearchViewItem> items = new();
        public enum ShowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }
        [DllImport("shell32.dll")]
        static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            ShowCommands nShowCmd);
        [ObservableProperty]
        public int? selectedIndex = -1;
        [ObservableProperty]
        public bool can = true;
        [RelayCommand]
        public void CommandByButton()
        {
            Can = false;
        }


        [RelayCommand(CanExecute = nameof(Can))]
        public void OpenFile(SearchViewItem searchViewItem)
        {

            if (SelectedIndex != -1)
                ShellExecute(IntPtr.Zero, "open", searchViewItem.fileInfo.FullName, "", "", ShowCommands.SW_SHOWNORMAL);

            SelectedIndex = -1;
            OnSearchChanged(Search);

        }
        [RelayCommand]
        public void OpenFolder(SearchViewItem searchViewItem)
        {

            ShellExecute(IntPtr.Zero, "open", searchViewItem.fileInfo.DirectoryName, "", "", ShowCommands.SW_SHOWNORMAL);
            SelectedIndex = -1;
            OnSearchChanged(Search);
            Can = true;
        }
        [RelayCommand]
        public void RunAsAdmin(SearchViewItem searchViewItem)
        {

            ShellExecute(IntPtr.Zero, "runas", searchViewItem.fileInfo.FullName, "", "", ShowCommands.SW_SHOWNORMAL);
            SelectedIndex = -1;
            OnSearchChanged(Search);
            Can = true;
        }
        [RelayCommand]
        public void OpenFolderInTerminal(SearchViewItem searchViewItem)
        {
            Process.Start("cmd", "/c cd " + searchViewItem.fileInfo.DirectoryName + " && start cmd.exe");
            SelectedIndex = -1;
            OnSearchChanged(Search);
            Can = true;

        }
    }
    public class SearchViewItem
    {
        public string? fileName { set; get; }
        public bool? IsVisible { set; get; }
        public List<string> keys { set; get; }
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
