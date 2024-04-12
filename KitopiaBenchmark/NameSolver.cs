using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;

using NPinyin;

namespace KitopiaBenchmark;

[RPlotExporter]
public partial class NameSolver
{
    [GeneratedRegex("[^A-Z]")]
    private static partial Regex MyRegex();

    [GeneratedRegex("[\u4e00-\u9fa5]")]
    private static partial Regex ChineseRegex();

    [Benchmark]
    public void Test1()
    {
        var name = "Windows Copilot Frame MSIX Pack";
        var keys = new System.Collections.Generic.List<string>();
        var initials = name.Split(" ",StringSplitOptions.RemoveEmptyEntries);
        CreateCombinations(keys, 0, new StringBuilder(), initials);
        
    }

    public static void CreateCombinations(List<string> keys, int startIndex, StringBuilder pair, string[] initialArray)
    {
        
        // 遍历初始数组中的元素
        pair.Append(initialArray[startIndex].ToLowerInvariant());
        var name = pair.ToString();
        AddUtil(keys, name);
        AddUtil(keys, Pinyin.GetInitials(name).Replace(" ", "").ToLowerInvariant());
        AddUtil(keys, Pinyin.GetPinyin(name).Replace(" ", "").ToLowerInvariant());
        AddUtil(keys, MyRegex().Replace(name, ""));
        if (startIndex > initialArray.Length-2)
        {
            return;
        }
        CreateCombinations(keys, startIndex + 1, pair, initialArray);
    }

    [Benchmark]
    public void Test2()
    {
        var name = "Windows Copilot Frame MSIX Pack";
        var keys = new System.Collections.Generic.List<string>();
        var initials = name.Split(" ",StringSplitOptions.RemoveEmptyEntries);
        CreateCombinations2(keys, 0, new StringBuilder(), initials);
        
    }

    public static void CreateCombinations2(List<string> keys, int startIndex, StringBuilder pair, string[] initialArray)
    {
        
        // 遍历初始数组中的元素
        var lowerInvariant = initialArray[startIndex].ToLowerInvariant();
       
        var name = pair.ToString();
        AddUtil(keys, $"{name}{lowerInvariant}");
        if (ChineseRegex().IsMatch(lowerInvariant))
        {
            AddUtil(keys, $"{name}{Pinyin.GetInitials(lowerInvariant).Replace(" ", "").ToLowerInvariant()}");
            AddUtil(keys, $"{name}{Pinyin.GetPinyin(lowerInvariant).Replace(" ", "").ToLowerInvariant()}");
        }
        
        AddUtil(keys, $"{name}{MyRegex().Replace(lowerInvariant, "")}");
        pair.Append(lowerInvariant);
        if (startIndex > initialArray.Length-2)
        {
            return;
        }
        CreateCombinations2(keys, startIndex + 1, pair, initialArray);
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
}