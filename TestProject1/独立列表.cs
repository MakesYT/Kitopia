using System.ComponentModel;
using Core.SDKs;

namespace TestProject1;
[TestClass]
public class 独立列表
{
    [TestMethod]
    public void deepClone()
    {
        GetIconFromFile d = new GetIconFromFile();
        List<SearchViewItem> a=new List<SearchViewItem>()
        {
            new SearchViewItem()
            {
                fileType = FileType.图像,
                fileInfo = new FileInfo("C:\\Users\\13540\\Desktop\\新建文件夹\\88197916815538162.png"),
                fileName = "1",
                IsVisible = true,
                keys = new HashSet<string>()
                {
                    "2","21"
                }
                
                
            }
        };
        BindingList<SearchViewItem> b = new BindingList<SearchViewItem>();
        b.Add((SearchViewItem)a[0].Clone());
        b[0].IsVisible = false;
        b[0].icon = d.GetIcon("C:\\Windows\\Speech\\Common\\sapisvr.exe");
        Console.WriteLine(a[0]);
        Console.WriteLine(b[0]);

    }
}