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
using ReactiveUI;
using ImageViewerWindow;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tweetinvi.Logic.DTO;
using System.IO;
using Tweetinvi.Models.DTO;
using Avalonia.Platform;

namespace CoreTiles.Tiles
{
    //todo should probably re-factor the template code to call a new control and shift the layout to xaml
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
                ClipToBounds = true
            };

        private static TextBlock DefaultHighlightedTextBlock(string text) =>
            new TextBlock
            {
                FontFamily = FontFamily.Parse("Cascadia Code PL"),
                FontSize = 14,
                Margin = Thickness.Parse("4"),
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Cursor = new Cursor(StandardCursorType.Hand),
                ClipToBounds = true,
                Foreground = Brushes.OrangeRed
            };

        private static Button DefaultButton(string text) =>
            new Button
            {
                FontFamily = FontFamily.Parse("Cascadia Code PL"),
                FontSize = 14,
                Margin = Thickness.Parse("4"),
                Content = text,
                Cursor = new Cursor(StandardCursorType.Hand),
                ClipToBounds = true
            };

        private static IEnumerable<Match> GetMatches(List<string> regexs, string input)
        {
            foreach (var r in regexs)
            {
                foreach (var m in Regex.Matches(input, r, RegexOptions.IgnoreCase).Cast<Match>())
                {
                    yield return m;
                }
            }
        }

        //todo multiple text blocks will never format right because of new lines
        //     need one textblock with inline color, which is not available
        //     perhaps try highlighting with emoji?
        private static IEnumerable<TextBlock> TweetTextToBlocks(string tweet)
        {
            var highlight = new List<string>
            {
                @"#\w*",
                @"\@\w*",
                @"htt(p://|ps://)\w*\.\w*(/\w*| )"
            };
            var matches = GetMatches(highlight, tweet)
                .ToList()
                .OrderBy(m => m.Index);

            var newTweet = string.Copy(tweet);
            foreach (var m in matches)
            {
                var replacement = tweet[m.Index] switch
                {
                    '#' => "#️⃣",
                    '@' => "✨",
                    'h' => "🔗h",
                    _ => ""
                };
                newTweet = tweet.Remove(m.Index, 1);
                newTweet = newTweet.Insert(m.Index, replacement);
            }

            yield return DefaultTextBlock(newTweet);

            //var currentStringIndex = 0;
            //foreach (var m in matches)
            //{
            //    var normalString = tweet.Substring(currentStringIndex, m.Index - currentStringIndex);
            //    if (!string.IsNullOrEmpty(normalString))
            //    {
            //        yield return DefaultTextBlock(normalString);
            //    }
            //    yield return DefaultHighlightedTextBlock(tweet.Substring(m.Index, m.Length));
            //    currentStringIndex = m.Index + m.Length;
            //}
            //if (currentStringIndex < tweet.Length)
            //{
            //    yield return DefaultTextBlock(tweet.Substring(currentStringIndex, tweet.Length - 1 - currentStringIndex));
            //}
        }

        public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<Twitter>((t, s) =>
        {
            var nameWrapPanel = new WrapPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal
            };
            var screenName = DefaultHighlightedTextBlock("@" + t.CurrentTweet.CreatedBy.ScreenName);
            var fullName = DefaultTextBlock((t.CurrentTweet.CreatedBy.Verified ? "✔️" : "") + t.CurrentTweet.CreatedBy.Name);
            nameWrapPanel.PointerPressed += (sender, e) => LaunchUrl("https://twitter.com/" + t.CurrentTweet.CreatedBy.ScreenName);
            nameWrapPanel.Children.Add(screenName);
            nameWrapPanel.Children.Add(fullName);
            
            //todo quoted tweets not showing on replies
            var tweetWrapPanel = new WrapPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            var tweetText = WebUtility.HtmlDecode(t.CurrentTweet.IsRetweet ? "RT @" + t.CurrentTweet.RetweetedTweet.CreatedBy.ScreenName + ": " + t.CurrentTweet.RetweetedTweet.FullText : t.CurrentTweet.FullText);
            var tweet = TweetTextToBlocks(tweetText);
            tweetWrapPanel.PointerPressed += (sender, e) => LaunchUrl(t.CurrentTweet.Urls.Count == 0 ? t.CurrentTweet.Url : t.CurrentTweet.Urls[0].URL);
            tweetWrapPanel.Children.AddRange(tweet);
            //tweetWrapPanel.Children.Add(DefaultTextBlock(tweetText));
            
            bool isRetweetThatQuotes = t.CurrentTweet.IsRetweet && t.CurrentTweet.QuotedTweet != null;
            var extraStats = string.Empty;
            if (t.CurrentTweet.QuotedTweet != null || isRetweetThatQuotes)
            {
                var est = isRetweetThatQuotes ? t.CurrentTweet.RetweetedTweet.QuotedTweet : t.CurrentTweet.QuotedTweet;
                extraStats = "🔁" + (t.CurrentTweet.RetweetCount + t.CurrentTweet.ReplyCount.GetValueOrDefault() +
                    t.CurrentTweet.QuoteCount.GetValueOrDefault()) + "❤️" + t.CurrentTweet.FavoriteCount;
            }
            var statsline = DefaultTextBlock("⏰" + t.CurrentTweet.CreatedAt.ToShortTimeString().Replace(" ", "") + extraStats);
            statsline.PointerPressed += (sender, e) => LaunchUrl(t.CurrentTweet.Url);
            statsline.Cursor = new Cursor(StandardCursorType.Hand);
            var statsWrapPanel = new WrapPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal
            };
            statsWrapPanel.Children.Add(statsline);
            var photosCount = t.CurrentTweet.Media.Count(m => m.MediaType == "photo");
            if (photosCount > 0)
            {
                var button = DefaultButton(photosCount > 1 ? "Photo(s)" : "Photo");
                button.Command = ReactiveCommand.Create<ITweet>(t =>
                    ImageViewer.Show(t.Media.Where(m => m.MediaType == "photo").Select(u => u.MediaURL).ToList()));
                button.CommandParameter = t.CurrentTweet;
                statsline.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                button.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                statsWrapPanel.Children.Add(button);
            }

            var stackPanel = new StackPanel
            {
                ClipToBounds = true
            };
            stackPanel.Children.Add(nameWrapPanel);
            stackPanel.Children.Add(tweetWrapPanel);
            stackPanel.Children.Add(statsWrapPanel);

            return new Border()
            {
                Background = Brush.Parse("#495057"),
                Padding = Thickness.Parse("4"),
                Margin = Thickness.Parse("4"),
                CornerRadius = CornerRadius.Parse("3"),
                BorderThickness = Thickness.Parse("2"),
                BorderBrush = Brush.Parse("#495050"),
                Child = stackPanel
            };
        });

        public ITweet CurrentTweet { get; }

        private Task streamTask;

        public Twitter() { }
        public Twitter(ITweet tweet) => CurrentTweet = tweet;

        public override async Task Initialize()
        {
#if DEBUG
            const string dataFile = @"C:\Users\Tyler\Desktop\Tweets\tweets637332765734054977.json";
            using var streamReader = new StreamReader(dataFile);
            var json = await streamReader.ReadToEndAsync();
            var tweetDTOs = json.ConvertJsonTo<ITweetDTO[]>();
            foreach (var tweet in Tweet.GenerateTweetsFromDTO(tweetDTOs.Take(20)))
            {
                TileQueue.Enqueue(new Twitter(tweet));
            }
            return;
#endif

            await InitTweetinvi();
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
