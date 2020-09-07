using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.InternalServices
{
    public class WeatherConfig
    {
        public string WeatherUndergroundUrl { get; set; }
    }

    public class Weather : Tile
    {
        //todo implement tile for changes?
        public override IDataTemplate DataTemplate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private const string defaultMessage = "No Weather Data";
        private WeatherConfig weatherConfig = new WeatherConfig();
        private Subject<string> infoLine { get; } = new Subject<string>();
        private Subject<string> weatherUrl { get; } = new Subject<string>();

        public override MenuItem MiniTile => new MenuItem
        {
            [!MenuItem.HeaderProperty] = infoLine.ToBinding(),
            [!MenuItem.CommandParameterProperty] = weatherUrl.ToBinding(),
            CommandParameter = weatherConfig.WeatherUndergroundUrl,
            Command = ReactiveCommand.Create<string>(url =>
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            })
        };

        private async Task<(int, string)> GetWeatherWunderground()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, weatherConfig.WeatherUndergroundUrl);
            req.Headers.Add("User-Agent", "curl");
            var response = await Helpers.HttpClient.SendAsync(req);
            var data = await response.Content.ReadAsStringAsync();
            var page = new HtmlAgilityPack.HtmlDocument();
            page.LoadHtml(data);
            var temp = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[2]/div/div/div[2]/lib-display-unit/span/span[1]").InnerHtml;
            var conditions = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[3]/div/div[1]/p").InnerHtml;
            return (Convert.ToInt32(temp), conditions);
        }

        public async Task ReloadConfig()
        {
            try
            {
                var currentUrl = weatherConfig.WeatherUndergroundUrl;
                weatherConfig = await Helpers.LoadConfigFile<Weather, WeatherConfig>();
                weatherUrl.OnNext(weatherConfig.WeatherUndergroundUrl);
                if (currentUrl != weatherConfig.WeatherUndergroundUrl)
                {
                    await UpdateWeatherMiniTile();
                }
            }
            catch (Exception e)
            {
                Log("Exception loading config: {0}", e.Message);
            }
        }

        public override async Task Initialize()
        {
            infoLine.OnNext(defaultMessage);
            await ReloadConfig();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15));

                        await UpdateWeatherMiniTile();
                    }
                    catch (Exception e)
                    {
                        Log("Exception! {0}", e.Message);
                        infoLine.OnNext("❌" + defaultMessage);
                    }
                }
            });
        }

        private async Task UpdateWeatherMiniTile()
        {
            var data = await GetWeatherWunderground();
            var _infoLine = data.Item1 + "°";
            var forecast = data.Item2.Trim().ToLower();
            if (forecast.Contains("partly"))
            {
                _infoLine += "⛅";
            }
            else if (forecast.Contains("rain"))
            {
                _infoLine += "🌧";
            }
            else if (forecast.Contains("cloud"))
            {
                _infoLine += "☁️";
            }
            else if (forecast.Contains("sun"))
            {
                _infoLine += "☀️";
            }
            else if ((forecast.Contains("clear") || forecast.Contains("fair")) && DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20)
            {
                _infoLine += "☀️";
            }
            else
            {
                _infoLine += "🌙";
            }
            Log("Latest update: {0}", _infoLine);
            infoLine.OnNext(_infoLine);
        }

        public override Task InitializeDebug() => Initialize();
    }
}
