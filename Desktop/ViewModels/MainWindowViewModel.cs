using Avalonia.Controls.Templates;
using CoreTiles.Desktop.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Text;

namespace CoreTiles.Desktop.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(Services services) => Tiles = new TileViewModel(services);

        public TileViewModel Tiles { get; }
    }
}
