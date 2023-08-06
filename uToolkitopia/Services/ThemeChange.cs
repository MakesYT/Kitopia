using Core.SDKs.Services;
using log4net;
using Wpf.Ui.Appearance;

namespace Kitopia.Services;

public class ThemeChange : IThemeChange
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ThemeChange));

    public void changeTo(string name)
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(changeTo) + "被调用");


        switch (name)
        {
            case "theme_light":
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light);
                break;

            default:
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
                break;
        }
    }

    public void changeAnother()
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(changeAnother) + "被调用");


        var currentTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();

        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(currentTheme == Wpf.Ui.Appearance.ApplicationTheme.Light
            ? Wpf.Ui.Appearance.ApplicationTheme.Dark
            : Wpf.Ui.Appearance.ApplicationTheme.Light);
    }

    public void followSys(bool follow)
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(follow) + "被调用");


        if (follow)
        {
            var currentTheme = Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme();
            if (!Wpf.Ui.Appearance.ApplicationThemeManager.IsAppMatchesSystem())
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(currentTheme == Wpf.Ui.Appearance.ApplicationTheme.Light
                    ? Wpf.Ui.Appearance.ApplicationTheme.Dark
                    : Wpf.Ui.Appearance.ApplicationTheme.Light);
            }
        }
    }

    public bool isDark()
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(isDark) + "被调用");


        return Wpf.Ui.Appearance.ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;
    }
}