using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CoreTiles.Desktop.ViewModels;
using CoreTiles.Desktop.Views;
using System;

namespace CoreTiles.Desktop
{
    public class App : Application
    {
        private static Services services;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                services = new Services();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(services)
                };
                //desktop.Exit += (s, e) => services.Tiles.ForEach(t => t.Dispose());
            }

            base.OnFrameworkInitializationCompleted();
        }

        
    }
}
