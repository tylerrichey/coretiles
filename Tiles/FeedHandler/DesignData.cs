using CoreTiles.Tiles;
using FeedParserCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Tiles.FeedHandler
{
    public static class DesignData
    {
        public static FeedHandlerViewModel FeedHandlerViewModel { get; } =
            new FeedHandlerViewModel(new FeedItem
            {
                Title = "Liverpool to face Blackpool in pre-season friendly",
                Link = "https://www.reddit.com/r/LiverpoolFC/comments/il6chr/liverpool_to_face_blackpool_in_preseason_friendly/",
                Content = @"Here's is some content....................................
................................................",
                PublishDate = DateTime.Now
            });

        public static FeedHandlerConfigWindowViewModel FeedHandlerConfigWindowViewModel
        {
            get
            {
                var logv = new LogViewerViewModel(new List<(DateTime, string)>
                {
                    (DateTime.Now, "Hello this is log"),
                    (DateTime.Now, "hi this is another log"),
                    (DateTime.Now, "bye this is log no more")
                });
                var vm = new FeedHandlerConfigWindowViewModel(new LogViewer { DataContext = logv });
                vm.Feeds = new ObservableCollection<FeedHandlerConfig>
                {
                    new FeedHandlerConfig { Url = "https://www.reddit.com/r/LiverpoolFC/new/.rss", CheckEveryMinutes = 5, Enabled = true, Regex = ".*" },
                    new FeedHandlerConfig { Url = "https://www.reddit.com/r/WatchURaffle/new/.rss", CheckEveryMinutes = 10, Enabled = false, Regex = ".*(?i)(iwc|omega|rolex).*" }
                 };
                return vm;
            }
        }
    }
}
