using Avalonia.Controls;
using Avalonia.Media;
using CoreTiles.Desktop.Tiles;
using CoreTiles.Desktop.Views;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.ViewModels
{
    //todo add way to randomize color bound to properties on this viewmodel
    public class TileViewModel : ViewModelBase
    {
        public ObservableCollection<Tile> Items { get; }

        private int itemsToCache = 250;
        private Services _services;

        private double itemWidth = 300;
        public double ItemWidth
        {
            get => itemWidth;
            set => this.RaiseAndSetIfChanged(ref itemWidth, value);
        }

        private IBrush windowBackground = Brush.Parse("#212529");
        public IBrush WindowBackground
        {
            get => windowBackground;
            set => this.RaiseAndSetIfChanged(ref windowBackground, value);
        }

        //todo this doesn't fall to tiles from here, re-factor
        private IBrush tileBackground = Brush.Parse("#495057");
        public IBrush TileBackground
        {
            get => tileBackground;
            set => this.RaiseAndSetIfChanged(ref tileBackground, value);
        }

        private string weatherData = "No Weather Data";
        public string WeatherData
        {
            get => weatherData;
            set => this.RaiseAndSetIfChanged(ref weatherData, value);
        }

        public TileViewModel(Services services)
        {
            _services = services;
            _services.Weather.StartMonitoring()
                .Subscribe(s => WeatherData = s);
            Items = new ObservableCollection<Tile>();
            
            Process.Execute()
                .Subscribe();
        }

        public ReactiveCommand<Unit, Task> Process => ReactiveCommand.Create(async () =>
        {
            while (true)
            {
                foreach (var p in _services.Tiles)
                {
                    if (p.TileQueue.TryDequeue(out Tile tile))
                    {
                        if (Items.Count == itemsToCache)
                        {
                            //todo hard coded columns
                            Enumerable.Range(0, 3)
                                .ToList()
                                .ForEach(i => Items.RemoveAt(0));
                        }
                        Items.Add(tile);
                    }
                }

                await Task.Delay(10);
            }
        });
    }
}
