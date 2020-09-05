using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CoreTiles.Tiles;
using System.Reactive.Linq;

namespace Tiles.Twitter
{
    public class TwitterConfigWindow : Window
    {
        public TwitterConfigWindow()
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
                .OfType<TwitterConfigViewModel>()
                .Subscribe(vm => _ = vm.CloseWindow.Subscribe(_ => this.Close()));
        }
    }
}
