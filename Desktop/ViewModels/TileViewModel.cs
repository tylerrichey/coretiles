﻿using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using CoreTiles.Desktop.InternalServices;
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
        public ObservableCollection<TileData> Items { get; } = new ObservableCollection<TileData>();
        public ObservableCollection<TileData> ItemsBuffer { get; } = new ObservableCollection<TileData>();
        public ObservableCollection<MenuItem> MiniTiles { get; internal set; } = new ObservableCollection<MenuItem>();
        public Subject<bool> ScrollToHome { get; } = new Subject<bool>();

        //todo make global config item and/or implement lazy loading?
        private const int itemsToCache = 50;
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

            _services.Tiles
                .ForEach(t => MiniTiles.Add(t.MiniTile));

            ReactiveCommand.Create(async () =>
            {
                foreach (var tile in _services.Tiles)
                {
                    await tile.Initialize();
                    if (tile is SystemTile systemTile)
                    {
                        systemTile.SetServices(ref _services);
                    }
                }
                _services.TilesInitialized.OnNext(true);
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
        });

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
