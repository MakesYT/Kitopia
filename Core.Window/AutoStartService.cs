using System.IO;
using Core.SDKs.Services;
using log4net;
using Microsoft.Win32;
using PluginCore;

namespace Core.Window;

public class AutoStartService : IAutoStartService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(AutoStartService));

    public bool ChangeAutoStart(bool autoStart)
    {
        try
        {
            if (autoStart)
            {
                var strName = AppDomain.CurrentDomain.BaseDirectory + "KitopiaAvalonia.exe"; //获取要自动运行的应用程序名
                if (!File.Exists(strName)) //判断要自动运行的应用程序文件是否存在
                {
                    return false;
                }

                var registry =
                    Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                        true); //检索指定的子项
                if (registry == null) //若指定的子项不存在
                {
                    registry = Registry.CurrentUser.CreateSubKey(
                        "Software\\Microsoft\\Windows\\CurrentVersion\\Run"); //则创建指定的子项
                }
                else
                {
                    if (Equals(registry.GetValue("Kitopia"), $"\"{strName}\""))
                    {
                        log.Info("开机自启配置已存在");
                        return true;
                    }
                }

                log.Info("用户确认启用开机自启");
                try
                {
                    registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                    ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启",
                        "开机自启设置成功");
                }
                catch (Exception exception)
                {
                    log.Error("开机自启设置失败");
                    log.Error(exception.StackTrace);
                    ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启",
                        "开机自启设置失败");
                    return false;
                }
            }
            else
            {
                try
                {
                    var registry =
                        Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                            true); //检索指定的子项
                    registry?.DeleteValue("Kitopia");
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            log.Error("开机自启设置失败");
            log.Error(e.StackTrace);
            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启",
                "开机自启设置失败");
            return false;
        }

        return true;
    }
}