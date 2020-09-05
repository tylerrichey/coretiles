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
        private string userAccessToken;
        public string UserAccessToken
        {
            get => userAccessToken;
            set => this.RaiseAndSetIfChanged(ref userAccessToken, value);
        }

        private string userAccessSecret;
        public string UserAccessSecret
        {
            get => userAccessSecret;
            set => this.RaiseAndSetIfChanged(ref userAccessSecret, value);
        }

        public Subject<bool> CloseWindow { get; } = new Subject<bool>();

        public LogViewer LogViewer { get; }

        public ReactiveCommand<Unit, Task> SaveItems { get; }

        public TwitterConfigViewModel(TwitterConfig twitterConfig, LogViewer logViewer)
        {
            UserAccessToken = twitterConfig.UserAccessToken;
            UserAccessSecret = twitterConfig.UserAccessSecret;
            LogViewer = logViewer;
            SaveItems = ReactiveCommand.Create(async () =>
            {
                await Helpers.SaveConfigFile<Twitter>(new TwitterConfig
                {
                    UserAccessToken = UserAccessToken,
                    UserAccessSecret = UserAccessSecret
                });
                CloseWindow.OnNext(true);
            });
        }
    }
}
