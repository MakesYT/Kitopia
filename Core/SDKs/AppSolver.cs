using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Core.SDKs
{
    public partial class AppSolver
    {
        public static void GetAllApps(ref List<SearchViewItem> collection, ref List<string> names)
        {
            foreach (string file in Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.lnk"))
            {
                AppSolverA(ref collection, ref names, file);
            }
            foreach (string file in Directory.EnumerateFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*.lnk"))
            {
                AppSolverA(ref collection, ref names, file);
            }
            foreach (string path in Directory.EnumerateDirectories("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs", "*", System.IO.SearchOption.AllDirectories))
            {
                foreach (string file in Directory.EnumerateFiles(path, "*.lnk"))
                {
                    AppSolverA(ref collection, ref names, file);
                }
                foreach (string file in Directory.EnumerateFiles(path, "*.appref-ms"))
                {
                    AppSolverA(ref collection, ref names, file);
                }
            }
            foreach (string path in Directory.EnumerateDirectories(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "*", System.IO.SearchOption.AllDirectories))
            {
                foreach (string file in Directory.EnumerateFiles(path, "*.lnk"))
                {
                    AppSolverA(ref collection, ref names, file);
                }
                foreach (string file in Directory.EnumerateFiles(path, "*.appref-ms"))
                {
                    AppSolverA(ref collection, ref names, file);
                }
            }
        }
        private static void AppSolverA(ref List<SearchViewItem> collection, ref List<string> names, string file)
        {
            FileInfo fileInfo = new FileInfo(file);

            FileInfo refFileInfo = new FileInfo(LnkSolver.ResolveShortcut(file));
            if (refFileInfo.Exists)
                if (refFileInfo.Extension != ".url" && refFileInfo.Extension != ".txt" && refFileInfo.Extension != ".chm" && refFileInfo.Extension != ".pdf" && !fileInfo.Name.Contains("install") && !fileInfo.Name.Contains("安装") && !fileInfo.Name.Contains("卸载"))
                {

                    var keys = new HashSet<string>();

                    //collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = fileInfo.Name.Replace(".lnk", ""), fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });
                    var localName = LnkSolver.GetLocalizedName(file);
                    nameSolver(keys, localName);
                    //nameSolver(keys, fileInfo.Name.Replace(".lnk", ""));
                    nameSolver(keys, refFileInfo.Name.Replace(".exe", ""));
                    if (!names.Contains(localName))
                    {
                        names.Add(localName);
                        collection.Add(new SearchViewItem { keys = keys, IsVisible = true, fileInfo = refFileInfo, fileName = localName, fileType = FileType.App, icon = GetIconFromFile.GetIcon(refFileInfo.FullName) });

                    }

                }
        }
    


            //Console.WriteLine();
        private static void CreateCombinations(HashSet<string> keys,int startIndex, string pair, string[] initialArray)
        {
            for (int i = startIndex; i < initialArray.Length; i++)
            {
                var value = $"{pair}{initialArray[i]}";
                AddUtil(keys,value.ToLowerInvariant());
                AddUtil(keys,NPinyin.Pinyin.GetInitials(value).Replace(" ", "").ToLowerInvariant());
                AddUtil(keys,NPinyin.Pinyin.GetPinyin(value).Replace(" ", "").ToLowerInvariant());
                AddUtil(keys,GetRegex().Replace(value,  "").ToLowerInvariant());
                CreateCombinations(keys,i + 1, value, initialArray);
            }

            
        }
        [GeneratedRegex("[^A-Z]", RegexOptions.IgnoreCase)]
        private static partial Regex GetRegex();
        // 使用const或readonly修饰符来声明pattern字符串
        public static void nameSolver(HashSet<string> keys, string name)
        {
            var initials = name.Split(" ");
            CreateCombinations(keys, 0, "", initials);
            
            AddUtil(keys,NPinyin.Pinyin.GetInitials(name).Replace(" ", "").ToLowerInvariant());
            AddUtil(keys,NPinyin.Pinyin.GetPinyin(name).Replace(" ", "").ToLowerInvariant());
            AddUtil(keys,String.Concat(GetRegex().Replace(name,  "").ToLowerInvariant(), name.Last().ToString().ToLowerInvariant()));
        }

        private static void AddUtil(HashSet<string> keys, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (name.Length<=1)
            {
                return;
            }

            keys.Add(name);
        }



    }
}
