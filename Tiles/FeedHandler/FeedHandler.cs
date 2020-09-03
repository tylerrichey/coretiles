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
                    DataContext = new FeedHandlerConfigWindowViewModel(logEntries)
                };
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    && await window.ShowDialog<bool>(desktop.MainWindow))
                {
                    logEntries.Add($"{DateTime.Now} - Restarting feed handlers...");
                    InitializeFeedHandlers();
                }
            })
        };

        private Subject<string> menuTileString = new Subject<string>();
        //todo make observable and/or more robust?
        private List<string> logEntries = new List<string>();
        private Task feedHandlerTask;

        public FeedItem CurrentItem { get; }
        public FeedHandler(FeedItem feedItem) => CurrentItem = feedItem;
        public FeedHandler() { }

        public override Task Initialize()
        {
            FeedParser.SetHttpClient(Helpers.HttpClient);

#if DEBUG
            return Task.CompletedTask;
#endif

            InitializeFeedHandlers();

            return Task.CompletedTask;
        }

        private void InitializeFeedHandlers()
        {
            feedHandlerTask = Task.Run(async () =>
            {
                const string configName = "FeedHandlerConfig";
                var config = await Helpers.LoadConfigFile<List<FeedHandlerConfig>>(configName);

                //todo enable on config save too
                foreach (var f in config.Where(c => c.Enabled))
                {
                    //todo update status to menuitem
                    _ = Task.Run(async () =>
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
                                        TileQueue.Enqueue(new FeedHandler(i));
                                    }
                                    lastSuccessfulCheck = items.Max(i => i.PublishDate);
                                }
                                logEntries.Add($"{DateTime.Now} - Successful check for {f.Url} - Items retrieved: {feed.Count()} - Items displayed: {items.Count}");
                                menuTileString.OnNext("✔️Feeds!");
                            }
                            catch (Exception e)
                            {
                                logEntries.Add($"{DateTime.Now} - Exception for {f.Url} - {e.Message}");
                                menuTileString.OnNext("❌Feeds!");
                            }

                            await Task.Delay(TimeSpan.FromMinutes(f.CheckEveryMinutes));
                        }
                    });
                }
            });
        }
    }
}
