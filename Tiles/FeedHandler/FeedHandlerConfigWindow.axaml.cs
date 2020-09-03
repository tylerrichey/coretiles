using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Reactive.Linq;

namespace Tiles.FeedHandler
{
    public class FeedHandlerConfigWindow : Window
    {
        public FeedHandlerConfigWindow()
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
                .OfType<FeedHandlerConfigWindowViewModel>()
                .Subscribe(vm => _ = vm.CloseWindow.Subscribe(reloadHandlers => this.Close(reloadHandlers)));
        }
    }
}
