using Core.SDKs;
using Core.SDKs.Tools;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace TestProject1;

[TestClass]
public class 文件索引性能
{
    List<SearchViewItem> collection = new List<SearchViewItem>(); //存储本机所有软件
    List<string> names = new List<string>(); //软件去重

    [TestMethod]
    public void GetAllAppskeys()
    {
        // 创建一个空的文件路径集合
        List<string> filePaths = new List<string>();

// 把桌面上的.lnk文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "*.lnk"));
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "*.url"));
// 把开始菜单中的.lnk和.appref-ms文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs", "*.lnk",
            SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs", "*.url",
            SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
            "*.appref-ms", SearchOption.AllDirectories));

// 把自定义集合中的文件路径添加到集合中
        //filePaths.AddRange(ConfigManger.Config.customCollections);

// 把程序文件夹中的.lnk和.appref-ms文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.lnk", SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.appref-ms", SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.url", SearchOption.AllDirectories));
        foreach (var file in filePaths)
        {
            var localizedName = Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(file)
                .GetDisplayName(Shell32.SIGDN.SIGDN_NORMALDISPLAY);
            var fileInfo = new FileInfo(file);
            if (fileInfo.Extension != ".url")
            {
                //var sb = new StringBuilder(260);
                // var shellLink = new ShellLink(file, LinkResolution.None);

                var refFileInfo = new FileInfo(new ShellLink(file).TargetPath);
                if (refFileInfo.Exists)
                {
                    if (names.Contains(refFileInfo.FullName))
                    {
                        return;
                    }
                }

                names.Add(refFileInfo.FullName);
                if (refFileInfo.Extension != ".url" && refFileInfo.Extension != ".txt" &&
                    refFileInfo.Extension != ".chm" &&
                    !refFileInfo.Name.Contains("powershell.exe") && !refFileInfo.Name.Contains("cmd.exe") &&
                    refFileInfo.Extension != ".pdf" && refFileInfo.Extension != ".bat" &&
                    !fileInfo.Name.Contains("install") &&
                    !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
                {
                    var keys = new HashSet<string>();

                    //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                    var localName = localizedName;
                    AppTools.NameSolver(keys, localName).Wait();
                    //nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
                    AppTools.NameSolver(keys, refFileInfo.Name.Replace(".exe", "")).Wait();

                    {
                        lock (collection)
                        {
                            collection.Add(new SearchViewItem
                            {
                                Keys = keys, IsVisible = true, FileInfo = refFileInfo, FileName = localName,
                                OnlyKey = refFileInfo.FullName,
                                FileType = FileType.应用程序, Icon = null
                            });
                        }
                    }
                }
                else
                {
                    if (file.Contains("Control.url"))
                    {
                    }
                }
            }
        }
    }
}