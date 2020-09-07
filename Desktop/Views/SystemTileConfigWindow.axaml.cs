using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Reactive.Linq;
using CoreTiles.Desktop.ViewModels;

namespace CoreTiles.Desktop.Views
{
    public class SystemTileConfigWindow : Window
    {
        public SystemTileConfigWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _ = this.GetObservable(Window.DataContextProperty)
                .OfType<SystemTileViewModel>()
                .Subscribe(vm => _ = vm.CloseWindow.Subscribe(_ => this.Close()));
        }
    }
}
