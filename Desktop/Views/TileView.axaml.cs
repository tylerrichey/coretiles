using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;
using CoreTiles.Desktop.ViewModels;
using System.Reactive.Linq;
using System.Threading;

namespace CoreTiles.Desktop.Views
{
    public class TileView : UserControl
    {
        private ScrollViewer scrollViewer => this.FindControl<ScrollViewer>("ScrollViewer");

        public TileView()
        {
            var pressed = false;

            var dataObv = this.GetObservable(Control.DataContextProperty)
                .OfType<TileViewModel>()
                .Subscribe(vm =>
                {
                    this.AttachedToLogicalTree += (s, e) =>
                    {
                        switch (e.Root)
                        {
                            case MainWindow mainWindow:
                                var clientSizeObv = mainWindow.GetObservable(Window.ClientSizeProperty);
                                clientSizeObv.Subscribe(size =>
                                {
                                    var noWiderThan = 500;
                                    var padding = 8;
                                    foreach (var i in Enumerable.Range(1, 10))
                                    {
                                        var target = size.Width / i;
                                        if (target <= noWiderThan)
                                        {
                                            vm.ItemWidth = target - padding;
                                            vm.Columns = i;
                                            break;
                                        }
                                    }
                                });
                                break;
                        }
                    };

                    //todo probably make this more robust
                    this.LayoutUpdated += (s, e) =>
                    {
                        if (!pressed && !scrollViewer.IsPointerOver)
                        {
                            scrollViewer.ScrollToEnd();
                        }
                    };
                    scrollViewer.PointerPressed += (s, e) => pressed = true;
                    scrollViewer.PointerReleased += (s, e) =>
                    {
                        Thread.Sleep(5000);
                        pressed = false;
                    };
                });

            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
