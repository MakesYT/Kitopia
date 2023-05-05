using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs;
using Kitopia.SDKs.Everything;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Kitopia.Core.ViewModel
{
    public partial class SearchViewModel : ObservableRecipient
    {

        public SearchViewModel()
        {


            try
            {
                foreach (string file in Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.lnk"))
                {
                    AppSolver(file);
                }
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

        List<SearchViewItem> collection = new List<SearchViewItem>();
        List<string> names = new List<string>();
        private void AppSolver(string file)
        {
            FileInfo fileInfo = new FileInfo(file);

            FileInfo refFileInfo = new FileInfo(LnkSolver.ResolveShortcut(file));
            if (refFileInfo.Exists)
                if (refFileInfo.Extension != ".url" && refFileInfo.Extension != ".txt" && refFileInfo.Extension != ".chm" && refFileInfo.Extension != ".pdf" && !fileInfo.Name.Contains("install") && !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
                {

                    List<string> keys = new List<string>();
                    
                    //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                    string localName = LnkSolver.GetLocalizedName(file);
                    keys = nameSolver(keys, localName);
                    keys = nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
                    keys = nameSolver(keys, refFileInfo.Name.Replace(".exe", ""));
                    if (!names.Contains(localName))
                    {
                        names.Add(localName);
                        collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = localName, fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });

                    }

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

        partial void OnSearchChanged(string? value)
        {

            if (value==null||value=="")
            {
                Items.Clear();
                System.GC.Collect();
                return;
            }
                

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




            Items.Clear();
            Kitopia.SDKs.Everything.Tools.main(ref items, value);
            System.GC.Collect();


            foreach ( SearchViewItem searchViewItem in collection)
            {
                bool show = false;
                foreach (string key in searchViewItem.keys)
                {
                    if (!String.IsNullOrEmpty(key))
                    {
                        if (key.Contains(value))
                        {
                            show = true;
                            searchViewItem.weight += 1;

                        }
                        if (key.StartsWith(value))
                        {
                            show = true;
                            searchViewItem.weight += 500;

                        }
                        if (key.Equals(value))
                        {
                            show = true;
                            searchViewItem.weight += 1000;

                        }
                    }
                    
                }
                searchViewItem.weight= searchViewItem.weight/searchViewItem.keys.Count();
                Items.Add(searchViewItem);
            }
            var sorted = Items.OrderByDescending(item => item.weight);
            Items = new ObservableCollection<SearchViewItem>(sorted);


        }
        
        [ObservableProperty]
        public ObservableCollection<SearchViewItem> items = new();

        [ObservableProperty]
        public int? selectedIndex = -1;



        [RelayCommand]
        public void OpenFile(SearchViewItem searchViewItem)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", searchViewItem.fileInfo.FullName, "", "", ShellTools.ShowCommands.SW_SHOWNORMAL);
            Search = "";
        }
        [RelayCommand]
        public void OpenFolder(SearchViewItem searchViewItem)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", searchViewItem.fileInfo.DirectoryName, "", "", ShellTools.ShowCommands.SW_SHOWNORMAL);
            Search = "";
        }
        [RelayCommand]
        public void RunAsAdmin(SearchViewItem searchViewItem)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "runas", searchViewItem.fileInfo.FullName, "", "", ShellTools.ShowCommands.SW_SHOWNORMAL);
            Search = "";
        }
        [RelayCommand]
        public void OpenFolderInTerminal(SearchViewItem searchViewItem)
        {
            Process.Start("cmd", "/c cd " + searchViewItem.fileInfo.DirectoryName + " && start cmd.exe");
            Search = "";
        }
    }


}
