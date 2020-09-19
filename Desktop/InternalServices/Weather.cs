using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using CoreTiles.Desktop.Views;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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

    public class WeatherUpdate
    {
        public string Conditions { get; set; }
        public int Current { get; set; }
        public string CurrentString => Current + "°";
        public string FullConditions => Precipitation + @" - " + TodayForecast;
        public string Low { get; set; }
        public string High { get; set; }
        public string TodayForecast { get; set; }
        public string Precipitation { get; set; }
        public string Emoji { get; set; }
    }

    public class Weather : Tile
    {
        public override IDataTemplate DataTemplate { get; } = new FuncDataTemplate<WeatherUpdate>((f, s) => new WeatherTileView { DataContext = f });

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

        public override Type ConfigType => typeof(WeatherConfig);

        private async Task<WeatherUpdate> GetWeatherWunderground()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, weatherConfig.WeatherUndergroundUrl);
            req.Headers.Add("User-Agent", "curl");
            var response = await Helpers.HttpClient.SendAsync(req);
            var data = await response.Content.ReadAsStringAsync();
            var page = new HtmlAgilityPack.HtmlDocument();
            page.LoadHtml(data);
            var temp = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[2]/div/div/div[2]/lib-display-unit/span/span[1]").InnerText;
            var conditions = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[3]/div/div[1]/p").InnerText;
            var high = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[2]/div/div/div[1]/span[1]").InnerText;
            var low = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[2]/div/div/div[1]/span[3]").InnerText;
            var precip = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[3]/div/lib-city-today-forecast/div/div[1]/div/div/div/a[1]").InnerText;
            var today = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[3]/div/lib-city-today-forecast/div/div[1]/div/div/div/a[2]").InnerText;
            return new WeatherUpdate
            {
                Current = Convert.ToInt32(temp),
                Conditions = conditions,
                High = high,
                Low = low,
                Precipitation = WebUtility.HtmlDecode(precip),
                TodayForecast = WebUtility.HtmlDecode(today)
            };
        }

        public async Task ReloadConfig()
        {
            try
            {
                var currentUrl = weatherConfig.WeatherUndergroundUrl;
                weatherConfig = Helpers.GetConfig<Weather, WeatherConfig>();
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

        private int lastTemp;
        private async Task UpdateWeatherMiniTile()
        {
            var data = await GetWeatherWunderground();
            var forecast = data.Conditions.Trim().ToLower();
            string emoji;
            if (forecast.Contains("partly"))
            {
                emoji = "⛅";
            }
            else if (forecast.Contains("rain"))
            {
                emoji = "🌧";
            }
            else if (forecast.Contains("cloud"))
            {
                emoji = "☁️";
            }
            else if (forecast.Contains("sun"))
            {
                emoji = "☀️";
            }
            else if ((forecast.Contains("clear") || forecast.Contains("fair")) && DateTime.Now.Hour > 6 && DateTime.Now.Hour < 20)
            {
                emoji = "☀️";
            }
            else if (DateTime.Now.Hour <= 6 && DateTime.Now.Hour >= 20)
            {
                emoji = "🌙";
            }
            else
            {
                emoji = "⛅";
            }
            var miniTileText = data.Current + "°" + emoji;
            Log("Latest update: {0}", miniTileText);
            infoLine.OnNext(miniTileText);
            data.Emoji = emoji;
            if (lastTemp != data.Current)
            {
                PushTileData(data);
            }
            lastTemp = data.Current;
        }

        public override Task InitializeDebug() => Initialize();
    }
}
