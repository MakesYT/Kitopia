﻿#region

using System.IO;
using System.Xml;
using Core.SDKs.Services.Config;
using log4net;
using PluginCore;
using Vanara.Extensions;
using Vanara.PInvoke;
using Vanara.Windows.Shell;

#endregion

namespace Core.Window;

internal static class UwpTools
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(UwpTools));

    private static XmlNode? GetApplicationNode(XmlNode node)
    {
        foreach (XmlNode o in node.ChildNodes)
        {
            if (o.Name == "Application")
            {
                return o;
            }

            var nodes = GetApplicationNode(o);
            if (nodes is not null)
            {
                return nodes;
            }
        }

        return null;
    }

    internal static async Task GetAll(Dictionary<string, SearchViewItem> items)
    {
        List<Task> list = new();
        FirewallApi.NetworkIsolationEnumAppContainers(FirewallApi.NETISO_FLAG.NETISO_FLAG_FORCE_COMPUTE_BINARIES,
            out var pdwNuminternalAppCs, out var ppinternalAppCs);
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 256
        };
        Parallel.ForEach(ppinternalAppCs.ToIEnum<FirewallApi.INET_FIREWALL_APP_CONTAINER>(
            (int)pdwNuminternalAppCs), options, file =>
        {
            list.Add(AppContainerAnalyse(file, items));
        });

        await Task.WhenAll(list.ToArray());
    }

    private static async Task AppContainerAnalyse(FirewallApi.INET_FIREWALL_APP_CONTAINER appContainer,
        Dictionary<string, SearchViewItem> items)
    {
        if (ConfigManger.Config.ignoreItems.Contains(appContainer.appContainerName))
        {
            Log.Debug("忽略索引:" + appContainer.appContainerName);
            return;
        }

        //log.Debug(Thread.CurrentThread.ManagedThreadId);
        if (string.IsNullOrWhiteSpace(appContainer.appContainerName) ||
            string.IsNullOrWhiteSpace(appContainer.displayName) ||
            string.IsNullOrWhiteSpace(appContainer.workingDirectory))
        {
            return;
        }

        var fileName = appContainer.displayName;
        try
        {
            fileName = new IndirectString(appContainer.displayName).Value;
        }
        catch (Exception e)
        {
            Log.Error($"{appContainer.displayName},{e.Message},{e.StackTrace}");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        List<string> keys = new();
        await Window.AppTools.NameSolver(keys, fileName);
        var xmlDocument = new XmlDocument();
        if (File.Exists($"{appContainer.workingDirectory}{Path.DirectorySeparatorChar}AppxManifest.xml"))
        {
            xmlDocument.Load($"{appContainer.workingDirectory}{Path.DirectorySeparatorChar}AppxManifest.xml");
        }
        else if (File.Exists($"{appContainer.workingDirectory}{Path.DirectorySeparatorChar}appxmanifest.xml"))
        {
            xmlDocument.Load($"{appContainer.workingDirectory}{Path.DirectorySeparatorChar}appxmanifest.xml");
        }
        else
        {
            return;
        }

        var application = GetApplicationNode(xmlDocument);

        if (application?.Attributes == null)
        {
            return;
        }

        var applicationAttribute = application.Attributes["Id"];
        if (applicationAttribute is null)
        {
            return;
        }

        var id = applicationAttribute.Value;
        XmlNode? visualElements = null;
        foreach (XmlNode applicationChildNode in application.ChildNodes)
        {
            //Console.WriteLine(applicationChildNode.Name);
            if (applicationChildNode.Name.Contains("VisualElements"))
            {
                visualElements = applicationChildNode;
            }
        }

        if (visualElements == null)
        {
            return;
        }

        if (visualElements.Attributes == null)
        {
            return;
        }

        var visualElementsAttribute = visualElements.Attributes["Square44x44Logo"];


        if (visualElementsAttribute == null)
        {
            return;
        }

        var squareLogo = visualElementsAttribute.Value;
        var logoName = squareLogo.Split(Path.DirectorySeparatorChar).Last().Split(".").First();
        var path = $"{appContainer.workingDirectory}{squareLogo.Split(Path.DirectorySeparatorChar).First()}";
        if (!Directory.Exists(path))
        {
            return;
        }

        var logos =
            new DirectoryInfo(path);
        var pa = $"{path}{Path.DirectorySeparatorChar}{logoName}.scale-200.png";
        if (File.Exists(pa))
        {
            var searchViewItem = new SearchViewItem()
            {
                ItemDisplayName = fileName,
                OnlyKey = $"{appContainer.appContainerName}!{id}",
                FileType = FileType.UWP应用,
                Keys = keys,
                IconPath = pa,
                IsVisible = true
            };

            Log.Debug(
                $"完成索引UWP应用{fileName},ID:{searchViewItem.OnlyKey}");
            //Console.WriteLine(searchViewItem);
            items.TryAdd(searchViewItem.OnlyKey, searchViewItem);
            return;
        }

        {
            foreach (var enumerateFile in logos.EnumerateFiles())
            {
                if (enumerateFile.Name.StartsWith(logoName))
                {
                    var searchViewItem = new SearchViewItem()
                    {
                        ItemDisplayName = fileName,
                        OnlyKey = $"{appContainer.appContainerName}!{id}",
                        FileType = FileType.UWP应用,
                        Keys = keys,
                        IconPath = enumerateFile.FullName,
                        IsVisible = true
                    };
                    Log.Debug(
                        $"完成索引UWP应用{fileName},ID:{searchViewItem.OnlyKey}");
                    //Console.WriteLine(searchViewItem);
                    items.TryAdd(appContainer.appContainerName, searchViewItem);
                    break;
                }
            }
        }
    }
}