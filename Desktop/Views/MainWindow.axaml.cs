using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Diagnostics;
using Avalonia.Native;
using System.Reactive.Linq;
using CoreTiles.Desktop.ViewModels;

namespace CoreTiles.Desktop.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _ = this.GetObservable(Window.DataContextProperty)
                .OfType<MainWindowViewModel>()
                .Subscribe(vm => vm.TileDataTemplate.Subscribe(this.DataTemplates.Add));
        }
    }
}
