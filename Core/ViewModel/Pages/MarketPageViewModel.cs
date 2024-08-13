using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Core.ViewModel.Pages;
public class ApiResponse
{
    public bool flag { get; set; }
    public List<OnlinePluginInfo> data { get; set; }
}
public partial class OnlinePluginInfo
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
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = delegate { return true; };
        _httpClient = new HttpClient(handler);
        LoadPlugins();
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
    private void OpenPlugin(PluginInfo plugin)
    {
       
    }
    
}