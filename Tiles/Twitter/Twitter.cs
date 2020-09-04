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
                return baseMini;
            }
        }

        public ITweet CurrentTweet { get; }

        private static IAssetLoader assets => AvaloniaLocator.Current.GetService<IAssetLoader>();

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

        private void MarkConnected(bool connected = true) => UpdateMiniTileText((connected ? "✔️" : "❌") + "Twitter");

        private async Task<bool> InitDebugEnvironment()
        {
#if DEBUG
            return true;

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
