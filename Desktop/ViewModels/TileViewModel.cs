using Avalonia.Controls;
using Desktop.Models;
using Desktop.Tiles;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Desktop.ViewModels
{
    public class TileViewModel : ViewModelBase
    {
        public TileViewModel(Services services)
        {
            Items = new ObservableCollection<Tile>(Enumerable.Range(0, ViewableItemCount)
                .Select(i => Tile.New()));

            Process.Execute()
                .Subscribe();
        }

        public ReactiveCommand<Unit, Task> Process => ReactiveCommand.Create(async () =>
        {
            int r = 0, c = 0;
            while (true)
            {
                if (TileQueue.Items.TryDequeue(out Tile tile))
                {
                    if (Items.Count == ViewableItemCount)
                    {
                        Items.RemoveAt(0);
                    }
                    Items.Add(tile);
                }

                await Task.Delay(10);
            }
        });

        public ObservableCollection<Tile> Items { get; }
        public GridCountModel GridCount = new GridCountModel { Columns = 3, Rows = 4 };

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
