using Avalonia.Controls;
using Avalonia.Media;
using CoreTiles.Desktop.ViewModels;
using CoreTiles.Tiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CoreTiles.Desktop.InternalServices
{
    public static class DesignData
    {
        public static WeatherUpdate WeatherUpdate { get; } =
            new WeatherUpdate
            {
                Current = 59,
                Conditions = "Showers in the Vicinity",
                Low = "54°",
                High = "72°",
                TodayForecast = "Partly cloudy. Expect widespread areas of smoke and haze, reducing visibility at times. High 72F. Winds light and variable.",
                Precipitation = "0% Precip.",
                Emoji = "☀️"
            };

        public static TileViewModel TileViewModel { get; } =
            new TileViewModel
            {
                MiniTiles = new ObservableCollection<MenuItem>
                {
                    new MenuItem
                    {
                        Header = "3",
                        Foreground = Brush.Parse("#DC143C"),
                        FontWeight = FontWeight.Bold
                    },
                    new MenuItem
                    {
                        Header = "12:05PM"
                    },
                    new MenuItem
                    {
                        Header = "70°☀️"
                    },
                }
            };

        public static SystemTileViewModel SystemTileViewModel
        {
            get
            {
                var logviewer = new LogViewer
                {
                    DataContext = new LogViewerViewModel(
                        new List<(DateTime, string)>
                        {
                            (DateTime.Now, "Hello this is log"),
                            (DateTime.Now, "hi this is another log"),
                            (DateTime.Now, "bye this is log no more")
                        })
                };
                return new SystemTileViewModel(logviewer)
                {
                    Weather = new WeatherConfig
                    {
                        WeatherUndergroundUrl = "Weather Url"
                    },
                    SystemTileConfig = new SystemTileConfig
                    {
                        ItemsToCache = 50,
                        VideoPlayerLocation = "Video Location"
                    }
                };
            }
        }
    }
}
