using Avalonia.Controls;
using CoreTiles.Desktop.Tiles;
using CoreTiles.Desktop.Views;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.ViewModels
{
    //todo add way to randomize color bound to properties on this viewmodel
    public class TileViewModel : ViewModelBase
    {
        public ObservableCollection<Tile> Items { get; }

        private int itemsToCache = 100;
        private Services _services;

        private double itemWidth = 300;
        public double ItemWidth
        {
            get => itemWidth;
            set => this.RaiseAndSetIfChanged(ref itemWidth, value);
        }

        public TileViewModel(Services services)
        {
            _services = services;
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
                            Items.RemoveAt(0);
                        }
                        Items.Add(tile);
                    }
                }

                await Task.Delay(10);
            }
        });
    }
}
