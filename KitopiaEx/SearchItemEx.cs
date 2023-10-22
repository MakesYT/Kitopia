using PluginCore;
using PluginCore.Attribute;

namespace KitopiaEx;

public class SearchItemEx
{
    [PluginMethod("打开/运行本地项目", $"{nameof(item)}=本地项目",
        "return=返回参数")]
    public void OpenSearchViewItem(string item)
    {
        Kitopia.ISearchItemTool.OpenSearchItemByOnlyKey(item);
    }
}