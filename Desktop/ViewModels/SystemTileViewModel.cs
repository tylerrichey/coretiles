using Avalonia.Controls;
using CoreTiles.Desktop.InternalServices;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.ViewModels
{
    class SystemTileViewModel : ViewModelBase
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

        public SystemTileViewModel(LogViewer logViewer)
        {
            LogViewer = logViewer;

            SaveItems = ReactiveCommand.Create(async () =>
            {
                await Helpers.SaveConfigFile<Weather>(Weather);
                CloseWindow.OnNext(true);
            });

            ReactiveCommand.Create(async () => Weather = await Helpers.LoadConfigFile<Weather, WeatherConfig>()).Execute().Subscribe();
        }
    }
}
