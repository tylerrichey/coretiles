using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Streaming;
using System.Linq;
using Avalonia.Media;
using System;
using Avalonia;
using System.Diagnostics;
using Tweetinvi.Models;
using System.Net;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using System.Net.Http;
using System.IO;
using ReactiveUI;

namespace CoreTiles.Tiles
{
    public class Twitter : Tile
    {
        private static TextBlock DefaultTextBlock(string text) =>
            new TextBlock
            {
                FontFamily = FontFamily.Parse("Cascadia Code PL"),
                FontSize = 14,
                Margin = Thickness.Parse("4"),
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Cursor = new Cursor(StandardCursorType.Hand)
            };

        private static Button DefaultButton(string text) =>
            new Button
            {
                FontFamily = FontFamily.Parse("Cascadia Code PL"),
                FontSize = 14,
                Margin = Thickness.Parse("4"),
                Content = text,
                Cursor = new Cursor(StandardCursorType.Hand)
            };

        public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<Twitter>((t, s) =>
        {
            var names = DefaultTextBlock("@" + t.Tweet.CreatedBy.ScreenName + " - " + (t.Tweet.CreatedBy.Verified ? "✔️" : "") + t.Tweet.CreatedBy.Name);
            names.PointerPressed += (sender, e) => LaunchUrl("https://twitter.com/" + t.Tweet.CreatedBy.ScreenName);

            //todo quoted tweets not showing on replies
            var tweet = DefaultTextBlock(WebUtility.HtmlDecode(t.Tweet.IsRetweet ? "RT @" + t.Tweet.RetweetedTweet.CreatedBy.ScreenName + ": " + t.Tweet.RetweetedTweet.FullText : t.Tweet.FullText));
            tweet.PointerPressed += (sender, e) => LaunchUrl(t.Tweet.Urls.Count == 0 ? t.Tweet.Url : t.Tweet.Urls[0].URL);

            bool isRetweetThatQuotes = t.Tweet.IsRetweet && t.Tweet.QuotedTweet != null;
            var extraStats = string.Empty;
            if (t.Tweet.QuotedTweet != null || isRetweetThatQuotes)
            {
                var est = isRetweetThatQuotes ? t.Tweet.RetweetedTweet.QuotedTweet : t.Tweet.QuotedTweet;
                extraStats = "🔁" + (t.Tweet.RetweetCount + t.Tweet.ReplyCount.GetValueOrDefault() +
                    t.Tweet.QuoteCount.GetValueOrDefault()) + "❤️" + t.Tweet.FavoriteCount;
            }
            var statsline = DefaultTextBlock("⏰" + t.Tweet.CreatedAt.ToShortTimeString().Replace(" ", "") + extraStats);
            statsline.PointerPressed += (sender, e) => LaunchUrl(t.Tweet.Url);
            var wrapPanel = new WrapPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal
            };
            wrapPanel.Children.Add(statsline);
            if (t.Tweet.Media.Count(m => m.MediaType == "photo") > 0)
            {
                var button = DefaultButton("Photo(s)");
                button.Command = ReactiveCommand.Create<ITweet>(t =>
                {
                    var bytes = Helpers.HttpClient.GetByteArrayAsync(t.Media.First().MediaURL).Result;
                    using var memoryStream = new MemoryStream(bytes);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var bitmap = new Bitmap(memoryStream);
                    //todo this window should display all the media
                    var window = new Window
                    {
                        Content = new Viewbox
                        {
                            Stretch = Stretch.None,
                            Child = new Image()
                            {
                                Source = bitmap
                            }
                        },
                        Width = bitmap.Size.Width,
                        Height = bitmap.Size.Height,
                        Name = t.Media.First().ExpandedURL
                    };
                    window.LostFocus += (sender, e) => window.Close();
                    window.Closing += (sender, e) => bitmap.Dispose();
                    window.Show();
                });
                button.CommandParameter = t.Tweet;
                statsline.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                button.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                wrapPanel.Children.Add(button);
            }

            var stackPanel = new StackPanel
            {
                Background = Brush.Parse("#495057"),
                Margin = Thickness.Parse("4")
            };
            stackPanel.Children.Add(names);
            stackPanel.Children.Add(tweet);
            stackPanel.Children.Add(wrapPanel);
            return stackPanel;
        });

        public ITweet Tweet { get; }

        private Task streamTask;

        public Twitter() { }
        public Twitter(ITweet tweet) => Tweet = tweet;

        public override async Task Initialize()
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

        private static void LaunchUrl(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized
            };
            Process.Start(psi);
        }

        public override Window GetConfigurationWindow()
        {
            throw new NotImplementedException();
        }
    }
}
