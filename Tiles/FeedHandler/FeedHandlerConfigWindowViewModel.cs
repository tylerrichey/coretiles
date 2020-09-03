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
        private const string configName = "FeedHandlerConfig";

        public ObservableCollection<FeedHandlerConfig> Feeds { get; set; } = new ObservableCollection<FeedHandlerConfig>();

        public ReactiveCommand<Unit, Unit> AddItem { get; }

        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public ReactiveCommand<string, Unit> DelItem { get; }

        public ReactiveCommand<Unit, Task> SaveItems { get; }

        public ReactiveCommand<Unit, Task> ViewLogs { get; }

        public Subject<bool> CloseWindow { get; } = new Subject<bool>();

        private string logString;

        public FeedHandlerConfigWindowViewModel()
        {
            
        }

        public FeedHandlerConfigWindowViewModel(List<string> logEntries)
        {
            var logs = new StringBuilder();
            logEntries.Reverse();
            logEntries.ForEach(l => logs.AppendLine(l));
            logString = logs.ToString();

            AddItem = ReactiveCommand.Create(() => Feeds.Add(
                new FeedHandlerConfig
                {
                    CheckEveryMinutes = 15,
                    Regex = ".*"
                }));
            DelItem = ReactiveCommand.Create<string>(url => Feeds.Remove(Feeds.First(f => f.Url == url)));
            SaveItems = ReactiveCommand.Create(async () =>
            {
                await Helpers.SaveConfigFile(Feeds, configName);
                CloseWindow.OnNext(true);
            });
            Cancel = ReactiveCommand.Create(() => CloseWindow.OnNext(true));
            ViewLogs = ReactiveCommand.Create(async () =>
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    //todo perhaps abstract this to tiles.interface to add ez plugin logging???
                    var window = new Window
                    {
                        Height = 400,
                        Width = 900,
                        Content = new ScrollViewer
                        {
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                            Margin = Thickness.Parse("4"),
                            Padding = Thickness.Parse("4"),
                            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                            Content = new TextBlock
                            {
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                                Foreground = Brushes.White,
                                FontSize = 14,
                                Text = logString
                            }
                        }
                    };
                    await window.ShowDialog(desktop.MainWindow);
                }
            });

            ReactiveCommand.Create(async () =>
            {
                try
                {
                    var config = await Helpers.LoadConfigFile<List<FeedHandlerConfig>>(configName);
                    config.ForEach(c => Feeds.Add(c));
                }
                catch
                {
                    AddItem.Execute().Subscribe();
                }
            }).Execute().Subscribe();
        }
    }
}
