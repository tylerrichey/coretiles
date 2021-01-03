using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Streaming;
using System.Linq;
using System;
using Tweetinvi.Models;
using System.IO;
using Tweetinvi.Models.DTO;
using ReactiveUI;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Media;
using System.Reactive.Subjects;
using Avalonia.Media.Imaging;
using Tweetinvi.Parameters;
using Tiles.Twitter;
using Avalonia.Controls.ApplicationLifetimes;
using System.Collections.Generic;

namespace CoreTiles.Tiles
{
    public class Twitter : Tile
    {
        public override IDataTemplate DataTemplate { get; } = new FuncDataTemplate<ITweet>((t, s) 
            => new TweetTile { DataContext = new TweetTileViewModel(t) }, true);

        public override MenuItem MiniTile
        {
            get
            {
                var baseMini = base.MiniTile;
                baseMini.Foreground = Brush.Parse("#1da1f2");
                //todo not sure about icons, seems like they don't work at the top level, need to test in green field
                //baseMini.Icon = new Bitmap(assets.Open(new Uri("avares://Tiles.Twitter/icon.ico")));
                baseMini.Command = ReactiveCommand.Create(async () =>
                {
                    var window = new TwitterConfigWindow
                    {
                        DataContext = new TwitterConfigViewModel(twitterConfig, GetLogViewerControl())
                    };
                    if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        await window.ShowDialog(desktop.MainWindow);
                        if (!isConnected)
                        {
                            await InitTweetinvi();
                        }
                    }
                });
                return baseMini;
            }
        }

        public override Type ConfigType => typeof(TwitterConfig);

        //private static IAssetLoader assets => AvaloniaLocator.Current.GetService<IAssetLoader>();
        private TwitterConfig twitterConfig = new TwitterConfig();
        private bool isConnected;

        public override async Task Initialize() => await InitTweetinvi();

        private async Task InitTweetinvi()
        {
            MarkConnected(false);
            twitterConfig = Helpers.GetConfig<Twitter, TwitterConfig>();
            if (!twitterConfig.IsPopulated())
            {
                Log("Twitter configuration missing!");
                return;
            }

            var twitterClient = new TwitterClient(twitterConfig.ConsumerKey, twitterConfig.ConsumerSecret, twitterConfig.UserAccessToken, twitterConfig.UserAccessSecret);
            twitterClient.Config.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;

            var stream = twitterClient.Streams.CreateFilteredStream();
            var user = await twitterClient.Users.GetAuthenticatedUserAsync();
            var friends = user.GetFriendIds();
            while (!friends.Completed)
            {
                var page = await friends.NextPageAsync();
                page.ForEach(id => stream.AddFollow(id));
            }
            stream.MatchOn = MatchOn.Follower;
            stream.MatchingTweetReceived += (s, e) =>
            {
                if (e.MatchOn == MatchOn.Follower && !e.Tweet.Text.StartsWith("@"))
                {
                    PushTileData(e.Tweet);
                }
            };
            stream.StreamStarted += (s, e) =>
            {
                Log("Stream started!");
                MarkConnected();
            };
            stream.StreamStopped += (s, e) =>
            {
                Log("Stream stopped: " + string.Join(" - ", e.Exception.Message, e.DisconnectMessage));
                MarkConnected(false);
                Thread.Sleep(1000);
                stream.StartMatchingAllConditionsAsync().Wait();
            };

            var currentTimeline = await user.GetHomeTimelineAsync();
            foreach (var t in currentTimeline.OrderByDescending(c => c.CreatedAt.LocalDateTime)
                .Take(20)
                .OrderBy(c => c.CreatedAt.LocalDateTime))
            {
                PushTileData(t);
            }

            await stream.StartMatchingAllConditionsAsync();
        }

        private void MarkConnected(bool connected = true)
        {
            UpdateMiniTileText((connected ? "✔️" : "❌") + "Twitter");
            isConnected = connected;
        }

        public override async Task InitializeDebug()
        {
            MarkConnected(false);
            const string dataFile = "sample.json";
            using var streamReader = new StreamReader(dataFile);
            var json = await streamReader.ReadToEndAsync();
            var client = new TwitterClient(string.Empty, string.Empty);
            var tweetDTOs = client.Json.Deserialize<IEnumerable<ITweetDTO>>(json);
            foreach (var tweet in client.Factories.CreateTweets(tweetDTOs).Take(10))
            {
                PushTileData(tweet);
            }

            //_ = Task.Run(() =>
            //{
            //    Thread.Sleep(5000);
            //    foreach (var tweet in Tweet.GenerateTweetsFromDTO(tweetDTOs.Take(40)))
            //    {
            //        MarkConnected();
            //        PushTileData(tweet);
            //        Thread.Sleep(1000);
            //    }
            //});
        }


        // streaming is better than this, but i'll leave it in case i find a way to embed the consumer key and secret safely
        //private async Task InitHomelineTweets()
        //{
        //    //// Create a new set of credentials for the application.
        //    //var appCredentials = new TwitterCredentials("CONSUMER_KEY", "CONSUMER_SECRET");

        //    //// Init the authentication process and store the related `AuthenticationContext`.
        //    //var authenticationContext = AuthFlow.InitAuthentication(appCredentials);

        //    //// Go to the URL so that Twitter authenticates the user and gives him a PIN code.
        //    //Process.Start(authenticationContext.AuthorizationURL);

        //    //// Ask the user to enter the pin code given by Twitter
        //    //var pinCode = Console.ReadLine();

        //    //// With this pin code it is now possible to get the credentials back from Twitter
        //    //var userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(pinCode, authenticationContext);

        //    //// Use the user credentials in your application
        //    //Auth.SetCredentials(userCredentials);

        //    RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
        //    var currentTimeline = await User.GetAuthenticatedUser().GetHomeTimelineAsync(20);
        //    long lastTweetProcessedId = 0;
        //    foreach (var t in currentTimeline.OrderBy(c => c.CreatedAt))
        //    {
        //        TileQueue.Enqueue(new Twitter(t));
        //        lastTweetProcessedId = t.Id;
        //    }

        //    _ = Task.Run(async () =>
        //    {
        //        while (true)
        //        {
        //            await Task.Delay(TimeSpan.FromMinutes(1));

        //            var timelineParams = new HomeTimelineParameters
        //            {
        //                SinceId = lastTweetProcessedId,
        //                MaximumNumberOfTweetsToRetrieve = 100
        //            };
        //            var tweets = await User.GetAuthenticatedUser().GetHomeTimelineAsync(timelineParams);
        //            foreach (var t in tweets)
        //            {
        //                TileQueue.Enqueue(new Twitter(t));
        //                lastTweetProcessedId = t.Id;
        //            }
        //        }
        //    });
        //}
    }
}
