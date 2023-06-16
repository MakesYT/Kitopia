using System.Text;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using log4net;
using NPinyin;

namespace Core.SDKs.Tools;

public partial class AppTools
{
    private static readonly ILog log = LogManager.GetLogger(nameof(AppTools));
    private static readonly List<string> ErrorLnkList = new();

    public static void GetAllApps(List<SearchViewItem> collection, List<string> names)
    {
        log.Debug("索引全部软件及收藏项目");


        // 创建一个空的文件路径集合
        List<string> filePaths = new List<string>();

// 把桌面上的.lnk文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "*.lnk"));

// 把开始菜单中的.lnk和.appref-ms文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs", "*.lnk",
            SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
            "*.appref-ms", SearchOption.AllDirectories));

// 把自定义集合中的文件路径添加到集合中
        filePaths.AddRange(ConfigManger.Config.customCollections);

// 把程序文件夹中的.lnk和.appref-ms文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.lnk", SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.appref-ms", SearchOption.AllDirectories));
        var options = new ParallelOptions();
        options.MaxDegreeOfParallelism = 256;
// 使用Parallel.ForEach并行执行AppSolverA方法
        List<Task> list = new();

        Parallel.ForEach(filePaths, options, file =>
        {
            list.Add(AppSolverA(collection, names, file));
        });
        Task.WaitAll(list.ToArray());

        if (ErrorLnkList.Any())
            if (false)

            {
                StringBuilder c = new StringBuilder("检测到多个无效的快捷方式\n需要Kitopia帮你清理吗?(该功能每个错误快捷方式只提示一次)\n以下为无效的快捷方式列表:\n");
                foreach (var s in ErrorLnkList)
                {
                    c.AppendLine(s);
                }

                log.Debug(c.ToString());
                ((IToastService)ServiceManager.Services!.GetService(typeof(IToastService))!).showMessageBox("Kitopia建议",
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
                            catch (Exception)
                            {
                                log.Debug("添加无效快捷方式记录:" + s);
                                ConfigManger.Config.errorLnk.Add(s);
                                ConfigManger.Save();
                            }
                        }

                        ErrorLnkList.Clear();
                    }), (() =>
                    {
                        foreach (var s in ErrorLnkList)
                        {
                            log.Debug("添加无效快捷方式记录:" + s);
                            ConfigManger.Config.errorLnk.Add(s);
                            ConfigManger.Save();
                        }

                        log.Debug("取消删除无效快捷方式");
                        ErrorLnkList.Clear();
                    }));
            }
    }

    public static async Task AppSolverA(List<SearchViewItem> collection, List<string> names, string file,
        bool star = false)
    {
        //log.Debug("索引:" + file);


        if (star)
        {
            log.Debug("索引为收藏项目:" + file);


            if (names.Contains(file))
            {
                log.Debug("重复跳过索引:" + file);


                return;
            }

            names.Add(file);

            if (Path.HasExtension(file) && File.Exists(file))
            {
                var keys = new HashSet<string>();
                await NameSolver(keys, LnkTools.GetLocalizedName(file));
                lock (collection)
                {
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
            }
            else if (Directory.Exists(file))
            {
                var keys = new HashSet<string>();
                await NameSolver(keys, file.Split("\\").Last());
                lock (collection)
                {
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
            }


            log.Debug("完成索引:" + file);


            return;
        }

        var fileInfo = new FileInfo(file);

        var refFileInfo = new FileInfo(LnkTools.ResolveShortcut(file));
        if (refFileInfo.Exists)
        {
            if (names.Contains(refFileInfo.FullName))
            {
                //log.Debug("重复索引:" + file);


                return;
            }
        }
        else
        {
            log.Debug("无效索引:\n" + file + "\n目标位置:" + refFileInfo.FullName);
            if (!ErrorLnkList.Contains(file) && !ConfigManger.Config.errorLnk.Contains(file))
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
            await NameSolver(keys, localName);
            //nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
            await NameSolver(keys, refFileInfo.Name.Replace(".exe", ""));

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

            log.Debug("完成索引:" + file);
        }
        else
        {
            log.Debug("不符合要求跳过索引:" + file);
        }
    }


    //Console.WriteLine();
    private static void CreateCombinations(HashSet<string> keys, int startIndex, string pair, string[] initialArray)
    {
        // 遍历初始数组中的元素
        for (var i = startIndex; i < initialArray.Length; i++)
        {
            // 使用StringBuilder来拼接字符串
            var value = new StringBuilder(pair).Append(initialArray[i]).ToString();

            // 使用ToLowerInvariant方法来统一字符串的大小写
            var lowerValue = value.ToLowerInvariant();

            // 添加原始字符串、拼音首字母、拼音全拼和去除非字母字符后的字符串到HashSet中
            AddUtil(keys, lowerValue);
            AddUtil(keys, Pinyin.GetInitials(value).ToLowerInvariant());
            AddUtil(keys, Pinyin.GetPinyin(value).ToLowerInvariant());
            AddUtil(keys, MyRegex().Replace(lowerValue, ""));

            // 递归调用自身方法，生成更长的字符串组合
            CreateCombinations(keys, i + 1, value, initialArray);
        }
    }


    // 使用const或readonly修饰符来声明pattern字符串
    public static async Task NameSolver(HashSet<string> keys, string name)
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
        if (string.IsNullOrEmpty(name) || name.Length <= 1)
        {
            return;
        }


        keys.Add(name);
    }

    [System.Text.RegularExpressions.GeneratedRegex("[^A-Z]")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();

    public static List<string> GetErrorLnks()
    {
        return ErrorLnkList;
    }
}