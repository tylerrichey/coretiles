using Avalonia.Controls;
using Desktop.Models;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Desktop.ViewModels
{
    public class TileViewModel : ViewModelBase
    {
        public TileViewModel()
        {
            Items = new ObservableCollection<Tile>();

            var processQueue = Task.Run(async () =>
            {
                if (TileQueue.Items.TryDequeue(out Tile tile))
                {
                    //todo only if exists
                    Items.RemoveAt(0);
                    Items.Add(tile);
                }
            });
        }

        public ObservableCollection<Tile> Items { get; }
        public GridCountModel GridCount = new GridCountModel { Columns = 3, Rows = 4 };
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
