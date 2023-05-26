using Core.SDKs.Config;
using Core.SDKs.Services;
using Wpf.Ui.Appearance;

namespace Kitopia.Services;

public class ThemeChange:IThemeChange
{
    public void changeTo(string name)
    {

        switch (name)
        {
            case "theme_light":
                Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
                break;

            default:
                Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
                break;
        }
    }

    public void changeAnother()
    {
        var currentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();

        Wpf.Ui.Appearance.Theme.Apply(currentTheme == Wpf.Ui.Appearance.ThemeType.Light ? Wpf.Ui.Appearance.ThemeType.Dark : Wpf.Ui.Appearance.ThemeType.Light);
    }

    public void followSys(bool follow)
    {
        if (follow)
        {
            var currentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
            if (!Wpf.Ui.Appearance.Theme.IsAppMatchesSystem())
            {
                Wpf.Ui.Appearance.Theme.Apply(currentTheme == Wpf.Ui.Appearance.ThemeType.Light ? Wpf.Ui.Appearance.ThemeType.Dark : Wpf.Ui.Appearance.ThemeType.Light);
            }
        }
        
    }

    public bool isDark()
    {
        return Wpf.Ui.Appearance.Theme.GetAppTheme() == ThemeType.Dark;
    }
}