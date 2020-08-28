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
        //todo fix, because obviously hacky but weird behavior occurs if i instantiate at this point, or anywhere after
        Services _services = Program.Services;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(_services)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
