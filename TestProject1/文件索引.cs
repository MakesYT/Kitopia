using System.Diagnostics;
using Core.SDKs;
using Core.SDKs.Tools;

namespace TestProject1;

[TestClass]
public class 文件索引
{
    [TestMethod]
    public void TestMethod1()
    {
        var watch = Stopwatch.StartNew();


        var set = new HashSet<string>();
        AppTools.NameSolver(set, "腾讯QQ");
        watch.Stop();
        // 获取执行时间，单位为毫秒
        var elapsedMs = watch.ElapsedMilliseconds;
        // 打印或者记录执行时间
        Console.WriteLine("Time elapsed: {0} ms", elapsedMs);
        foreach (var item in set) Console.WriteLine(item);
    }

    [TestMethod]
    public void TestMethod2()
    {
        // 创建一个空的文件路径集合
        List<string> filePaths = new List<string>();

// 把桌面上的.lnk文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "*.lnk"));

// 把开始菜单中的.lnk和.appref-ms文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs",
            "*.lnk", SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles("C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs",
            "*.appref-ms", SearchOption.AllDirectories));

// 把自定义集合中的文件路径添加到集合中
        //filePaths.AddRange(ConfigManger.Config.customCollections);

// 把程序文件夹中的.lnk和.appref-ms文件路径添加到集合中
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.lnk", SearchOption.AllDirectories));
        filePaths.AddRange(Directory.EnumerateFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "*.appref-ms", SearchOption.AllDirectories));
    }

    [TestMethod]
    public void GetAllAppskeys()
    {
        var collection = new List<SearchViewItem>(); //存储本机所有软件
        var names = new List<string>(); //软件去重
        var watch = Stopwatch.StartNew();

        AppTools.GetAllApps(collection, names);
        watch.Stop();
        // 获取执行时间，单位为毫秒
        var elapsedMs = watch.ElapsedMilliseconds;
        // 打印或者记录执行时间
        Console.WriteLine("Time elapsed: {0} ms", elapsedMs);
        Console.WriteLine(collection.Count);
    }
}