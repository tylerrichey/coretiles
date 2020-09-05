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

namespace CoreTiles.Tiles
{
    public class Twitter : Tile
    {
        public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<Twitter>((t, s) 
            => new TweetTile { DataContext = new TweetTileViewModel(t.CurrentTweet) });

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
                            await LoadConfig();
                            await InitTweetinvi();
                        }
                    }
                });
                return baseMini;
            }
        }

        public ITweet CurrentTweet { get; }

        private static IAssetLoader assets => AvaloniaLocator.Current.GetService<IAssetLoader>();
        private TwitterConfig twitterConfig;
        private bool isConnected;

        public Twitter() { }
        public Twitter(ITweet tweet) => CurrentTweet = tweet;

        public override async Task Initialize()
        {
            MarkConnected(false);
            await LoadConfig();
            
            if (!await InitDebugEnvironment())
            {
                await InitTweetinvi();
            }
        }

        private async Task LoadConfig()
        {
            try
            {
                twitterConfig = await Helpers.LoadConfigFile<Twitter, TwitterConfig>();
            }
            catch
            {
                twitterConfig = new TwitterConfig();
            }
        }

        private async Task InitTweetinvi()
        {
            //todo this obviously isn't a long term solution
            var twitterCreds = Environment.GetEnvironmentVariable("TWITTERCREDS").Split(',');
            if (twitterCreds.Length != 4)
            {
                throw new ApplicationException("Missing Twitter creds!");
            }
            Auth.SetUserCredentials(twitterCreds[0], twitterCreds[1], twitterConfig.UserAccessToken, twitterConfig.UserAccessSecret);
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;

            var stream = Tweetinvi.Stream.CreateFilteredStream();
            var friends = await User.GetAuthenticatedUser().GetFriendIdsAsync();
            friends.ForEach(f => stream.AddFollow(f));
            stream.MatchOn = MatchOn.Follower;
            stream.MatchingTweetReceived += (s, e) =>
            {
                if (e.MatchOn == MatchOn.Follower && !e.Tweet.Text.StartsWith("@"))
                {
                    TileQueue.Enqueue(new Twitter(e.Tweet));
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
                _ = stream.StartStreamMatchingAllConditionsAsync();
            };

            var currentTimeline = await User.GetAuthenticatedUser().GetHomeTimelineAsync(20);
            foreach (var t in currentTimeline.OrderBy(c => c.CreatedAt))
            {
                TileQueue.Enqueue(new Twitter(t));
            }

            _ = stream.StartStreamMatchingAllConditionsAsync();
        }

        private async Task InitHomelineTweets()
        {
            //// Create a new set of credentials for the application.
            //var appCredentials = new TwitterCredentials("CONSUMER_KEY", "CONSUMER_SECRET");

            //// Init the authentication process and store the related `AuthenticationContext`.
            //var authenticationContext = AuthFlow.InitAuthentication(appCredentials);

            //// Go to the URL so that Twitter authenticates the user and gives him a PIN code.
            //Process.Start(authenticationContext.AuthorizationURL);

            //// Ask the user to enter the pin code given by Twitter
            //var pinCode = Console.ReadLine();

            //// With this pin code it is now possible to get the credentials back from Twitter
            //var userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(pinCode, authenticationContext);

            //// Use the user credentials in your application
            //Auth.SetCredentials(userCredentials);

            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;
            var currentTimeline = await User.GetAuthenticatedUser().GetHomeTimelineAsync(20);
            long lastTweetProcessedId = 0;
            foreach (var t in currentTimeline.OrderBy(c => c.CreatedAt))
            {
                TileQueue.Enqueue(new Twitter(t));
                lastTweetProcessedId = t.Id;
            }

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1));

                    var timelineParams = new HomeTimelineParameters
                    {
                        SinceId = lastTweetProcessedId,
                        MaximumNumberOfTweetsToRetrieve = 100
                    };
                    var tweets = await User.GetAuthenticatedUser().GetHomeTimelineAsync(timelineParams);
                    foreach (var t in tweets)
                    {
                        TileQueue.Enqueue(new Twitter(t));
                        lastTweetProcessedId = t.Id;
                    }
                }
            });
        }

        private void MarkConnected(bool connected = true)
        {
            UpdateMiniTileText((connected ? "✔️" : "❌") + "Twitter");
            if (!connected)
            {
                isConnected = false;
            }
        }

        private async Task<bool> InitDebugEnvironment()
        {
#if DEBUG
            //return true;

            const string dataFile = "sample.json";
            using var streamReader = new StreamReader(dataFile);
            var json = await streamReader.ReadToEndAsync();
            var tweetDTOs = json.ConvertJsonTo<ITweetDTO[]>();
            foreach (var tweet in Tweet.GenerateTweetsFromDTO(tweetDTOs.Take(30)))
            {
                TileQueue.Enqueue(new Twitter(tweet));
            }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //Task.Run(() =>
            //{
            //    Thread.Sleep(5000);
            //    foreach (var tweet in Tweet.GenerateTweetsFromDTO(tweetDTOs.Take(200)))
            //    {
            //        MarkConnected();
            //        TileQueue.Enqueue(new Twitter(tweet));
            //        Thread.Sleep(1000);
            //    }
            //});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
#endif
            return false;
        }
    }
}
