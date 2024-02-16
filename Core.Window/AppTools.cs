#region

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using NPinyin;
using PluginCore;
using Vanara.PInvoke;
using File = System.IO.File;

#endregion

namespace Core.SDKs.Tools;

internal partial class AppTools
{
    private static readonly ILog log = LogManager.GetLogger(nameof(AppTools));
    private static readonly List<string> ErrorLnkList = new();

    internal static void AutoStartEverything(Dictionary<string, SearchViewItem> collection, Action action)
    {
        if (ConfigManger.Config.autoStartEverything)
        {
            if (string.IsNullOrWhiteSpace(ConfigManger.Config.everythingOnlyKey))
            {
                foreach (var (key, value) in collection)
                {
                    if (key.Contains("Everything.exe"))
                    {
                        ConfigManger.Config.everythingOnlyKey = key;
                        ConfigManger.Save();
                        break;
                    }
                }
            }

            if (collection.TryGetValue(ConfigManger.Config.everythingOnlyKey, out var searchViewItem))
            {
                var isRun = ServiceManager.Services.GetService<IEverythingService>().isRun();


                if (!isRun)
                {
                    var 程序名称 = "noUAC.Everything";
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "noUAC\\" + 程序名称 + ".lnk"))
                    {
                        var dialog = new DialogContent()
                        {
                            Title = $"Kitopia提示",
                            Content =
                                $"Kitopia即将使用任务计划来创建绕过UAC启动Everything的快捷方式\n需要确认UAC权限\n按下取消则关闭自动启动功能\n路径:{AppDomain.CurrentDomain.BaseDirectory}noUAC\\{程序名称}.lnk",
                            PrimaryButtonText = "确定",
                            CloseButtonText = "取消",
                            PrimaryAction = () =>
                            {
                                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "noUAC");
                                var TempFileName = AppDomain.CurrentDomain.BaseDirectory + "noUAC\\" + 程序名称 + ".xml";
                                var XML_Text =
                                    $"<?xml version=\"1.0\" encoding=\"UTF-16\"?>\n<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">\n  <Triggers />\n  <Principals>\n    <Principal id=\"Author\">\n      <LogonType>InteractiveToken</LogonType>\n      <RunLevel>HighestAvailable</RunLevel>\n    </Principal>\n  </Principals>\n  <Settings>\n    <MultipleInstancesPolicy>Parallel</MultipleInstancesPolicy>\n    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>\n    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>\n    <AllowHardTerminate>false</AllowHardTerminate>\n    <StartWhenAvailable>false</StartWhenAvailable>\n    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>\n    <IdleSettings>\n      <StopOnIdleEnd>false</StopOnIdleEnd>\n      <RestartOnIdle>false</RestartOnIdle>\n    </IdleSettings>\n    <AllowStartOnDemand>true</AllowStartOnDemand>\n    <Enabled>true</Enabled>\n    <Hidden>false</Hidden>\n    <RunOnlyIfIdle>false</RunOnlyIfIdle>\n    <WakeToRun>false</WakeToRun>\n    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>\n    <Priority>7</Priority>\n  </Settings>\n  <Actions Context=\"Author\">\n    <Exec>{Environment.NewLine}      <Command>\"{searchViewItem.OnlyKey}\"</Command>{Environment.NewLine}      <Arguments>-startup</Arguments>{Environment.NewLine}    </Exec>\n  </Actions>\n</Task>";
                                File.WriteAllText(TempFileName, XML_Text, Encoding.Unicode);

                                Shell32.ShellExecute(IntPtr.Zero, "runas", "schtasks.exe",
                                    $"/create /tn \"{程序名称}\" /xml \"{@TempFileName}\"", "",
                                    ShowWindowCommand.SW_HIDE);

                                // var shell = new WshShell();
                                // var shortcut =
                                //     (IWshShortcut)shell.CreateShortcut(
                                //         $"{AppDomain.CurrentDomain.BaseDirectory}noUAC\\{程序名称}.lnk");
                                // //Debug.Print(Path.GetDirectoryName(Application.ExecutablePath) + @"\" + TextBox_程序名称.Text + ".lnk");
                                // shortcut.TargetPath = "schtasks.exe";
                                // shortcut.Arguments = $"/run /tn \"{程序名称}\"";
                                // shortcut.IconLocation = searchViewItem.OnlyKey + ", 0";
                                // shortcut.WindowStyle = 7;
                                // shortcut.Save();
                                Thread.Sleep(200);
                                File.Delete(TempFileName);
                                log.Debug("创建Everything的noUAC任务计划完成");
                                Shell32.ShellExecute(IntPtr.Zero, "open",
                                    $"{AppDomain.CurrentDomain.BaseDirectory}noUAC\\{程序名称}.lnk", "", "",
                                    ShowWindowCommand.SW_HIDE);
                                action.Invoke();
                            },
                            CloseAction = () =>
                            {
                                log.Debug("关闭自动启动Everything功能");
                                ConfigManger.Config.autoStartEverything = false;
                                ConfigManger.Save();
                            }
                        };
                        ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(
                            null,
                            dialog);
                    }
                    else

                    {
                        Shell32.ShellExecute(IntPtr.Zero, "open",
                            $"{AppDomain.CurrentDomain.BaseDirectory}noUAC\\{程序名称}.lnk", "", "",
                            ShowWindowCommand.SW_HIDE);
                    }
                }
            }
        }
    }

    internal static void DelNullFile(Dictionary<string, SearchViewItem> collection)
    {
        var toRemove = new List<string>();
        foreach (var (key, searchViewItem) in collection)
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
                        toRemove.Add(key);
                        //collection.Remove(searchViewItem);
                    }

                    break;
                }
                case FileType.文件夹:
                {
                    if (!Directory.Exists(searchViewItem.OnlyKey))
                    {
                        toRemove.Add(key);
                        //collection.Remove(searchViewItem);
                    }

                    break;
                }
            }
        }

        foreach (var searchViewItem in toRemove)
        {
            collection.Remove(searchViewItem);
        }
    }

    internal static void GetAllApps(Dictionary<string, SearchViewItem> collection,
        bool logging = false)
    {
        log.Debug("索引全部软件及收藏项目");
        foreach (var customScenario in CustomScenarioManger.CustomScenarios)
        {
            if (customScenario.ExecutionManual)
            {
                if (customScenario.Keys.Any())
                {
                    var onlyKey = $"CustomScenario:{customScenario.UUID}";
                    var viewItem1 = new SearchViewItem()
                    {
                        ItemDisplayName = $"执行自定义情景:{customScenario.Name}",
                        FileType = FileType.自定义情景,
                        OnlyKey = onlyKey,
                        Keys = customScenario.Keys.ToList(),
                        Icon = null,
                        IconSymbol = 0xF78B,
                        IsVisible = true
                    };

                    collection.TryAdd(onlyKey, viewItem1);
                }
            }
        }


        UwpTools.GetAll(collection);


        // 创建一个空的文件路径集合
        List<string> filePaths = new();

// 把桌面上的.lnk文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
        filePaths.AddRange(
            Directory.EnumerateDirectories(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
                "*.lnk",
                SearchOption.AllDirectories));
            filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
                "*.url",
                SearchOption.AllDirectories));
            filePaths.AddRange(Directory.EnumerateFiles(@"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
                "*.appref-ms", SearchOption.AllDirectories));
            filePaths.AddRange(ConfigManger.Config.customCollections);
            filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                "*.lnk", SearchOption.AllDirectories));
            filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                "*.appref-ms", SearchOption.AllDirectories));
            filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                "*.url", SearchOption.AllDirectories));
        }

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

            list.Add(AppSolverA(collection, file, logging: logging));
        });

        try
        {
            Task.WaitAll(list.ToArray());
        }
        catch (Exception e)
        {
        }


        //AutoStartEverything(collection);
        if (ErrorLnkList.Any())
        {
            var c = new StringBuilder("检测到多个无效的快捷方式\n需要Kitopia帮你清理吗?(该功能每个错误快捷方式只提示一次)\n以下为无效的快捷方式列表:\n");
            foreach (var s in ErrorLnkList)
            {
                c.AppendLine(s);
            }

            log.Debug(c.ToString());
            var dialog = new DialogContent()
            {
                Title = $"Kitopia建议",
                Content = c.ToString(),
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消",
                PrimaryAction = () =>
                {
                    foreach (var s in ErrorLnkList)
                    {
                        log.Debug($"删除无效快捷方式:{s}");
                        try
                        {
                            File.Delete(s);
                        }
                        catch (Exception)
                        {
                            log.Debug($"添加无效快捷方式记录:{s}");
                            ConfigManger.Config.errorLnk.Add(s);
                            ConfigManger.Save();
                        }
                    }

                    ErrorLnkList.Clear();
                },
                SecondaryAction = () =>
                {
                    foreach (var s in ErrorLnkList)
                    {
                        log.Debug($"添加无效快捷方式记录:{s}");
                        ConfigManger.Config.errorLnk.Add(s);
                        ConfigManger.Save();
                    }

                    log.Debug("取消删除无效快捷方式");
                    ErrorLnkList.Clear();
                }
            };
            ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(null,
                dialog);
        }
    }

    internal static async Task AppSolverA(Dictionary<string, SearchViewItem> collection, string file,
        bool star = false, bool logging = false)
    {
        //log.Debug(Thread.CurrentThread.ManagedThreadId);
        var localizedName = "";

        try
        {
            localizedName = Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(file)
                .GetDisplayName(Shell32.SIGDN.SIGDN_NORMALDISPLAY);
        }
        catch (Exception e)
        {
            localizedName = "f";
        }

        if (Path.HasExtension(file))
        {
            var fileInfo = new FileInfo(file);
            switch (fileInfo.Extension)
            {
                case ".lnk":
                {
                    //var sb = new StringBuilder(260);
                    // var shellLink = new ShellLink(file, LinkResolution.None);
                    var link = new Shell32.IShellLinkW();
                    ((IPersistFile)link).Load(file, (int)STGM.STGM_READ);
                    var sb = new StringBuilder(260);
                    var data = new WIN32_FIND_DATA();
                    //((IShellLinkW)link).GetShowCmd
                    ((Shell32.IShellLinkW)link).GetPath(sb, sb.Capacity, out data, 0);
                    var argSb = new StringBuilder(260);
                    link.GetArguments(argSb, argSb.Capacity);
                    var arg = argSb.Length > 0 ? argSb.ToString() : null;
                    if (arg != null && arg.Contains('%'))
                    {
                        Regex regex = new Regex("%(\\w+)%");

                        // 替换后的字符串
                        arg = regex.Replace(arg, match =>
                        {
                            // 获取匹配到的环境变量名称
                            var variable = match.Groups[1].Value;

                            // 获取环境变量值
                            var value = Environment.GetEnvironmentVariable(variable);


                            // 返回替换后的值
                            return value;
                        });
                    }

                    var targetPath = sb.ToString() ?? file;

                    if (string.IsNullOrWhiteSpace(targetPath))
                    {
                        targetPath = file;
                    }

                    if (!File.Exists(targetPath))
                    {
                        if (File.Exists(targetPath.Replace("Program Files (x86)", "Program Files")))
                        {
                            targetPath = targetPath.Replace("Program Files (x86)", "Program Files");
                            goto next;
                        }

                        if (File.Exists(targetPath.Replace("Program Files", "Program Files (x86)")))
                        {
                            targetPath = targetPath.Replace("Program Files", "Program Files (x86)");
                            goto next;
                        }

                        if (File.Exists(targetPath.Replace("system32", "sysnative")))
                        {
                            targetPath = targetPath.Replace("system32", "sysnative");
                            goto next;
                        }
                    }

                    next:
                    var refFileInfo = new FileInfo(targetPath);
                    var fullName = refFileInfo.FullName;
                    if (refFileInfo.Exists)
                    {
                        if (collection.ContainsKey(fullName))
                        {
                            if (logging)
                            {
                                log.Debug($"重复索引:{file}");
                            }


                            return;
                        }

                        if (ConfigManger.Config.ignoreItems.Contains(targetPath))
                        {
                            log.Debug($"忽略索引:{targetPath}");
                            return;
                        }
                    }
                    else
                    {
                        log.Debug($"无效索引:\n{file}\n目标位置:{fullName}");
                        if (!ErrorLnkList.Contains(file) && !ConfigManger.Config.errorLnk.Contains(file))
                        {
                            ErrorLnkList.Add(file);
                        }

                        return;
                    }


                    var extension = refFileInfo.Extension;
                    if (extension != ".url" && extension != ".txt" && extension != ".chm" &&
                        !refFileInfo.Name.Contains("powershell.exe") && !refFileInfo.Name.Contains("cmd.exe") &&
                        extension != ".pdf" && extension != ".bat" &&
                        !fileInfo.Name.Contains("install") &&
                        !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
                    {
                        var keys = new List<string>();

                        //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                        var localName = localizedName;
                        await NameSolver(keys, localName);
                        //nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
                        await NameSolver(keys, refFileInfo.Name.Replace(".exe", ""));

                        {
                            collection.TryAdd(fullName, new SearchViewItem
                            {
                                Keys = keys, IsVisible = true, ItemDisplayName = localName,
                                OnlyKey = fullName, IsStared = star, Arguments = arg,
                                FileType = FileType.应用程序, Icon = null
                            });
                        }

                        log.Debug($"完成索引:{file}");
                    }
                    else
                    {
                        log.Debug($"不符合要求跳过索引:{file}");
                    }

                    break;
                }
                case ".url":
                {
                    var url = "";
                    var relFile = "";
                    var fileContent = File.ReadAllText(file); // read the file content
                    var pattern = @"URL=(.*)"; // the regex pattern to match the url
                    var match = Regex.Match(fileContent, pattern, RegexOptions.NonBacktracking); // match the pattern
                    if (match.Success) // if a match is found
                    {
                        url = match.Groups[1].Value.Replace("\r", ""); // get the url from the first group
                    }

                    var onlyKey = url;
                    if (collection.ContainsKey(onlyKey))
                    {
                        if (logging)
                        {
                            log.Debug($"重复索引:{file}");
                        }

                        return;
                    }

                    if (ConfigManger.Config.ignoreItems.Contains(onlyKey))
                    {
                        log.Debug($"忽略索引:{onlyKey}");
                        return;
                    }

                    var pattern2 = @"IconFile=(.*)"; // the regex pattern to match the url
                    var match2 = Regex.Match(fileContent, pattern2, RegexOptions.NonBacktracking); // match the pattern
                    if (match2.Success) // if a match is found
                    {
                        relFile = match2.Groups[1].Value.Replace("\r", ""); // get the url from the first group
                    }

                    if (string.IsNullOrWhiteSpace(relFile))
                    {
                        return;
                    }

                    var keys = new List<string>();

                    //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                    var localName = localizedName;
                    await NameSolver(keys, localName);
                    //nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
                    await NameSolver(keys, fileInfo.Name.Replace(".url", ""));

                    {
                        collection.TryAdd(onlyKey, new SearchViewItem
                        {
                            Keys = keys, IsVisible = true, ItemDisplayName = localName,
                            OnlyKey = onlyKey, IsStared = star,
                            IconPath = relFile,
                            FileType = FileType.URL, Icon = null
                        });
                    }

                    log.Debug($"完成索引:{file}");
                    break;
                }
                default:
                    if (File.Exists(file))
                    {
                        var keys = new List<string>();
                        await NameSolver(keys, localizedName);
                        collection.TryAdd(file, new SearchViewItem()
                        {
                            ItemDisplayName = localizedName,
                            FileType = FileType.文件,

                            OnlyKey = file,
                            Keys = keys,
                            IsStared = star,
                            IsVisible = true
                        });
                    }

                    break;
            }
        }
        else
        {
            if (Directory.Exists(file))
            {
                var keys = new List<string>();
                await NameSolver(keys, file.Split("\\").Last());

                collection.TryAdd(file, new SearchViewItem()
                {
                    ItemDisplayName = file.Split("\\").Last(),
                    FileType = FileType.文件夹,
                    IsStared = star,
                    OnlyKey = file,
                    Keys = keys,
                    Icon = null,
                    IsVisible = true
                });
            }
        }
    }


    //Console.WriteLine();
    private static void CreateCombinations(List<string> keys, int startIndex, string pair, string[] initialArray)
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
            AddUtil(keys, Pinyin.GetInitials(value).Replace(" ", "").ToLowerInvariant());
            AddUtil(keys, Pinyin.GetPinyin(value).Replace(" ", "").ToLowerInvariant());
            AddUtil(keys, MyRegex().Replace(lowerValue, ""));

            // 递归调用自身方法，生成更长的字符串组合
            CreateCombinations(keys, i + 1, value, initialArray);
        }
    }


    // 使用const或readonly修饰符来声明pattern字符串
    internal static async Task NameSolver(List<string> keys, string name)
    {
        var initials = name.Split(" ");
        CreateCombinations(keys, 0, "", initials);
        AddUtil(keys, Pinyin.GetInitials(name).Replace(" ", "").ToLowerInvariant());
        AddUtil(keys, Pinyin.GetPinyin(name).Replace(" ", "").ToLowerInvariant());
        AddUtil(keys,
            string.Concat(MyRegex().Replace(name, "").ToLowerInvariant(), name.Last().ToString().ToLowerInvariant()));
    }

    private static void AddUtil(List<string> keys, string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length <= 1)
        {
            return;
        }


        if (!keys.Contains(name))
        {
            keys.Add(name);
        }
    }

    [GeneratedRegex("[^A-Z]")]
    private static partial Regex MyRegex();
}