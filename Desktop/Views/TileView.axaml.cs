using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Layout;
using System.Collections;
using Desktop.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System;
using System.Collections.ObjectModel;
using Desktop.ViewModels;

namespace Desktop.Views
{
    public static class ExtensionMethods
    {
        public static void GenerateTiles(this Grid grid, int cols, int rows)
        {
            cols.ForEach(i => grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(100 / cols, GridUnitType.Star)
            }));
            rows.ForEach(i => grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(100 / rows, GridUnitType.Star)
            }));
        }

        public static void UpdateTilesSize(this Grid grid, double wHeight, double wWidth)
        {
            int rowCount = grid.RowDefinitions.Count, colCount = grid.ColumnDefinitions.Count;
            foreach (var r in grid.RowDefinitions)
            {
                r.MinHeight = wHeight / rowCount;
            }
            foreach (var c in grid.ColumnDefinitions)
            {
                c.MinWidth = wWidth / colCount;
            }
        }

        public static void ForEach(this int range, Action<int> forEachAction) => Enumerable.Range(0, range)
            .ToList()
            .ForEach(forEachAction);
    }

    public class TileView : UserControl
    {
        private Grid mainGrid => this.FindControl<Grid>("MainGrid");

        public TileView()
        {
            this.InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                var v = (TileView)s;
                var vm = (TileViewModel)v.DataContext;
                mainGrid.GenerateTiles(vm.GridCount.Columns, vm.GridCount.Rows);
            };

            mainGrid.AttachedToVisualTree += (s, e) => mainGrid.UpdateTilesSize(e.Root.ClientSize.Height, e.Root.ClientSize.Width);
            mainGrid.DataContextChanged += (s, e) =>
            {
                var grid = (Grid)s;
                var tiles = (ObservableCollection<Tile>)grid.DataContext;
                if (tiles.Count > 0)
                {
                    int r = 0, c = 0;
                    for (var i = 0; i < 12; i++)
                    {
                        var control = tiles[i].GetControl();
                        Grid.SetColumn(control, c);
                        Grid.SetRow(control, r);
                        mainGrid.Children.Add(control);
                        if (c == 2)
                        {
                            r++;
                            c = 0;
                        }
                        else
                        {
                            c++;
                        }
                    }
                }
            };
            App.ClientSize += (sender, size) => mainGrid.UpdateTilesSize(size.Height, size.Width);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
