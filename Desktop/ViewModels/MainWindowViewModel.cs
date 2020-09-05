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
        public MainWindowViewModel(Services services)
        {
            Tiles = new TileViewModel(services);

            services.TilesInitialized.Subscribe(_ =>
            {
                ReactiveCommand.Create(() =>
                {
                    foreach (var tile in services.Tiles)
                    {
                        try
                        {
                            TileDataTemplate.OnNext(tile.DataTemplate);
                        }
                        catch (NotImplementedException)
                        {
                            //do nothing
                        }
                    }
                }).Execute().Subscribe();
            });
        }

        public Subject<IDataTemplate> TileDataTemplate { get; } = new Subject<IDataTemplate>();

        public TileViewModel Tiles { get; }
    }
}
