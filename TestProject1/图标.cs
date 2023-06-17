using System.Drawing;
using System.Text;
using System.Xml;
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
    public void lnkUrl()
    {
        string file = @"C:\Users\13540\Desktop\Control.url";
        var link = new LnkTools.ShellLink();
        ((LnkTools.IPersistFile)link).Load(file, 0);
        // TODO: if I can get hold of the hwnd call resolve first. This handles moved and renamed files.  
        // ((IShellLinkW)link).Resolve(hwnd, 0) 
        var sb = new StringBuilder();
        var data = new LnkTools.WIN32_FIND_DATAW();
        //((IShellLinkW)link).GetShowCmd
        ((LnkTools.IShellLinkW)link).GetPath(sb, sb.Capacity, out data, 0);
        ((LnkTools.IShellLinkW)link).GetIconLocation(sb, sb.Capacity, out var piIcon);
        Icon icon = Icon.ExtractAssociatedIcon(file + "," + piIcon);
        using (FileStream fs = new FileStream("1.png", FileMode.Create))
        {
            icon.Save(fs);
        }

        Console.WriteLine(sb);
        //Console.WriteLine(cmd);
    }

    [TestMethod]
    public void MscIcon()
    {
        IconTools iconTools = new IconTools();
        XmlDocument xd = new XmlDocument();
        xd.Load("C:\\Windows\\system32\\taskschd.msc"); //加载xml文档
        XmlNode rootNode = xd.SelectSingleNode("MMC_ConsoleFile"); //得到xml文档的根节点
        XmlNode BinaryStorage = rootNode.SelectSingleNode("VisualAttributes").SelectSingleNode("Icon");
        XmlNodeList lists = BinaryStorage.SelectNodes("Image");
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