#region

using System.Windows;
using Avalonia.Media;
using log4net;
using Microsoft.Win32;
using ResourceDictionary = Avalonia.Controls.ResourceDictionary;

#endregion

namespace Kitopia.Assets;

public class ColorD : ResourceDictionary
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ColorD));

    public ColorD()
    {
        Instance = this;
        /*Add("SystemAccentColorSecondary", ColorConverter.ConvertFromString ("#EC407A"));*/

        Add("SystemAccentColorPrimary", SolidColorBrush.Parse((SystemParameters.WindowGlassBrush).ToString()));
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            ReloadColor();

            /*var currentTheme = ApplicationThemeManager.GetAppTheme();
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
            }*/
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
        Remove("SystemAccentColorPrimary");
        Add("SystemAccentColorPrimary", SolidColorBrush.Parse((SystemParameters.WindowGlassBrush).ToString()));
    }
}