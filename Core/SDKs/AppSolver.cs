using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.SDKs
{
    public class AppSolver
    {
        public static void GetAllApps(ref List<SearchViewItem> collection, ref List<string> names)
        {
            try
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
            catch (Exception ex)
            {

            }
        }
        private static void AppSolverA(ref List<SearchViewItem> collection,ref List<string> names,string file)
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
            
            //Console.WriteLine();
        }
        private static string pattern = @"([a-zA-Z0-9.]+)|([\u4e00-\u9fa5]+)"; //匹配英文数字或中文
        private static void nameSolver(List<string> keys, string name)
        {
            //匹配非中文或中文
            var matches = Regex.Matches(name, pattern);
            foreach (Match match in matches)
            {
                var a2 = match.Groups[1].Value;
                var a3 = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(a2))
                {
                    keys.Add(a2); //PowerPoint
                    keys.Add(a2.ToLowerInvariant()); //powerpoint
                    keys.Add(a2.ToUpperInvariant()); //POWERPOINT
                    var a1 = Regex.Replace(a2, "[^A-Z]", "");
                    keys.Add(a1); //PP
                    keys.Add($"{a1}{a2.Last()}"); //PPt
                    keys.Add($"{a1}{a2.Last().ToString().ToUpperInvariant()}"); //PPT
                    keys.Add($"{a1}{a2.Last().ToString().ToLowerInvariant()}"); //ppt
                }
                else if (!string.IsNullOrEmpty(a3))
                {
                    keys.Add(NPinyin.Pinyin.GetInitials(a3).ToLowerInvariant());
                    keys.Add(NPinyin.Pinyin.GetInitials(a3).ToUpperInvariant());
                    keys.Add(NPinyin.Pinyin.GetPinyin(a3).Replace(" ", "").ToUpperInvariant());
                    keys.Add(NPinyin.Pinyin.GetPinyin(a3).Replace(" ", "").ToLowerInvariant());
                }
            }
        }

    }
}
