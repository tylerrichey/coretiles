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
        public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<FeedHandler>((f, s) =>
            new FeedHandlerTile { DataContext = new FeedHandlerViewModel(f.CurrentItem) });

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
                    Log($"Restarting feed handlers...");
                    InitializeFeedHandlers();
                }
            })
        };

        public FeedItem CurrentItem { get; }

        public FeedHandler(FeedItem feedItem) => CurrentItem = feedItem;
        public FeedHandler() { }

        public override Task Initialize()
        {
            FeedParser.SetHttpClient(Helpers.HttpClient);

#if DEBUG
            //return Task.CompletedTask;
#endif

            InitializeFeedHandlers();

            return Task.CompletedTask;
        }

        private Subject<string> menuTileString = new Subject<string>();
        private List<Task> feedHandlerTasks = new List<Task>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void InitializeFeedHandlers()
        {
            _ = Task.Run(async () =>
            {
                var config = await Helpers.LoadConfigFile<FeedHandler, List<FeedHandlerConfig>>();

                //todo enable on config save too
                foreach (var f in config.Where(c => c.Enabled))
                {
                    feedHandlerTasks.Add(Task.Run(async () =>
                    {
                        var lastSuccessfulCheck = DateTime.Now;
                        while (true)
                        {
                            if (cancellationTokenSource.IsCancellationRequested)
                            {
                                Log($"Cancelling thread...");
                                return;
                            }
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
                                        TileQueue.Enqueue(new FeedHandler(i));
                                    }
                                    lastSuccessfulCheck = items.Max(i => i.PublishDate);
                                }
                                Log($"Successful check for {f.Url} - Items retrieved: {feed.Count()} - Items displayed: {items.Count}");
                                menuTileString.OnNext("✔️Feeds!");
                            }
                            catch (Exception e)
                            {
                                Log($"Exception for {f.Url} - {e.Message}");
                                menuTileString.OnNext("❌Feeds!");
                            }

                            await Task.Delay(TimeSpan.FromMinutes(f.CheckEveryMinutes));
                        }
                    }, cancellationTokenSource.Token));
                }
            });
        }
    }
}
