using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media.Imaging;
using Core.ViewModel.Pages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;
using SixLabors.ImageSharp;

namespace KitopiaAvalonia.Converter.MarketPage;

public class IconCtr : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not null)
        {
            return value;
        }
        var onlinePluginInfo =
            ((Control)((CompiledBindingExtension)parameter).DefaultAnchor.Target).DataContext as OnlinePluginInfo;
        {
            if (onlinePluginInfo.Icon is not null)
            {
                return onlinePluginInfo.Icon;
            }

            Task.Run(async () =>
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri("https://www.ncserver.top:5111/api/plugin/avatar"),
                    Method = HttpMethod.Get,
                };
                request.Headers.Add("id", onlinePluginInfo.Id.ToString());
                var sendAsync =await MarketPageViewModel._httpClient.SendAsync(request);
                var stringAsync =await  sendAsync.Content.ReadAsStringAsync();
                var deserializeObject = (JObject)JsonConvert.DeserializeObject(stringAsync);
                onlinePluginInfo.Icon = new Bitmap(new MemoryStream(deserializeObject["data"].ToObject<byte[]>()));
            });


        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}