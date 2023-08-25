using System.Drawing;
using System.IO;
using System.Xml;
using log4net;
using Vanara.Extensions;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

namespace Core.SDKs.Tools;

public class UWPAPPsTools
{
    private static readonly ILog log = LogManager.GetLogger(nameof(UWPAPPsTools));

    public static XmlNode? GetApplicationNode(XmlNode node)
    {
        foreach (XmlNode? o in node.ChildNodes)
        {
            if (o.Name == "Application")
            {
                return o;
            }
            else
            {
                var nodes = GetApplicationNode(o);
                if (nodes is not null)
                {
                    return nodes;
                }
            }
        }

        return null;
    }

    public static async void GetAll(Dictionary<string, SearchViewItem> items)
    {
        FirewallApi.NetworkIsolationEnumAppContainers(FirewallApi.NETISO_FLAG.NETISO_FLAG_FORCE_COMPUTE_BINARIES,
            out var pdwNumPublicAppCs, out var ppPublicAppCs);
        foreach (var appContainer in ppPublicAppCs.ToIEnum<FirewallApi.INET_FIREWALL_APP_CONTAINER>(
                     (int)pdwNumPublicAppCs))
        {
            if (string.IsNullOrWhiteSpace(appContainer.appContainerName) ||
                string.IsNullOrWhiteSpace(appContainer.displayName) ||
                string.IsNullOrWhiteSpace(appContainer.workingDirectory))
            {
                continue;
            }

            string? fileName = appContainer.displayName;
            try
            {
                fileName = new IndirectString(appContainer.displayName).Value;
            }
            catch (Exception e)
            {
                log.Error($"{appContainer.displayName},{e.Message},{e.StackTrace}");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                continue;
            }

            HashSet<string> keys = new HashSet<string>();
            AppTools.NameSolver(keys, fileName).Wait();
            XmlDocument xmlDocument = new XmlDocument();
            if (File.Exists($"{appContainer.workingDirectory}\\AppxManifest.xml"))
            {
                xmlDocument.Load($"{appContainer.workingDirectory}\\AppxManifest.xml");
            }
            else if (File.Exists($"{appContainer.workingDirectory}\\appxmanifest.xml"))
            {
                xmlDocument.Load($"{appContainer.workingDirectory}\\appxmanifest.xml");
            }
            else
            {
                continue;
            }

            XmlNode? Application = GetApplicationNode(xmlDocument);
            if (Application is null)
            {
                continue;
            }

            XmlNode? VisualElements = null;
            foreach (XmlNode applicationChildNode in Application.ChildNodes)
            {
                Console.WriteLine(applicationChildNode.Name);
                if (applicationChildNode.Name.Contains("VisualElements"))
                {
                    VisualElements = applicationChildNode;
                }
            }

            var SquareLogo = VisualElements.Attributes["Square44x44Logo"].Value;
            var logoName = SquareLogo.Split("\\").Last().Split(".").First();
            var path = $"{appContainer.workingDirectory}{SquareLogo.Split("\\").First()}";
            if (!Directory.Exists(path))
            {
                continue;
            }

            DirectoryInfo logos =
                new DirectoryInfo(path);
            var pa = $"{path}\\{logoName}.scale-200.png";
            if (File.Exists(pa))
            {
                using (Bitmap bm = new Bitmap(pa))
                {
                    using (Bitmap iconBm = new Bitmap(bm, new Size(64, 64)))
                    {
                        //如果是windows调用，直接下面一行代码就可以了
                        //此代码不能在web程序中调用，会有安全异常抛出
                        using (Icon icon = Icon.FromHandle(iconBm.GetHicon()))
                        {
                            var searchViewItem = new SearchViewItem()
                            {
                                FileName = fileName,
                                OnlyKey = appContainer.appContainerName,
                                FileType = FileType.UWP应用,
                                Keys = keys,
                                Icon = (Icon?)icon.Clone(),
                                IsVisible = true
                            };

                            log.Debug(
                                $"appContainerName:{appContainer.appContainerName},\n displayName:{searchViewItem.FileName},\nworkingDirectory:{appContainer.workingDirectory}\n");
                            //Console.WriteLine(searchViewItem);
                            items.TryAdd(appContainer.appContainerName, searchViewItem);
                            continue;
                        }
                    }
                }
            }

            {
                foreach (var enumerateFile in logos.EnumerateFiles())
                {
                    if (enumerateFile.Name.StartsWith(logoName))
                    {
                        using (Bitmap bm = new Bitmap(enumerateFile.FullName))
                        {
                            using (Bitmap iconBm = new Bitmap(bm, new Size(64, 64)))
                            {
                                //如果是windows调用，直接下面一行代码就可以了
                                //此代码不能在web程序中调用，会有安全异常抛出
                                using (Icon icon = Icon.FromHandle(iconBm.GetHicon()))
                                {
                                    var searchViewItem = new SearchViewItem()
                                    {
                                        FileName = fileName,
                                        OnlyKey = appContainer.appContainerName,
                                        FileType = FileType.UWP应用,
                                        Keys = keys,
                                        Icon = (Icon?)icon.Clone(),
                                        IsVisible = true
                                    };
                                    Console.WriteLine(
                                        $"appContainerName:{appContainer.appContainerName},\n displayName:{searchViewItem.FileName},\nworkingDirectory:{appContainer.workingDirectory}\n");
                                    //Console.WriteLine(searchViewItem);
                                    items.TryAdd(appContainer.appContainerName, searchViewItem);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}