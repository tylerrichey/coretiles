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
    public class TileViewModel : ViewModelBase
    {
        private Services _services;

        public TileViewModel(Services services)
        {
            _services = services;
            Items = new Dictionary<int, ObservableCollection<Tile>>();
            GridCount.Columns.ForEach(i => Items.Add(i, new ObservableCollection<Tile>()));
            
            Process.Execute()
                .Subscribe();
        }

        public ReactiveCommand<Unit, Task> Process => ReactiveCommand.Create(async () =>
        {
            int r = 0, c = 0;
            while (true)
            {
                foreach (var p in _services.Tiles)
                {
                    if (p.TileQueue.TryDequeue(out Tile tile))
                    {
                        Items[c].Add(tile);
                        c = c + 1 > GridCount.Columns - 1 ? 0 : c + 1;
                    }
                }

                await Task.Delay(10);
            }
        });

        public Dictionary<int, ObservableCollection<Tile>> Items { get; }
        public GridCountModel GridCount = new GridCountModel { Columns = 2, Rows = 1 };

        private int ViewableItemCount => GridCount.Columns * GridCount.Rows;
    }

    public class GridCountModel : ReactiveObject
    {
        private int columns, rows;

        public int Columns
        {
            get => columns;
            set => this.RaiseAndSetIfChanged(ref columns, value);
        }

        public int Rows
        {
            get => rows;
            set => this.RaiseAndSetIfChanged(ref rows, value);
        }
    }
}
