using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;
using CoreTiles.Desktop.ViewModels;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using Avalonia.Markup.Xaml.Templates;
using CoreTiles.Desktop.Tiles;
using Avalonia.Data;
using System.Collections;
using Avalonia.Controls.Generators;
using System.Data;
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
                                clientSizeObv.Subscribe(size => vm.ItemWidth = (size.Width / 3) - 8);
                                break;
                        }
                    };

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
