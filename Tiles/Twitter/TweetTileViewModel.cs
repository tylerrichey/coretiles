using Avalonia;
using Avalonia.Controls;
using ImageViewerWindow;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace CoreTiles.Tiles
{
    public class TweetTileViewModel : ReactiveObject
    {
        public string ScreenName { get; }
        public string Name { get; }
        public string TweetText { get; }
        public string TweetTime { get; }
        public string StatsCount { get; }
        public string FavoriteCount { get; }
        public string PhotoButtonLabel { get; }
        public ReactiveCommand<Unit, Unit> PhotoCommand { get; }
        public ReactiveCommand<Unit, Unit> VideoCommand { get; }
        public bool PhotoButtonEnabled { get; }
        public bool VideoButtonEnabled { get; }
        public string ProfileUrl { get; }
        public string TweetUrl { get; }
        public string StatsUrl { get; }
        public IControl QuoteTweet { get; }

        public TweetTileViewModel(ITweet tweet)
        {
            ScreenName = "@" + tweet.CreatedBy.ScreenName;
            ProfileUrl = "https://twitter.com/" + tweet.CreatedBy.ScreenName;
            TweetUrl = tweet.Urls.Count == 0 ? tweet.Url : tweet.Urls[0].URL;
            StatsUrl = tweet.Url;
            Name = (tweet.CreatedBy.Verified ? "✔️" : "") + tweet.CreatedBy.Name;
            TweetText = WebUtility.HtmlDecode(tweet.IsRetweet ? "RT @" + tweet.RetweetedTweet.CreatedBy.ScreenName + ": " + tweet.RetweetedTweet.FullText : tweet.FullText);
            TweetTime = "⏰" + tweet.CreatedAt.ToShortTimeString().Replace(" ", "");
            var stats = tweet.RetweetCount + tweet.ReplyCount.GetValueOrDefault() + tweet.QuoteCount.GetValueOrDefault();
            StatsCount = stats > 0 ? "🔁" + stats : string.Empty;
            FavoriteCount = tweet.FavoriteCount > 0 ? "❤️" + tweet.FavoriteCount : string.Empty;
            var photos = tweet.Media.Where(m => m.MediaType == "photo");
            PhotoButtonLabel = photos.Count() > 1 ? "Photo(s)" : "Photo";
            PhotoButtonEnabled = photos.Any();
            PhotoCommand = ReactiveCommand.Create(() =>
            {
                var viewModel = new ImageViewerViewModel(photos.Select(u => u.MediaURL));
                var imageViewer = new ImageViewer
                {
                    DataContext = viewModel
                };
                imageViewer.Show();
            });
            VideoButtonEnabled = tweet.Media.Any(v => v.MediaType == "video");
            if (VideoButtonEnabled)
            {
                var videoUrl = tweet.Media
                    .Select(v => v.VideoDetails)
                    .SelectMany(v => v.Variants)
                    .OrderByDescending(v => v.Bitrate)
                    .Select(v => v.URL)
                    .Take(1)
                    .First();
                VideoCommand = ReactiveCommand.Create(() =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        //todo make config value
                        FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe",
                        Arguments = videoUrl,
                        UseShellExecute = true
                    });
                });
            }
            var isRetweetThatQuotes = tweet.IsRetweet && tweet.RetweetedTweet.QuotedTweet != null;
            if (isRetweetThatQuotes || tweet.QuotedTweet != null)
            {
                QuoteTweet =
                    new TweetTile
                    {
                        DataContext = new TweetTileViewModel(isRetweetThatQuotes ? tweet.RetweetedTweet.QuotedTweet : tweet.QuotedTweet)
                    };
            }
        }

        public void LaunchUrl(string url) => Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });

        //private static IEnumerable<Match> GetMatches(List<string> regexs, string input)
        //{
        //    foreach (var r in regexs)
        //    {
        //        foreach (var m in Regex.Matches(input, r, RegexOptions.IgnoreCase).Cast<Match>())
        //        {
        //            yield return m;
        //        }
        //    }
        //}

        //todo multiple text blocks will never format right because of new lines
        //     need one textblock with inline color, which is not available
        //     perhaps try highlighting with emoji?
        //     nope, is not effective, has to be done in avalonia i think
        //private static IEnumerable<TextBlock> TweetTextToBlocks(string tweet)
        //{
        //    var highlight = new List<string>
        //    {
        //        @"#\w*",
        //        @"\@\w*",
        //        //@"htt(p://|ps://)\w*\.\w*(/\w*| )"
        //    };
        //    var matches = GetMatches(highlight, tweet)
        //        .ToList()
        //        .OrderBy(m => m.Index);

        //    var newTweet = string.Copy(tweet);
        //    foreach (var m in matches)
        //    {
        //        var replacement = tweet[m.Index] switch
        //        {
        //            '#' => "👉🏻",
        //            '@' => "✨",
        //            //'h' => "🔗h",
        //            _ => ""
        //        };
        //        newTweet = newTweet.Remove(m.Index, 1);
        //        newTweet = newTweet.Insert(m.Index, replacement);
        //    }

        //    yield return DefaultTextBlock(newTweet);
        //}
    }
}
