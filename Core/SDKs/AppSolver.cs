using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Core.SDKs
{
    public class AppSolver
    {
        // 定义一个方法，用于遍历指定目录下的所有符合条件的文件，并调用AppSolverA方法
        private static void EnumerateFiles(string directory, ref List<SearchViewItem> collection, ref List<string> names)
        {
            // 使用DirectoryInfo类来提高遍历效率[^1^][1]
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            // 使用SearchOption.TopDirectoryOnly来避免递归遍历所有子目录[^2^][2]
            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*.lnk", SearchOption.TopDirectoryOnly))
            {
                AppSolverA(ref collection, ref names, fileInfo.FullName);
            }
            foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*.appref-ms", SearchOption.TopDirectoryOnly))
            {
                AppSolverA(ref collection, ref names, fileInfo.FullName);
            }
        }
        public static void GetAllApps(ref List<SearchViewItem> collection, ref List<string> names)
        {

            // 调用EnumerateFiles方法来遍历不同的目录
            EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ref collection, ref names);
            EnumerateFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", ref collection, ref names);
            // 使用DirectoryInfo类来获取子目录[^1^][1]
            DirectoryInfo programDir = new DirectoryInfo("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs");
            foreach (DirectoryInfo subDir in programDir.EnumerateDirectories())
            {
                EnumerateFiles(subDir.FullName, ref collection, ref names);
            }
            DirectoryInfo userDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
            foreach (DirectoryInfo subDir in userDir.EnumerateDirectories())
            {
                EnumerateFiles(subDir.FullName, ref collection, ref names);
            }
        }
        private static void AppSolverA(ref List<SearchViewItem> collection, ref List<string> names, string file)
        {
            FileInfo fileInfo = new FileInfo(file);

            FileInfo refFileInfo = new FileInfo(LnkSolver.ResolveShortcut(file));
            if (refFileInfo.Exists)
                if (refFileInfo.Extension != ".url" && refFileInfo.Extension != ".txt" && refFileInfo.Extension != ".chm" && refFileInfo.Extension != ".pdf" && !fileInfo.Name.Contains("install") && !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
                {

                    List<string> keys = new List<string>();

                    //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                    string localName = LnkSolver.GetLocalizedName(file);
                    nameSolver(keys, localName);
                    nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
                    nameSolver(keys, refFileInfo.Name.Replace(".exe", ""));
                    if (!names.Contains(localName))
                    {
                        names.Add(localName);
                        collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = localName, fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });

                    }

                }
        }
    


            //Console.WriteLine();
        
        // 使用const或readonly修饰符来声明pattern字符串
        private readonly static string pattern = @"([a-zA-Z0-9.]+)|([\u4e00-\u9fa5]+)"; //匹配英文数字或中文
        private static void nameSolver(List<string> keys, string name)
        {
            // 使用集合初始化器或LINQ表达式来简化向字符串列表中添加元素的操作
            keys.AddRange(new[]
            {
                // 使用StringComparison.OrdinalIgnoreCase来转换字符串为小写
               
                NPinyin.Pinyin.GetInitials(name).Replace(" ", "").ToLowerInvariant(),
                NPinyin.Pinyin.GetPinyin(name).Replace(" ", "").ToLowerInvariant()
            }) ;
            //匹配非中文或中文
            var matches = Regex.Matches(name, pattern);
            foreach (Match match in matches)
            {
                var a2 = match.Groups[1].Value;
                var a3 = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(a2))
                {
                    //PowerPoint
                    // 使用StringComparison.OrdinalIgnoreCase来转换字符串为小写
                    keys.AddRange(new[]
                    {
                        a2.ToLowerInvariant(),
                        String.Concat(Regex.Replace(a2, "[^A-Z]", ""), a2.Last().ToString().ToLowerInvariant())
                    });

                    //powerpoint


                    // 使用String.Concat方法来拼接字符串

                }
                else if (!string.IsNullOrEmpty(a3))
                {
                    // 使用集合初始化器或LINQ表达式来简化向字符串列表中添加元素的操作
                    keys.AddRange(new[]
                    {
                        NPinyin.Pinyin.GetInitials(a3).ToLowerInvariant(),
                        NPinyin.Pinyin.GetPinyin(a3).Replace(" ", "").ToLowerInvariant()
                    });
                }
            }
        }



    }
}
