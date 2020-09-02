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
            Header = "Feeds!",
            Foreground = Brush.Parse("#ff4500"),
            Command = ReactiveCommand.Create(async () =>
            {
                var window = new FeedHandlerConfigWindow
                {
                    DataContext = new FeedHandlerConfigWindowViewModel()
                };
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    await window.ShowDialog(desktop.MainWindow);
                }
            })
        };

        public override Window GetConfigurationWindow() => throw new NotImplementedException();

        public FeedItem CurrentItem { get; }
        public FeedHandler(FeedItem feedItem) => CurrentItem = feedItem;
        public FeedHandler() { }

        public override Task Initialize()
        {
            FeedParser.SetHttpClient(Helpers.HttpClient);

            _ = Task.Run(async () =>
            {
                //var lastSuccessfulCheck = DateTime.Now;
                var lastSuccessfulCheck = DateTime.MinValue;
                var regex = ".*(?i)(iwc|omega|rolex).*";
                while (true)
                {
                    //var feed = await FeedParser.ParseAsync("https://www.reddit.com/r/WatchURaffle/new/.rss", FeedType.Atom);

                    //foreach(var i in feed)
                    //{
                    //    TileQueue.Enqueue(new FeedHandler(i));
                    //}

                    //var items = feed.Where(i => i.PublishDate > lastSuccessfulCheck && Regex.IsMatch(i.Title, regex));
                    //if (items.Any())
                    //{
                    //    lastSuccessfulCheck = items.Max(i => i.PublishDate);
                    //    foreach (var i in items)
                    //    {
                    //        TileQueue.Enqueue(new FeedHandler(i));
                    //    }
                    //}

                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            });

            return Task.CompletedTask;
        }
    }
}
