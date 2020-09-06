using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;
using CoreTiles.Desktop.ViewModels;
using System.Reactive.Linq;
using Avalonia.Controls.Templates;

namespace CoreTiles.Desktop.Views
{
    public class TileView : UserControl
    {
        private ScrollViewer scrollViewer => this.FindControl<ScrollViewer>("ScrollViewer");

        public TileView()
        {
            this.InitializeComponent();

            var dataObv = this.GetObservable(Control.DataContextProperty)
                .OfType<TileViewModel>()
                .Subscribe(vm =>
                {
                    vm.TileDataTemplate.CollectionChanged += (s, e) =>
                    {
                        foreach (var item in e.NewItems)
                        {
                            var tile = item as IDataTemplate;
                            this.DataTemplates.Add(tile);
                        }
                    };

                    this.AttachedToLogicalTree += (_, e) =>
                    {
                        switch (e.Root)
                        {
                            case MainWindow mainWindow:
                                var clientSizeObv = mainWindow.GetObservable(Window.ClientSizeProperty)
                                    .Subscribe(size =>
                                    {
                                        const int noWiderThan = 500;
                                        const int padding = 8;
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
                                var focusObv = mainWindow.GetObservable(Window.IsFocusedProperty)
                                    .Subscribe(isFocused => vm.AnnounceWindowFocus(isFocused));
                                var mouseObv = mainWindow.GetObservable(Window.IsPointerOverProperty)
                                    .Subscribe(isPointerOver => vm.AnnounceWindowFocus(isPointerOver));
                                break;
                        }
                    };

                    var scrollObv = scrollViewer.GetObservable(ScrollViewer.VerticalScrollBarValueProperty)
                        .Subscribe(v => vm.BufferItems = v != 0);

                    var scrollHomeObv = vm.ScrollToHome.Subscribe(_ => scrollViewer.ScrollToHome());
                });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
