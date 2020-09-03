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
    public class Weather : Tile
    {
        //todo implement tile for changes?
        public override IDataTemplate DataTemplate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //todo config
        private const string weatherUrl = @"https://www.wunderground.com/weather/us/wa/seattle/KWASEATT2097";
        private Subject<string> infoLine { get; } = new Subject<string>();
        public override MenuItem MiniTile => new MenuItem
        {
            [!MenuItem.HeaderProperty] = infoLine.ToBinding(),
            CommandParameter = weatherUrl,
            Command = ReactiveCommand.Create<string>(url => Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            }))
        };

        private async Task<(int, string)> GetWeatherWunderground()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, weatherUrl);
            req.Headers.Add("User-Agent", "curl");
            var response = await Helpers.HttpClient.SendAsync(req);
            var data = await response.Content.ReadAsStringAsync();
            var page = new HtmlAgilityPack.HtmlDocument();
            page.LoadHtml(data);
            var temp = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[2]/div/div/div[2]/lib-display-unit/span/span[1]").InnerHtml;
            var conditions = page.DocumentNode.SelectSingleNode("/html/body/app-root/app-today/one-column-layout/wu-header/sidenav/mat-sidenav-container/mat-sidenav-content/div/section/div[3]/div[1]/div/div[1]/div[1]/lib-city-current-conditions/div/div[3]/div/div[1]/p").InnerHtml;
            return (Convert.ToInt32(temp), conditions);
        }

        public override Task Initialize()
        {
            infoLine.OnNext("No Weather Data");
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var _infoLine = string.Empty;
                        var data = await GetWeatherWunderground();
                        _infoLine = data.Item1 + "°";
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
                        infoLine.OnNext(_infoLine);
                    }
                    catch
                    {
                        var last = await infoLine.LastOrDefaultAsync();
                        infoLine.OnNext("❌" + last);
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15));
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
