using BenchmarkDotNet.Attributes;
using Core.SDKs;
using Core.SDKs.Tools;

namespace TestProject1;

public class 文件索引性能
{
    List<SearchViewItem> collection = new List<SearchViewItem>(); //存储本机所有软件
    List<string> names = new List<string>(); //软件去重

    [Benchmark()]
    public void GetAllAppskeys()
    {
        AppTools.GetAllApps(collection, names);
    }
}