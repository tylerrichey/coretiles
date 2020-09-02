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
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.ViewModels
{
    //todo something with randomizing the color pallete
    public class TileViewModel : ViewModelBase
    {
        public ObservableCollection<Tile> Items { get; } = new ObservableCollection<Tile>();
        public ObservableCollection<Tile> ItemsBuffer { get; } = new ObservableCollection<Tile>();
        public ObservableCollection<MenuItem> MiniTiles { get; } = new ObservableCollection<MenuItem>();
        public Subject<bool> ScrollToHome { get; } = new Subject<bool>();

        private const int itemsToCache = 35;
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
        public string TimeDisplay => DateTime.Now.ToShortTimeString().Replace(" ", "");

        private int newItemCounter;
        public int NewItemCounter
        {
            get => newItemCounter;
            set => this.RaiseAndSetIfChanged(ref newItemCounter, value);
        }

        public TileViewModel(Services services)
        {
            _services = services;
            _services.Weather.StartMonitoring()
                .Subscribe(s => WeatherData = s);

            //this feels wrong, but is effective
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    this.RaisePropertyChanged(nameof(TimeDisplay));
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

            Process.Execute()
                .Subscribe();
            //todo some kind of app wide error handling, this does nothing
            Process.ThrownExceptions.Subscribe(e => throw e);

            MiniTiles.Add(new MenuItem
            {
                [!MenuItem.HeaderProperty] = new Binding("NewItemCounter"),
                [!MenuItem.IsVisibleProperty] = new Binding("NewItemCounter"),
                Foreground = Brush.Parse("#DC143C"),
                FontWeight = FontWeight.Bold,
                Command = ReactiveCommand.Create(() => ScrollToHome.OnNext(true))
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
                    .Reverse()
                    .ToList()
                    .ForEach(i => Items.RemoveAt(i));
            }
            Items.Insert(0, tile);
        }
    }
}
