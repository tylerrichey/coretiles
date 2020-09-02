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

namespace CoreTiles.Tiles
{
    public class Twitter : Tile
    {
        public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<Twitter>((t, s) 
            => new TweetTile { DataContext = new TweetTileViewModel(t.CurrentTweet) });

        public override MenuItem MiniTile
            => new MenuItem
            {
                //todo icon doesn't work for some reason
                Icon = "avares://Tiles.Twitter/icon.ico",
                [!MenuItem.HeaderProperty] = currentlyConnected.ToBinding(),
                Foreground = Brush.Parse("#1da1f2")
            };

        public ITweet CurrentTweet { get; }

        private Task streamTask;
        private Subject<string> currentlyConnected = new Subject<string>();

        public Twitter() { }
        public Twitter(ITweet tweet) => CurrentTweet = tweet;

        public override async Task Initialize()
        {
            MarkConnected(false);
            if (!await InitDebugEnvironment())
            {
                await InitTweetinvi();
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
            Auth.SetUserCredentials(twitterCreds[0], twitterCreds[1], twitterCreds[2], twitterCreds[3]);
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
            stream.StreamStarted += (s, e) => MarkConnected();
            stream.StreamStopped += (s, e) =>
            {
                MarkConnected(false);
                Thread.Sleep(1000);
                streamTask = stream.StartStreamMatchingAllConditionsAsync();
            };

            var currentTimeline = await User.GetAuthenticatedUser().GetHomeTimelineAsync();
            foreach (var t in currentTimeline.OrderBy(c => c.CreatedAt))
            {
                TileQueue.Enqueue(new Twitter(t));
            }

            streamTask = stream.StartStreamMatchingAllConditionsAsync();
        }

        private void MarkConnected(bool connected = true) => currentlyConnected.OnNext((connected ? "✔️" : "❌") + "Twitter");

        public override Window GetConfigurationWindow() => throw new NotImplementedException();

        private async Task<bool> InitDebugEnvironment()
        {
#if DEBUG
            const string dataFile = "sample.json";
            using var streamReader = new StreamReader(dataFile);
            var json = await streamReader.ReadToEndAsync();
            var tweetDTOs = json.ConvertJsonTo<ITweetDTO[]>();
            foreach (var tweet in Tweet.GenerateTweetsFromDTO(tweetDTOs.Take(100)))
            {
                TileQueue.Enqueue(new Twitter(tweet));
            }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                foreach (var tweet in Tweet.GenerateTweetsFromDTO(tweetDTOs.Take(200)))
                {
                    MarkConnected();
                    TileQueue.Enqueue(new Twitter(tweet));
                    Thread.Sleep(1000);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
#endif
            return false;
        }
    }
}
