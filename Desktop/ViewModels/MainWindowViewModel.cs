using CoreTiles.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreTiles.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(Services services) => Tiles = new TileViewModel(services);

        public TileViewModel Tiles { get; }
    }
}
