#region

using System.Windows;
using System.Windows.Media;
using Core.SDKs.Services.Config;

#endregion

namespace Kitopia.Assets;

public class ColorD : ResourceDictionary
{
    public static ColorD Instance
    {
        get;
        set;
    }

    public ColorD()
    {
        Instance = this;
        /*Add("SystemAccentColorSecondary", ColorConverter.ConvertFromString ("#EC407A"));*/
        Add("SystemAccentColorSecondary", ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color);
    }

    public void ReloadColor()
    {
        Remove("SystemAccentColorSecondary");
        Add("SystemAccentColorSecondary", ColorConverter.ConvertFromString(ConfigManger.Config.themeColor));
    }
}