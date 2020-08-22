using Desktop.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(Services services)
        {
            Tiles = new TileViewModel();
        }

        public TileViewModel Tiles { get; }
    }
}
