using System.Windows;
using PluginCore.Attribute;

namespace PluginDemo;

public class Test1
{
    [PluginMethod]
    public void t(string id)
    {
        MessageBox.Show("5a");
    }

    [PluginMethod]
    public void t2()
    {
        MessageBox.Show("5a");
    }

    [ConfigField("名称", "设置名称")] public string name = "1";
}