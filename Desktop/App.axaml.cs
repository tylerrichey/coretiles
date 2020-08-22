using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Desktop.ViewModels;
using Desktop.Views;
using System;

namespace Desktop
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new Services();
                
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(services)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
