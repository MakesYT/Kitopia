#region

using System.Windows;
using System.Windows.Media;
using Core.SDKs.Services.Config;
using log4net;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

#endregion

namespace Kitopia.Assets;

public class ColorD : ResourceDictionary
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ColorD));

    public ColorD()
    {
        Instance = this;
        /*Add("SystemAccentColorSecondary", ColorConverter.ConvertFromString ("#EC407A"));*/
        Add("SystemAccentColorSecondary", ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color);

        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            ReloadColor();
            SystemThemeManager.UpdateSystemThemeCache();
            var currentTheme = ApplicationThemeManager.GetAppTheme();
            var b = ApplicationThemeManager.IsAppMatchesSystem();
            log.Debug("系统主题改变,当前主题为" + currentTheme + "当前系统主题为" + ApplicationThemeManager.GetSystemTheme());

            if (ConfigManger.Config.themeChoice == "跟随系统" && (int)ApplicationThemeManager.GetAppTheme() !=
                (int)ApplicationThemeManager.GetSystemTheme())
            {
                log.Debug("主题跟随系统,当前不符合切换主题");
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    ApplicationThemeManager.Apply(currentTheme == ApplicationTheme.Light
                        ? ApplicationTheme.Dark
                        : ApplicationTheme.Light);
                });
            }
        };
    }

    public static ColorD Instance
    {
        get;
        set;
    }

    public void ReloadColor()
    {
        Remove("SystemAccentColorSecondary");
        Add("SystemAccentColorSecondary", ((SolidColorBrush)SystemParameters.WindowGlassBrush).Color);
    }
}