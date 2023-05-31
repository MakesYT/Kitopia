using System.Text;
using Core.SDKs.Config;
using Core.SDKs.Services;
using log4net;
using NPinyin;

namespace Core.SDKs.Tools;

public partial class AppTools
{
    private static readonly ILog log = LogManager.GetLogger(nameof(AppTools));
    private static readonly List<string> ErrorLnkList = new();

    public static void GetAllApps(List<SearchViewItem> collection, List<string> names)
    {
        if (ConfigManger.config.debugMode)
        {
            log.Debug("索引全部软件及收藏项目");
        }

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
        if (ErrorLnkList.Any())
        {
            StringBuilder c = new StringBuilder("检测到多个无效的快捷方式\n需要Kitopia帮你清理吗?(该功能每个错误快捷方式只提示一次)\n以下为无效的快捷方式列表:\n");
            foreach (string s in ErrorLnkList)
            {
                c.AppendLine(s);
            }

            log.Debug(c.ToString());
            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).showMessageBox("Kitopia建议",
                c.ToString(),
                (() =>
                {
                    foreach (var s in ErrorLnkList)
                    {
                        log.Debug("删除无效快捷方式:" + s);
                        try
                        {
                            File.Delete(s);
                        }
                        catch (Exception e)
                        {
                            log.Debug("添加无效快捷方式记录:" + s);
                            ConfigManger.config.errorLnk.Add(s);
                            ConfigManger.Save();
                        }
                    }

                    ErrorLnkList.Clear();
                }), (() =>
                {
                    foreach (var s in ErrorLnkList)
                    {
                        log.Debug("添加无效快捷方式记录:" + s);
                        ConfigManger.config.errorLnk.Add(s);
                        ConfigManger.Save();
                    }

                    log.Debug("取消删除无效快捷方式");
                    ErrorLnkList.Clear();
                }));
        }
    }

    public static void AppSolverA(List<SearchViewItem> collection, List<string> names, string file, bool star = false)
    {
        if (ConfigManger.config.debugMode)
        {
            log.Debug("索引:" + file);
        }

        if (star)
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("索引为收藏项目:" + file);
            }

            if (names.Contains(file))
            {
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("重复跳过索引:" + file);
                }

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

            if (ConfigManger.config.debugMode)
            {
                log.Debug("完成索引:" + file);
            }

            return;
        }

        var fileInfo = new FileInfo(file);
        var refFileInfo = new FileInfo(LnkTools.ResolveShortcut(file));
        if (refFileInfo.Exists)
        {
            if (names.Contains(refFileInfo.FullName))
            {
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("重复索引:" + file);
                }

                return;
            }
        }
        else
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("无效索引:" + file);
            }

            if (!ErrorLnkList.Contains(file) && !ConfigManger.config.errorLnk.Contains(file))
            {
                ErrorLnkList.Add(file);
            }

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
            if (ConfigManger.config.debugMode)
            {
                log.Debug("完成索引:" + file);
            }
        }
        else
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("不符合要求跳过索引:" + file);
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

    public static List<string> GetErrorLnks()
    {
        return ErrorLnkList;
    }
}