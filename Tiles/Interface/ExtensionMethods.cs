using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CoreTiles.Tiles
{
    public static class ExtensionMethods
    {
        public static void SetLoc(this Control control, Grid grid, int row, int col)
        {
            Grid.SetColumn(control, col);
            Grid.SetRow(control, row);
            grid.Children.Add(control);
        }
    }
}
