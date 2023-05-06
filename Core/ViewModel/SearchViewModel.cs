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
        private List<SearchViewItem> collection = new List<SearchViewItem>();//存储本机所有软件
        private List<string> names = new List<string>();//软件去重
        [ObservableProperty]
        public ObservableCollection<SearchViewItem> items = new();//搜索界面显示的软件
        public SearchViewModel()
        {
            AppSolver.GetAllApps(ref collection, ref names);

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
                
                Items.Add(searchViewItem) ;
            }
            var sorted = Items.OrderByDescending(item => item.weight);
            Items = new ObservableCollection<SearchViewItem>(sorted);


        }
        
        

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
