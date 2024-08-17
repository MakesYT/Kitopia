using System.Drawing;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace PluginCore;

[ObservableObject]
public partial class PluginInfo
{
    public int Id { set; get; }

    public string AuthorName { set; get; }

    public int AuthorId { set; get; }

    public string Name { set; get; }
    public string NameSign { set; get; }
    public bool IsPublic { set; get; }

    public string Version { set; get; }
    public int VersionId { set; get; }

    public string Description { set; get; }
    
    public string Main { set; get; }
    public string FullPath { set; get; }
    public string Path { set; get; }
    [JsonIgnore]
    [property:JsonIgnore]
    [ObservableProperty] private Bitmap? _icon;
    
    [ObservableProperty] public bool isEnabled;
    [ObservableProperty] public bool unloadFailed;

    [ObservableProperty] private bool canUpdata;
    [ObservableProperty] private string canUpdateVersion;
    public int UpdateTargetVersion { set; get; }
    public string ToPlgString() => $"{Id}_{AuthorId}_{NameSign}";

    public override string ToString()
    {
        return ToPlgString();
    }
}