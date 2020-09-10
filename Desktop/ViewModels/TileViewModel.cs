using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using CoreTiles.Desktop.InternalServices;
using CoreTiles.Tiles;
using ReactiveUI;
using Serilog;
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
    public class TileViewModel : ViewModelBase
    {
        public ObservableCollection<TileData> Items { get; } = new ObservableCollection<TileData>();
        public List<TileData> ItemsBuffer { get; } = new List<TileData>();
        public ObservableCollection<MenuItem> MiniTiles { get; internal set; } = new ObservableCollection<MenuItem>();
        public Subject<bool> ScrollToHome { get; } = new Subject<bool>();
        public ObservableCollection<IDataTemplate> TileDataTemplate { get; } = new ObservableCollection<IDataTemplate>();

        //todo make global config item and/or implement lazy loading?
        private const int itemsToCache = 75;
        private Services _services;
        private ILogger logger = Log.ForContext<TileViewModel>();

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

        private int newItemCounter;
        public int NewItemCounter
        {
            get => newItemCounter;
            set => this.RaiseAndSetIfChanged(ref newItemCounter, value);
        }

        internal TileViewModel() { }

        public TileViewModel(Services services)
        {
            _services = services;

            MiniTiles.Add(new MenuItem
            {
                [!MenuItem.HeaderProperty] = new Binding("NewItemCounter"),
                [!MenuItem.IsVisibleProperty] = new Binding("NewItemCounter"),
                Foreground = Brush.Parse("#DC143C"),
                FontWeight = FontWeight.Bold,
                Command = ReactiveCommand.Create(() => ScrollToHome.OnNext(true))
            });

            _services.Tiles
                .ForEach(t => MiniTiles.Add(t.MiniTile));

            _ = Task.Run(async () =>
            {
                try
                {
                    foreach (var tile in _services.Tiles)
                    {
                        var initTasks = new Collection<Task>();
                        if (_services.LoadMockData)
                        {
                            initTasks.Add(tile.InitializeDebug());
                        }
                        else
                        {
                            initTasks.Add(tile.Initialize());
                        }

                        try
                        {
                            TileDataTemplate.Add(tile.DataTemplate);
                            logger.Verbose("Loaded {tileName} tile data template.", tile.GetType().Name);
                        }
                        catch (NotImplementedException)
                        {
                            logger.Information("Tile {tileName} has no data template.", tile.GetType().Name);
                        }

                        if (tile is SystemTile systemTile)
                        {
                            systemTile.SetServices(ref _services);
                        }

                        await Task.WhenAll(initTasks);
                    }
                }
                catch (AggregateException ae)
                {
                    throw ae.Flatten().InnerException;
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Exception initializing tiles!");
                }
            });

            _ = Task.Run(async () =>
            {
                try
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
                            if (p.TileQueue.TryDequeue(out TileData tile))
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

                        await Task.Delay(50);
                    }
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Exception processing new tiles!");
                }
            });
        }

        public int Columns = 3;
        public bool BufferItems = false;

        private bool windowFocused;
        public void AnnounceWindowFocus(bool isFocused)
        {
            windowFocused = isFocused;
            NewItemCounter = windowFocused && ItemsBuffer.Count == 0 ? 0 : NewItemCounter;
        }

        private void InsertNewTile(TileData tile)
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
