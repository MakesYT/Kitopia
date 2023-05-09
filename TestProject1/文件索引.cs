using System.Diagnostics;
using Core.SDKs;

namespace TestProject1;

[TestClass]
public class 文件索引
{
    [TestMethod]
    public void TestMethod1()
    {
        var watch = Stopwatch.StartNew();


        var set = new HashSet<string>();
        AppSolver.nameSolver(set, "腾讯QQ");
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
        var watch = Stopwatch.StartNew();


        var set = new HashSet<string>();
        AppSolver.nameSolver(set, "Adobe After Effects 2023");
        watch.Stop();
        // 获取执行时间，单位为毫秒
        var elapsedMs = watch.ElapsedMilliseconds;
        // 打印或者记录执行时间
        Console.WriteLine("Time elapsed: {0} ms", elapsedMs);
        foreach (var item in set) Console.WriteLine(item);
    }

    [TestMethod]
    public void GetAllAppskeys()
    {
        var collection = new List<SearchViewItem>(); //存储本机所有软件
        var names = new List<string>(); //软件去重
        var watch = Stopwatch.StartNew();

        AppSolver.GetAllApps( collection,  names);
        watch.Stop();
        // 获取执行时间，单位为毫秒
        var elapsedMs = watch.ElapsedMilliseconds;
        // 打印或者记录执行时间
        Console.WriteLine("Time elapsed: {0} ms", elapsedMs);
        Console.WriteLine(collection.Count);
    }
}