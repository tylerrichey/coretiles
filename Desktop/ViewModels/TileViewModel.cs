using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.ViewModels
{
    //todo something with randomizing the color pallete
    public class TileViewModel : ViewModelBase
    {
        public ObservableCollection<Tile> Items { get; }
        public ObservableCollection<Tile> ItemsBuffer { get; }

        private int itemsToCache = 100;
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

        private string weatherData = "No Weather Data";
        public string WeatherData
        {
            get => weatherData;
            set => this.RaiseAndSetIfChanged(ref weatherData, value);
        }

        private string timeDisplay = DateTime.Now.ToShortTimeString().Replace(" ", "");
        public string TimeDisplay
        {
            get => timeDisplay;
            set => this.RaiseAndSetIfChanged(ref timeDisplay, value);
        }

        private int newItemCounter;
        public int NewItemCounter
        {
            get => newItemCounter;
            set => this.RaiseAndSetIfChanged(ref newItemCounter, value);
        }

        public ObservableCollection<MenuItem> MiniTiles { get; }

        private readonly Timer timer;

        public TileViewModel(Services services)
        {
            _services = services;
            _services.Weather.StartMonitoring()
                .Subscribe(s => WeatherData = s);
            Items = new ObservableCollection<Tile>();
            ItemsBuffer = new ObservableCollection<Tile>();
            MiniTiles = new ObservableCollection<MenuItem>();

            var then = DateTime.Now.AddSeconds(1);
            timer = new Timer((s) => TimeDisplay = DateTime.Now.ToShortTimeString().Replace(" ", "")
                ,null, new DateTime(then.Year, then.Month, then.Day, then.Hour, then.Minute, then.Second) - DateTime.Now
                ,TimeSpan.FromMinutes(1));

            Process.Execute()
                .Subscribe();

            MiniTiles.Add(new MenuItem
            {
                [!MenuItem.HeaderProperty] = new Binding("NewItemCounter"),
                [!MenuItem.IsVisibleProperty] = new Binding("NewItemCounter"),
                Foreground = Brush.Parse("#DC143C"),
                FontWeight = FontWeight.Bold
            });
            MiniTiles.Add(new MenuItem
            {
                [!MenuItem.HeaderProperty] = new Binding("TimeDisplay")
            });
            MiniTiles.Add(new MenuItem
            {
                [!MenuItem.HeaderProperty] = new Binding("WeatherData")
            });

            _services.Tiles
                .ForEach(t => MiniTiles.Add(t.MiniTile));

            ReactiveCommand.Create(async () =>
            {
                foreach (var tile in _services.Tiles)
                {
                    await tile.Initialize();
                }
            }).Execute().Subscribe();
        }

        public int Columns = 3;
        public bool BufferItems = false;

        private bool windowFocused;
        public void AnnounceWindowFocus(bool isFocused)
        {
            windowFocused = isFocused;
            NewItemCounter = windowFocused && ItemsBuffer.Count == 0 ? 0 : NewItemCounter;
        }

        public ReactiveCommand<Unit, Task> Process => ReactiveCommand.Create(async () =>
        {
            while (true)
            {
                if (!BufferItems && ItemsBuffer.Any())
                {
                    ItemsBuffer.ToList()
                        .ForEach(InsertNewTile);
                    ItemsBuffer.Clear();
                    NewItemCounter = 0;
                }

                foreach (var p in _services.Tiles)
                {
                    if (p.TileQueue.TryDequeue(out Tile tile))
                    {
                        if (BufferItems)
                        {
                            ItemsBuffer.Add(tile);
                            NewItemCounter++;
                        }
                        else
                        {
                            InsertNewTile(tile);
                            NewItemCounter = windowFocused ? 0 : NewItemCounter + 1;
                        }
                    }
                }

                await Task.Delay(10);
            }
        });

        private void InsertNewTile(Tile tile)
        {
            if (Items.Count == itemsToCache)
            {
                Enumerable.Range(itemsToCache - 1 - Columns, Columns)
                    .ToList()
                    .ForEach(i => Items.RemoveAt(i));
            }
            Items.Insert(0, tile);
        }
    }
}
