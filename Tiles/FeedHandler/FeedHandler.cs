using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using CoreTiles.Tiles;
using FeedParserCore;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Tiles.FeedHandler
{
    public class FeedHandler : Tile
    {
        public override IDataTemplate DataTemplate { get; } = new FuncDataTemplate<FeedItem>((f, s) =>
            new FeedHandlerTile { DataContext = new FeedHandlerViewModel(f) });

        public override MenuItem MiniTile => new MenuItem
        {
            [!MenuItem.HeaderProperty] = menuTileString.ToBinding(),
            Foreground = Brush.Parse("#ff4500"),
            Command = ReactiveCommand.Create(async () =>
            {
                var window = new FeedHandlerConfigWindow
                {
                    DataContext = new FeedHandlerConfigWindowViewModel(GetLogViewerControl())
                };
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    && await window.ShowDialog<bool>(desktop.MainWindow))
                {
                    Log("Restarting feed handlers...");
                    cancellationTokenSource.Cancel();
                    while (tasks.Any(t => t.Status == TaskStatus.Running))
                    {
                        Log("Waiting for tasks to cancel...");
                        await Task.Delay(500);
                    }
                    cancellationTokenSource = new CancellationTokenSource();
                    tasks.Clear();
                    InitializeFeedHandlers();
                }
            })
        };

        public override Type ConfigType => typeof(List<FeedHandlerConfig>);

        public override Task Initialize()
        {
            FeedParser.SetHttpClient(Helpers.HttpClient);
            InitializeFeedHandlers();
            return Task.CompletedTask;
        }

        private Subject<string> menuTileString = new Subject<string>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private List<Task> tasks = new List<Task>();

        private void InitializeFeedHandlers()
        {
			menuTileString.OnNext("❌Feeds!");
            _ = Task.Run(() =>
            {
                var config = Helpers.GetConfig<FeedHandler, List<FeedHandlerConfig>>();

                foreach (var f in config.Where(c => c.Enabled))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var lastSuccessfulCheck = DateTime.Now;
                        while (true)
                        {
                            try
                            {
                                //todo make type a config option?
                                var feed = await FeedParser.ParseAsync(f.Url, FeedType.Atom);

                                var items = feed.Where(i => i.PublishDate > lastSuccessfulCheck && Regex.IsMatch(i.Title, f.Regex))
                                    .ToList();
                                if (items.Count > 0)
                                {
                                    foreach (var i in items)
                                    {
                                        PushTileData(i);
                                    }
                                    lastSuccessfulCheck = items.Max(i => i.PublishDate);
                                }
                                Log($"Successful for {f.Url} - Retrieved: {feed.Count()} - Displayed: {items.Count}");
                                menuTileString.OnNext("✔️Feeds!");
                            }
                            catch (Exception e)
                            {
                                Log($"Exception for {f.Url} - {e.Message}");
                                menuTileString.OnNext("❌Feeds!");
                            }

                            await Task.Delay(TimeSpan.FromMinutes(f.CheckEveryMinutes), cancellationTokenSource.Token);
                        }
                    }, cancellationTokenSource.Token));
                }
            });
        }

        //public override Task InitializeDebug() => Initialize();
        public override Task InitializeDebug()
        {
            PushTileData(new FeedItem
            {
                Title = "Title",
                Link = "https://www.google.com",
                Content = "Content",
                PublishDate = DateTime.Now
            });
            return Task.CompletedTask;
        }
    }
}
