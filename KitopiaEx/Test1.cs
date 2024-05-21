using System.Threading;
using PluginCore.Attribute;

namespace KitopiaEx;

public class Test1
{
    [PluginMethod("Test", $"{nameof(item)}=本地项目",
        "return=返回参数")]
    public void OpenSearchViewItem(NodeInputType1 item, CancellationToken cancellationToken)
    {
    }
}