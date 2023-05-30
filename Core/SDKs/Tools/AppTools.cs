using Core.SDKs.Config;
using NPinyin;
using static System.Text.RegularExpressions.Regex;

namespace Core.SDKs.Tools;

public partial class AppTools
{
    public static void GetAllApps(List<SearchViewItem> collection, List<string> names)
    {
        List<Task> taskList = new List<Task>();

        foreach (var file in Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                     "*.lnk"))
            taskList.Add(Task.Run(() =>
            {
                AppSolverA(collection, names, file);
            }));

        foreach (var file in Directory.EnumerateFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs",
                     "*.lnk"))
            taskList.Add(Task.Run(() =>
            {
                AppSolverA(collection, names, file);
            }));
        foreach (var path in Directory.EnumerateDirectories("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs",
                     "*", SearchOption.AllDirectories))
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.lnk"))
                taskList.Add(Task.Run(() =>
                {
                    AppSolverA(collection, names, file);
                }));
            foreach (var file in Directory.EnumerateFiles(path, "*.appref-ms"))
                taskList.Add(Task.Run(() =>
                {
                    AppSolverA(collection, names, file);
                }));
        }

        foreach (var file in ConfigManger.config.customCollections)
        {
            taskList.Add(Task.Run(() =>
            {
                AppSolverA(collection, names, file, true);
            }));
        }

        foreach (var path in Directory.EnumerateDirectories(
                     Environment.GetFolderPath(Environment.SpecialFolder.Programs), "*", SearchOption.AllDirectories))
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.lnk"))
                taskList.Add(Task.Run(() =>
                {
                    AppSolverA(collection, names, file);
                }));
            foreach (var file in Directory.EnumerateFiles(path, "*.appref-ms"))
                taskList.Add(Task.Run(() =>
                {
                    AppSolverA(collection, names, file);
                }));
        }

        Task.WaitAll(taskList.ToArray());
    }

    public static void AppSolverA(List<SearchViewItem> collection, List<string> names, string file, bool star = false)
    {
        if (star)
        {
            if (names.Contains(file))
            {
                return;
            }

            names.Add(file);
            if (Path.HasExtension(file) && File.Exists(file))
            {
                var keys = new HashSet<string>();
                NameSolver(keys, LnkTools.GetLocalizedName(file));
                collection.Add(new SearchViewItem()
                {
                    FileInfo = new FileInfo(file),
                    FileName = "打开文件:" + LnkTools.GetLocalizedName(file) + "?",
                    FileType = FileType.文件,
                    OnlyKey = file,
                    Keys = keys,
                    IsStared = true,
                    IsVisible = true
                });
            }
            else if (Directory.Exists(file))
            {
                var keys = new HashSet<string>();
                NameSolver(keys, file.Split("\\").Last());
                collection.Add(new SearchViewItem()
                {
                    DirectoryInfo = new DirectoryInfo(file),
                    FileName = "打开" + file.Split("\\").Last() + "?",
                    FileType = FileType.文件夹,
                    IsStared = true,
                    OnlyKey = file,
                    Keys = keys,
                    Icon = null,
                    IsVisible = true
                });
            }

            return;
        }

        var fileInfo = new FileInfo(file);
        var refFileInfo = new FileInfo(LnkTools.ResolveShortcut(file));
        if (refFileInfo.Exists)
            if (names.Contains(refFileInfo.FullName))
            {
                return;
            }

        names.Add(refFileInfo.FullName);
        if (refFileInfo.Extension != ".url" && refFileInfo.Extension != ".txt" && refFileInfo.Extension != ".chm" &&
            !refFileInfo.Name.Contains("powershell.exe") && !refFileInfo.Name.Contains("cmd.exe") &&
            refFileInfo.Extension != ".pdf" && refFileInfo.Extension != ".bat" && !fileInfo.Name.Contains("install") &&
            !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
        {
            var keys = new HashSet<string>();

            //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
            var localName = LnkTools.GetLocalizedName(file);
            NameSolver(keys, localName);
            //nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
            NameSolver(keys, refFileInfo.Name.Replace(".exe", ""));

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
    }


    //Console.WriteLine();
    private static void CreateCombinations(HashSet<string> keys, int startIndex, string pair, string[] initialArray)
    {
        for (var i = startIndex; i < initialArray.Length; i++)
        {
            var value = $"{pair}{initialArray[i]}";
            AddUtil(keys, value.ToLowerInvariant());
            AddUtil(keys, Pinyin.GetInitials(value).Replace(" ", "").ToLowerInvariant());
            AddUtil(keys, Pinyin.GetPinyin(value).Replace(" ", "").ToLowerInvariant());
            AddUtil(keys, MyRegex().Replace(value, "").ToLowerInvariant());
            CreateCombinations(keys, i + 1, value, initialArray);
        }
    }


    // 使用const或readonly修饰符来声明pattern字符串
    public static void NameSolver(HashSet<string> keys, string name)
    {
        var initials = name.Split(" ");
        CreateCombinations(keys, 0, "", initials);

        AddUtil(keys, Pinyin.GetInitials(name).Replace(" ", "").ToLowerInvariant());
        AddUtil(keys, Pinyin.GetPinyin(name).Replace(" ", "").ToLowerInvariant());
        AddUtil(keys,
            string.Concat(MyRegex().Replace(name, "").ToLowerInvariant(), name.Last().ToString().ToLowerInvariant()));
    }

    private static void AddUtil(HashSet<string> keys, string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        if (name.Length <= 1) return;

        keys.Add(name);
    }

    [System.Text.RegularExpressions.GeneratedRegex("[^A-Z]")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}