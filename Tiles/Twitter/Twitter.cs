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

namespace CoreTiles.Tiles
{
    public class Twitter : Tile
    {
        public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<Twitter>((t, s) 
            => new TweetTile { DataContext = new TweetTileViewModel(t.CurrentTweet) });

        //todo stick with text for now, binding to icon not working, maybe icon not getting added as proper resource
        public override MenuItem MiniTile => new MenuItem { Header = "Twitter" };

        public ITweet CurrentTweet { get; }

        private Task streamTask;

        public Twitter() { }
        public Twitter(ITweet tweet) => CurrentTweet = tweet;

        public override async Task Initialize()
        {
            if (!await InitDebugEnvironment())
            {
                await InitTweetinvi();
            }

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
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
            stream.StreamStarted += (s, e) => { };
            stream.StreamStopped += (s, e) =>
            {
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

        public override Window GetConfigurationWindow()
        {
            throw new NotImplementedException();
        }

        private async Task<bool> InitDebugEnvironment()
        {
#if DEBUG
            const string dataFile = @"C:\Users\Tyler\Desktop\Tweets\tweets637332765734054977.json";
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
            //    foreach (var tweet in Tweet.GenerateTweetsFromDTO(tweetDTOs.Take(30)))
            //    {
            //        TileQueue.Enqueue(new Twitter(tweet));
            //        Thread.Sleep(10000);
            //    }
            //});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
#endif
            return false;
        }
    }
}
