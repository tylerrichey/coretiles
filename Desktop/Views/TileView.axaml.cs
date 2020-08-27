using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;
using CoreTiles.Desktop.ViewModels;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Avalonia.Markup.Xaml.Templates;
using CoreTiles.Desktop.Tiles;
using Avalonia.Data;
using System.Collections;
using Avalonia.Controls.Generators;

namespace CoreTiles.Desktop.Views
{
    public static class ExtensionMethods
    {
        public static void GenerateGrid(this Grid grid, int cols, int rows)
        {
            cols.ForEach(i => grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(100 / cols, GridUnitType.Star)
            }));
            //rows.ForEach(i => grid.RowDefinitions.Add(new RowDefinition
            //{
            //    Height = new GridLength(100 / rows, GridUnitType.Star)
            //}));

            cols.ForEach(i =>
            {
                var itemsControl = new ItemsControl
                {
                    ItemTemplate = new DataTemplate
                    {
                        Content = new BaseTile()
                    },
                    [!ItemsControl.ItemsProperty] = new Binding("Items")
                };
                var stackPanel = new StackPanel();
                stackPanel.Children.Add(itemsControl);
                Grid.SetColumn(stackPanel, i);
                grid.Children.Add(stackPanel);
            });
        }

        //public static void UpdateTilesSize(this Grid grid, double wHeight, double wWidth)
        //{
        //    int rowCount = grid.RowDefinitions.Count, colCount = grid.ColumnDefinitions.Count;
        //    foreach (var r in grid.RowDefinitions)
        //    {
        //        r.MinHeight = wHeight / rowCount;
        //    }
        //    foreach (var c in grid.ColumnDefinitions)
        //    {
        //        c.MinWidth = wWidth / colCount;
        //    }
        //}

        public static void ForEach(this int range, Action<int> forEachAction) => Enumerable.Range(0, range)
            .ToList()
            .ForEach(forEachAction);
    }

    public class TileView : UserControl
    {
        private Grid mainGrid => this.FindControl<Grid>("MainGrid");

        public TileView()
        {
            //var dataContextObv = this.GetObservable(Window.DataContextProperty)
            //    .OfType<TileViewModel>();
            //dataContextObv.Subscribe(vm =>
            //{
            //    mainGrid.GenerateGrid(vm.GridCount.Columns, vm.GridCount.Rows);

            //    //    var gridDataObv = mainGrid.GetObservable(Window.DataContextProperty)
            //    //        .OfType<ObservableCollection<Control>>();
            //    //    gridDataObv.Subscribe(tile =>
            //    //    {
            //    //        int r = 0, c = 0;
            //    //        for (var i = 0; i < (vm.GridCount.Rows * vm.GridCount.Columns) - 1; i++)
            //    //        {
            //    //            Grid.SetColumn(tile[i], c);
            //    //            Grid.SetRow(tile[i], r);
            //    //            mainGrid.Children.Add(tile[i]);
            //    //            if (c == vm.GridCount.Rows - 1)
            //    //            {
            //    //                r++;
            //    //                c = 0;
            //    //            }
            //    //            else
            //    //            {
            //    //                c++;
            //    //            }
            //    //        }
            //    //    });
            //});

            //mainGrid.AttachedToVisualTree += (s, e) => mainGrid.UpdateTilesSize(e.Root.ClientSize.Height, e.Root.ClientSize.Width);

            //this.AttachedToLogicalTree += (s, e) =>
            //{
            //    switch (e.Root)
            //    {
            //        case MainWindow mainWindow:
            //            var clientSizeObv = mainWindow.GetObservable(Window.ClientSizeProperty);
            //            clientSizeObv.Subscribe(size => mainGrid.UpdateTilesSize(size.Height, size.Width));
            //            break;
            //    }
            //};

            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
