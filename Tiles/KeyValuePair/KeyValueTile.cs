using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using CoreTiles.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{
    //public class KeyValueTile : Tile
    //{
    //    public string Key { get; set; }
    //    public string Value { get; set; }
    //    public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<KeyValueTile>((t, s) =>
    //    {
    //        var keyLabel = new TextBlock() { Text = "Key:" };
    //        var key = new TextBlock() { Text = t.Key };
    //        var valueLabel = new TextBlock() { Text = "Value: " };
    //        var value = new TextBlock() { Text = t.Value };
    //        var grid = new Grid() { Background = Brushes.Coral };
    //        keyLabel.SetLoc(grid, 0, 0);
    //        key.SetLoc(grid, 0, 1);
    //        valueLabel.SetLoc(grid, 1, 0);
    //        value.SetLoc(grid, 1, 1);
    //        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
    //        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
    //        grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Auto));
    //        grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Auto));
    //        return grid;
    //    });
    //    public override IDataTemplate MiniTile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    //    public override Window GetConfigurationWindow()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override async Task Initialize()
    //    {
    //        await Task.CompletedTask;

    //        //todo have this start a simple http server for webhooks?
    //        //need some external messaging source

    //        ////testing
    //        //var mock = Task.Run(async () =>
    //        //{
    //        //    while (true)
    //        //    {

    //        //        TileQueue.Enqueue(new KeyValueTile
    //        //        {
    //        //            Key = "Hello",
    //        //            Value = "World!"
    //        //        });
    //        //        await Task.Delay(TimeSpan.FromSeconds(3));
    //        //    }
    //        //});
    //    }
    //}
}
