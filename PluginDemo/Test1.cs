using System.Windows;
using PluginCore;

namespace PluginDemo;

public class Test1
{
    [PluginMethod]
    public void t()
    {
        MessageBox.Show("a");
    }
}