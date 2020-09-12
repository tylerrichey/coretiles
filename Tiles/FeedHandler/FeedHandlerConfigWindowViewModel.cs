using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Tiles.FeedHandler
{

    public class FeedHandlerConfigWindowViewModel : ReactiveObject
    {
        public ObservableCollection<FeedHandlerConfig> Feeds { get; set; } = new ObservableCollection<FeedHandlerConfig>();

        public ReactiveCommand<Unit, Unit> AddItem { get; }

        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public ReactiveCommand<string, Unit> DelItem { get; }

        public ReactiveCommand<Unit, Task> SaveItems { get; }

        public ReactiveCommand<Unit, Task> ViewLogs { get; }

        public Subject<bool> CloseWindow { get; } = new Subject<bool>();

        public FeedHandlerConfigWindowViewModel() { }

        public FeedHandlerConfigWindowViewModel(LogViewer logViewer)
        {
            AddItem = ReactiveCommand.Create(() => Feeds.Add(
                new FeedHandlerConfig
                {
                    CheckEveryMinutes = 15,
                    Regex = ".*"
                }));
            DelItem = ReactiveCommand.Create<string>(url => Feeds.Remove(Feeds.First(f => f.Url == url)));
            SaveItems = ReactiveCommand.Create(async () =>
            {
                await Helpers.SaveConfigFile<FeedHandler>(Feeds);
                CloseWindow.OnNext(true);
            });
            Cancel = ReactiveCommand.Create(() => CloseWindow.OnNext(false));
            ViewLogs = ReactiveCommand.Create(async () =>
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var window = new Window
                    {
                        Height = 400,
                        Width = 1200,
                        Content = logViewer
                    };
                    await window.ShowDialog(desktop.MainWindow);
                }
            });

            var config = Helpers.GetConfig<FeedHandler, List<FeedHandlerConfig>>();
            if (config.Count == 0)
            {
                AddItem.Execute().Subscribe();
            }
            else
            {
                config.ForEach(c => Feeds.Add(c));
            }
        }
    }
}
