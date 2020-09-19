using Avalonia.Controls;
using CoreTiles.Desktop.InternalServices;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.ViewModels
{
    public class SystemTileViewModel : ViewModelBase
    {
        public LogViewer LogViewer { get; }
        public ReactiveCommand<Unit, Task> SaveItems { get; }
        public Subject<bool> CloseWindow { get; } = new Subject<bool>();

        private WeatherConfig weatherConfig = new WeatherConfig();
        public WeatherConfig Weather
        {
            get => weatherConfig;
            set => this.RaiseAndSetIfChanged(ref weatherConfig, value);
        }

        private SystemTileConfig systemTileConfig;
        public SystemTileConfig SystemTileConfig
        {
            get => systemTileConfig;
            set => this.RaiseAndSetIfChanged(ref systemTileConfig, value);
        }

        public SystemTileViewModel(LogViewer logViewer)
        {
            LogViewer = logViewer;

            SaveItems = ReactiveCommand.Create(async () =>
            {
                var saves = new Collection<Task>
                {
                    Helpers.SaveConfigFile<Weather>(Weather),
                    Helpers.SaveConfigFile<SystemTile>(SystemTileConfig)
                };
                await Task.WhenAll(saves);

                CloseWindow.OnNext(true);
            });

            Weather = Helpers.GetConfig<Weather, WeatherConfig>();
            SystemTileConfig = Helpers.GetConfig<SystemTile, SystemTileConfig>();
        }
    }
}
