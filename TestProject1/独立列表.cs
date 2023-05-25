using System.ComponentModel;
using Core.SDKs;
using Core.SDKs.Tools;

namespace TestProject1;
[TestClass]
public class 独立列表
{
    [TestMethod]
    public void deepClone()
    {
        IconTools d = new IconTools();
        List<SearchViewItem> a=new List<SearchViewItem>()
        {
            new SearchViewItem()
            {
                FileType = FileType.图像,
                FileInfo = new FileInfo("C:\\Users\\13540\\Desktop\\新建文件夹\\88197916815538162.png"),
                FileName = "1",
                IsVisible = true,
                Keys = new HashSet<string>()
                {
                    "2","21"
                }
                
                
            }
        };
        BindingList<SearchViewItem> b = new BindingList<SearchViewItem>();
        b.Add((SearchViewItem)a[0].Clone());
        b[0].IsVisible = false;
        b[0].Icon = d.GetIcon("C:\\Windows\\Speech\\Common\\sapisvr.exe");
        Console.WriteLine(a[0]);
        Console.WriteLine(b[0]);

    }
}