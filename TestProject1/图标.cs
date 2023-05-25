using System.Drawing;
using System.Xml;
using Core.SDKs;
using Core.SDKs.Tools;

namespace TestProject1;
[TestClass]
public class 图标
{
    [TestMethod]
    public void FolderIcon()
    {
        IconTools iconTools = new IconTools();
        Icon d = iconTools.GetIcon("C:\\Windows\\system32\\WF.msc");
    }
    [TestMethod]
    public void MscIcon()
    {
        IconTools iconTools = new IconTools();
        XmlDocument xd = new XmlDocument();
        xd.Load("C:\\Windows\\system32\\taskschd.msc");//加载xml文档
        XmlNode rootNode = xd.SelectSingleNode("MMC_ConsoleFile");//得到xml文档的根节点
        XmlNode BinaryStorage=rootNode.SelectSingleNode("VisualAttributes").SelectSingleNode("Icon");
        XmlNodeList lists= BinaryStorage.SelectNodes("Image");
        foreach (XmlElement list in lists)
        {
            //Console.WriteLine(list.Name);
            if (list.GetAttribute("Name").Equals("Large48x"))
            {
                Console.WriteLine(list.GetAttribute("BinaryRefIndex"));
            }
        }

    }
}