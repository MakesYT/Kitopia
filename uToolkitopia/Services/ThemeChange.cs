#region

using Core.SDKs.Services;
using log4net;
using Wpf.Ui.Appearance;

#endregion

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
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
                break;

            default:
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                break;
        }
    }

    public void changeAnother()
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(changeAnother) + "被调用");


        var currentTheme = ApplicationThemeManager.GetAppTheme();

        ApplicationThemeManager.Apply(currentTheme == ApplicationTheme.Light
            ? ApplicationTheme.Dark
            : ApplicationTheme.Light);
    }

    public void followSys(bool follow)
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(follow) + "被调用");


        if (follow)
        {
            var currentTheme = ApplicationThemeManager.GetAppTheme();
            if (!ApplicationThemeManager.IsAppMatchesSystem())
            {
                ApplicationThemeManager.Apply(currentTheme == ApplicationTheme.Light
                    ? ApplicationTheme.Dark
                    : ApplicationTheme.Light);
            }
        }
    }

    public bool isDark()
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(isDark) + "被调用");


        return ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;
    }
}