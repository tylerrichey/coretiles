using Avalonia.Controls;
using Avalonia.Media;
using CoreTiles.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CoreTiles.Desktop.InternalServices
{
    public static class DesignData
    {
        public static TileViewModel TileViewModel { get; } =
            new TileViewModel
            {
                MiniTiles = new ObservableCollection<MenuItem>
                {
                    new MenuItem
                    {
                        Header = "3",
                        Foreground = Brush.Parse("#DC143C"),
                        FontWeight = FontWeight.Bold
                    },
                    new MenuItem
                    {
                        Header = "12:05PM"
                    },
                    new MenuItem
                    {
                        Header = "70°☀️"
                    },
                }
            };
    }
}
