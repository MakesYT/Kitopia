using System;
using System.Windows;
using PluginCore.Attribute;

namespace PluginDemo;

public class Test1
{
    [PluginMethod("测试代码1")]
    public (string, bool) t(string id, string id2)
    {
        MessageBox.Show("5a");
        return new ValueTuple<string, bool>();
    }

    [PluginMethod("测试代码2")]
    public void t2(string id)
    {
        MessageBox.Show("5a");
    }

    [ConfigField("名称", "设置名称")] public string name = "1";
}