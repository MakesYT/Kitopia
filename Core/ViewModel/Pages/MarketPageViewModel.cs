using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Core.ViewModel.Pages;
public class ApiResponse
{
    public bool flag { get; set; }
    public List<OnlinePluginInfo> data { get; set; }
}
public partial class OnlinePluginInfo : ObservableObject
{
    public int Id { set; get; }

    public string AuthorName
    {
        set => throw new NotImplementedException();
        get
        {
           
            var request = new HttpRequestMessage() {
                RequestUri = new Uri("https://www.ncserver.top:5111/api/user/baseInfo"),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("id",AuthorId.ToString());
            var async = MarketPageViewModel._httpClient.SendAsync(request).GetAwaiter().GetResult();
            var stringAsync =  async.Content.ReadAsStringAsync().Result;
            var deserializeObject = (JObject)JsonConvert.DeserializeObject(stringAsync);
            
            return deserializeObject["data"]["userName"].ToString();
        }
    }

    public int AuthorId { set; get; }
    [ObservableProperty] public Bitmap? _icon;

    public string Name { set; get; }
    public string NameSign { set; get; }
    public bool IsPublic { set; get; }

    public string LastVersion { set; get; }
    public int LastVersionId { set; get; }

    public string Description { set; get; }
    
    public bool InLocal {
        get{
            return PluginManager.AllPluginInfos.Any(x=>x.NameSign==NameSign);
        }}

    public void Upadate()
    {
        OnPropertyChanged(nameof(InLocal));
    }
    public string ToPlgString() => $"{Id}_{AuthorId}_{NameSign}";

    public override string ToString()
    {
        return ToPlgString();
    }
}
public partial class MarketPageViewModel : ObservableObject
{
    public static HttpClient _httpClient;
    [ObservableProperty] private ObservableCollection<OnlinePluginInfo> _plugins = new();

    public MarketPageViewModel()
    {
        _httpClient = new HttpClient();
        LoadPlugins();
    }

     ~MarketPageViewModel()
     {
         for (var i = 0; i < _plugins.Count; i++)
         {
             _plugins[i].Icon?.Dispose();
         }
     }
    private async Task LoadPlugins()
    {
        var async =await  MarketPageViewModel._httpClient.GetAsync("https://www.ncserver.top:5111/api/plugin/all");
        var stringAsync = await async.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(stringAsync,options);
        if (apiResponse != null && apiResponse.data != null)
        {
            for (var i = 0; i < apiResponse.data.Count; i++)
            {
                Plugins.Add(apiResponse.data[i]);
            }
        }
    }

    [RelayCommand]
    private async Task DownloadPlugin(OnlinePluginInfo plugin)
    {
        var streamAsync =await _httpClient.GetStreamAsync($"https://www.ncserver.top:5111/api/plugin/download/1/{plugin.Id}/{plugin.LastVersionId}");
        Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"));
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp",$"{plugin}.zip");
        using (var fs = new FileStream(path, FileMode.Create))
        {
            await streamAsync.CopyToAsync(fs);
        }

        var zipArchive = ZipFile.Open(path,ZipArchiveMode.Read);
        zipArchive.ExtractToDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins",plugin.ToPlgString()));
        zipArchive.Dispose();
        File.Delete(path);
        PluginManager.Reload();
        var pluginInfoEx = PluginManager.AllPluginInfos.FirstOrDefault(e=>e.ToPlgString()==plugin.ToPlgString());
        if (pluginInfoEx is null)
        {
            return;
        }
        PluginManager.EnablePluginByInfo(pluginInfoEx);
        plugin.Upadate();
    }
    
}