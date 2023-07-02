﻿using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using Core.SDKs.Everything;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using log4net;
using NPinyin;
using Vanara.PInvoke;

namespace Core.SDKs.Tools;

public partial class AppTools
{
    private static readonly ILog log = LogManager.GetLogger(nameof(AppTools));
    private static readonly List<string> ErrorLnkList = new();

    public static void DelNullFile(List<SearchViewItem> collection, List<string> names)
    {
        foreach (var searchViewItem in collection)
        {
            switch (searchViewItem.FileType)
            {
                case FileType.文件:
                case FileType.Excel文档:
                case FileType.Word文档:
                case FileType.PDF文档:
                case FileType.PPT文档:
                {
                    if (!File.Exists(searchViewItem.OnlyKey))
                    {
                        collection.Remove(searchViewItem);
                        names.Remove(searchViewItem.OnlyKey);
                    }

                    break;
                }
                case FileType.文件夹:
                {
                    if (!Directory.Exists(searchViewItem.OnlyKey))
                    {
                        collection.Remove(searchViewItem);
                        names.Remove(searchViewItem.OnlyKey);
                    }

                    break;
                }
            }
        }
    }

    public static void GetAllApps(List<SearchViewItem> collection, List<string> names, bool logging = false)
    {
        log.Debug("索引全部软件及收藏项目");
        UWPAPPsTools.GetAll(collection, names);

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
        filePaths.AddRange(ConfigManger.Config.customCollections);

// 把程序文件夹中的.lnk和.appref-ms文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.lnk", SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.appref-ms", SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.url", SearchOption.AllDirectories));
        var options = new ParallelOptions();
        options.MaxDegreeOfParallelism = 256;
// 使用Parallel.ForEach并行执行AppSolverA方法
        List<Task> list = new();

        Parallel.ForEach(filePaths, options, file =>
        {
            if (logging)
            {
                log.Debug("索引:" + file);
            }

            list.Add(AppSolverA(collection, names, file, logging: logging));
        });

        try
        {
            Task.WaitAll(list.ToArray());
        }
        catch (Exception e)
        {
        }


        if (ErrorLnkList.Any())
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
        bool star = false, bool logging = false)
    {
        var localizedName = Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(file)
            .GetDisplayName(Shell32.SIGDN.SIGDN_NORMALDISPLAY);

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
                await NameSolver(keys, localizedName);
                lock (collection)
                {
                    collection.Add(new SearchViewItem()
                    {
                        FileInfo = new FileInfo(file),
                        FileName = "打开文件:" + localizedName + "?",
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
        if (fileInfo.Extension != ".url")
        {
            //var sb = new StringBuilder(260);
            // var shellLink = new ShellLink(file, LinkResolution.None);
            var link = new Shell32.IShellLinkW();
            ((IPersistFile)link).Load(file, (int)STGM.STGM_READ);
            var sb = new StringBuilder(260);
            var data = new WIN32_FIND_DATA();
            //((IShellLinkW)link).GetShowCmd
            ((Shell32.IShellLinkW)link).GetPath(sb, sb.Capacity, out data, 0);

            var targetPath = sb.ToString() ?? file;

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                targetPath = file;
            }

            if (targetPath.Contains("Everything.exe") && ConfigManger.Config.autoStartEverything)
            {
                var isRun = false;
                if (IntPtr.Size == 8)
                {
                    // 64-bit
                    Everything64.Everything_SetMax(1);
                    isRun = Everything64.Everything_QueryW(true);
                }
                else
                {
                    // 32-bit
                    Everything32.Everything_SetMax(1);
                    isRun = Everything32.Everything_QueryW(true);
                }

                if (!isRun)
                {
                    log.Debug("自动启动Everything");
                    Shell32.ShellExecute(IntPtr.Zero, "open", file, "-startup", "",
                        ShowWindowCommand.SW_HIDE);
                }
            }

            var refFileInfo = new FileInfo(targetPath);
            if (refFileInfo.Exists)
            {
                if (names.Contains(refFileInfo.FullName))
                {
                    if (logging)
                    {
                        log.Debug("重复索引:" + file);
                    }


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
                refFileInfo.Extension != ".pdf" && refFileInfo.Extension != ".bat" &&
                !fileInfo.Name.Contains("install") &&
                !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
            {
                var keys = new HashSet<string>();

                //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                var localName = localizedName;
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
                if (file.Contains("Control.url"))
                {
                }

                log.Debug("不符合要求跳过索引:" + file);
            }
        }
        else
        {
            if (names.Contains(file))
            {
                if (logging)
                {
                    log.Debug("重复索引:" + file);
                }

                return;
            }

            names.Add(file);
            string url = "";
            string relFile = "";
            string fileContent = File.ReadAllText(file); // read the file content
            string pattern = @"URL=(.*)"; // the regex pattern to match the url
            Match match = Regex.Match(fileContent, pattern); // match the pattern
            if (match.Success) // if a match is found
            {
                url = match.Groups[1].Value.Replace("\r", ""); // get the url from the first group
            }


            string pattern2 = @"IconFile=(.*)"; // the regex pattern to match the url
            Match match2 = Regex.Match(fileContent, pattern2); // match the pattern
            if (match2.Success) // if a match is found
            {
                relFile = match2.Groups[1].Value.Replace("\r", ""); // get the url from the first group
            }

            if (string.IsNullOrWhiteSpace(relFile))
            {
                return;
            }

            var keys = new HashSet<string>();

            //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
            var localName = localizedName;
            await NameSolver(keys, localName);
            //nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
            await NameSolver(keys, fileInfo.Name.Replace(".url", ""));

            {
                lock (collection)
                {
                    collection.Add(new SearchViewItem
                    {
                        Keys = keys, IsVisible = true, FileName = localName,
                        OnlyKey = url, Url = url, FileInfo = new FileInfo(relFile),
                        FileType = FileType.URL, Icon = null
                    });
                }
            }

            log.Debug("完成索引:" + file);
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