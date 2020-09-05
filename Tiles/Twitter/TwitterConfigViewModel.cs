using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{

    public class TwitterConfigViewModel : ReactiveObject
    {
        public Subject<bool> CloseWindow { get; } = new Subject<bool>();

        public LogViewer LogViewer { get; }

        public ReactiveCommand<Unit, Task> SaveItems { get; }

        public TwitterConfig TwitterConfig { get; }

        public TwitterConfigViewModel(TwitterConfig twitterConfig, LogViewer logViewer)
        {
            TwitterConfig = twitterConfig;
            LogViewer = logViewer;
            SaveItems = ReactiveCommand.Create(async () =>
            {
                await Helpers.SaveConfigFile<Twitter>(TwitterConfig);
                CloseWindow.OnNext(true);
            });
        }
    }
}
