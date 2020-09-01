using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;
using CoreTiles.Desktop.ViewModels;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.Collections;
using System.Collections.Generic;
using System.Data;
using ReactiveUI;
using System.Reactive;

namespace CoreTiles.Desktop.Views
{
    public class TileView : UserControl
    {
        private ScrollViewer scrollViewer => this.FindControl<ScrollViewer>("ScrollViewer");
        private MenuItem itemCounterMenuItem => this.FindControl<MenuItem>("ItemCounterMenuItem");
        private List<IDisposable> disposables = new List<IDisposable>();

        public TileView()
        {
            this.InitializeComponent();

            var dataObv = this.GetObservable(Control.DataContextProperty)
                .OfType<TileViewModel>()
                .Subscribe(vm =>
                {
                    this.AttachedToLogicalTree += (_, e) =>
                    {
                        switch (e.Root)
                        {
                            case MainWindow mainWindow:
                                var clientSizeObv = mainWindow.GetObservable(Window.ClientSizeProperty)
                                    .Subscribe(size =>
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
                                disposables.Add(clientSizeObv);
                                var focusObv = mainWindow.GetObservable(Window.IsFocusedProperty)
                                    .Subscribe(isFocused => vm.AnnounceWindowFocus(isFocused));
                                disposables.Add(focusObv);
                                var mouseObv = mainWindow.GetObservable(Window.IsPointerOverProperty)
                                    .Subscribe(isPointerOver => vm.AnnounceWindowFocus(isPointerOver));
                                break;
                        }
                    };

                    var scrollObv = scrollViewer.GetObservable(ScrollViewer.VerticalScrollBarValueProperty)
                        .Subscribe(v => vm.BufferItems = v != 0);
                    disposables.Add(scrollObv);

                    itemCounterMenuItem.Command = ReactiveCommand.Create(() => scrollViewer.ScrollToHome());
                });
            disposables.Add(dataObv);
        }

        ~TileView() => disposables.ForEach(d => d?.Dispose());

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
