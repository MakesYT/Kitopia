using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PluginCore;

[ObservableObject]
public partial class PluginInfo
{
    public int Id { set; get; }

    public string AuthorName { set; get; }

    public int AuthorId { set; get; }

    public string DisplayName { set; get; }
    public string NameSign { set; get; }
    public bool IsPublic { set; get; }

    public string Version { set; get; }
    public int VersionId { set; get; }

    public string Description { set; get; }
    
    public string Main { set; get; }
    public string FullPath { set; get; }
    
    [ObservableProperty] public bool isEnabled;
    [ObservableProperty] public bool unloadFailed;
    
    public string ToPlgString() => $"{Id}_{AuthorId}_{NameSign}";

    public override string ToString()
    {
        return ToPlgString();
    }
}