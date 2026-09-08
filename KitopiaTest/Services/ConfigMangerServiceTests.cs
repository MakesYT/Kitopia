using Kitopia.Desktop.Features.Services.Config;
using Kitopia.Desktop.Features.Services.Interfaces;
using PluginCore.Config;

namespace KitopiaTest.Services;

[TestClass]
[DoNotParallelize]
public sealed class ConfigMangerServiceTests
{
    [TestMethod]
    public void ConfigManger_ExposesManagerStateThroughConfigService()
    {
        var originalConfigs = ConfigManger.Configs;

        try
        {
            var config = new KitopiaConfig { Name = "KitopiaConfig" };
            ConfigManger.Configs = new Dictionary<string, ConfigBase>
            {
                ["KitopiaConfig"] = config
            };

            IConfigService service = new ConfigManger();

            Assert.AreEqual(ConfigManger.Version, service.Version);
            Assert.AreEqual(ConfigManger.ApiUrl, service.ApiUrl);
            Assert.AreSame(ConfigManger.Configs, service.Configs);
            Assert.AreSame(ConfigManger.Config, service.Config);
            Assert.AreSame(ConfigManger.DefaultOptions, service.DefaultOptions);
            Assert.AreEqual(50, config.indexingMaximumCpuUsagePercent);
        }
        finally
        {
            ConfigManger.Configs = originalConfigs;
        }
    }

    [TestMethod]
    public void ConfigManger_WhenUseLocalhostDebugEnabled_ReturnsLocalhostUrls()
    {
#if DEBUG
        var originalConfigs = ConfigManger.Configs;
        try
        {
            var config = new KitopiaConfig { Name = "KitopiaConfig", useLocalhostDebug = true };
            ConfigManger.Configs = new Dictionary<string, ConfigBase>
            {
                ["KitopiaConfig"] = config
            };

            Assert.AreEqual("https://localhost:5111", ConfigManger.ApiUrl);
            Assert.AreEqual("http://localhost:3000", ConfigManger.WebUrl);

            config.useLocalhostDebug = false;
            Assert.AreEqual("https://api.kitopia.top:5111", ConfigManger.ApiUrl);
            Assert.AreEqual("https://kitopia.top", ConfigManger.WebUrl);
        }
        finally
        {
            ConfigManger.Configs = originalConfigs;
        }
#endif
    }
}
