using System.Windows;
using PluginCore.Attribute;

namespace PluginDemo;

public class Test1
{
    [PluginMethod]
    public void t()
    {
        MessageBox.Show("a");
    }

    [ConfigField] public string name = "1";
}