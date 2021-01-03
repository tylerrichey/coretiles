using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using ImageViewerWindow;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;
using NeoSmart.Unicode;
using System.Text.RegularExpressions;
using Avalonia.Media;

namespace CoreTiles.Tiles
{
    public class TweetTileViewModel : ReactiveObject
    {
        public string ScreenName { get; set; }
        public string Name { get; set; }
        public string TweetText { get; set; }
        public string TweetTime { get; set; }
        public string StatsCount { get; set; }
        public string FavoriteCount { get; set; }
        public ReactiveCommand<Unit, Task> PhotoCommand { get; set; }
        public ReactiveCommand<Unit, Unit> VideoCommand { get; set; }
        public bool PhotoButtonEnabled { get; set; }
        public bool VideoButtonEnabled { get; set; }
        public string ProfileUrl { get; set; }
        public string TweetUrl { get; set; }
        public string StatsUrl { get; set; }
        public bool ShowAll { get; set; }

        private IControl embeddedControl;
        public IControl EmbeddedControl
        {
            get => embeddedControl;
            set => this.RaiseAndSetIfChanged(ref embeddedControl, value);
        }

        public TweetTileViewModel() { }

        public TweetTileViewModel(ITweet tweet)
        {
            ScreenName = "@" + tweet.CreatedBy.ScreenName;
            ProfileUrl = "https://twitter.com/" + tweet.CreatedBy.ScreenName;
            TweetUrl = tweet.Urls.Count == 0 ? tweet.Url : tweet.Urls[0].URL;
            StatsUrl = tweet.Url;
            Name = (tweet.CreatedBy.Verified ? "✔️" : "") + tweet.CreatedBy.Name;
            if (tweet.IsRetweet && tweet.RetweetedTweet.QuotedTweet != null)
            {
                TweetText = "RT @" + tweet.RetweetedTweet.CreatedBy.ScreenName + ": " + tweet.RetweetedTweet.FullText;
            }
            else if (tweet.IsRetweet && tweet.RetweetedTweet.QuotedTweet == null)
            {
                TweetText = string.Empty;
            }
            else
            {
                TweetText = tweet.FullText;
            }
            TweetText = WebUtility.HtmlDecode(TweetText);
            TweetTime = "⏰" + tweet.CreatedAt.LocalDateTime.ToShortTimeString().Replace(" ", "");
            var stats = tweet.RetweetCount + tweet.ReplyCount.GetValueOrDefault() + tweet.QuoteCount.GetValueOrDefault();
            StatsCount = stats > 0 && !(tweet.IsRetweet && tweet.RetweetedTweet.QuotedTweet == null) ? "🔁" + stats : string.Empty;
            FavoriteCount = tweet.FavoriteCount > 0 && !(tweet.IsRetweet && tweet.RetweetedTweet.QuotedTweet == null) ? "❤️" + tweet.FavoriteCount : string.Empty;
            ShowAll = !(tweet.IsRetweet && tweet.RetweetedTweet.QuotedTweet == null);

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
                        FileName = Helpers.SystemConfig.VideoPlayerLocation,
                        Arguments = videoUrl,
                        UseShellExecute = true
                    });
                });
            }

            var photos = tweet.Media.Where(m => m.MediaType == "photo");
            PhotoButtonEnabled = photos.Count() > 1;
            PhotoCommand = ReactiveCommand.Create(async () =>
            {
                using var viewModel = new ImageViewerViewModel(photos.Select(u => u.MediaURL));
                var imageViewer = new ImageViewer
                {
                    DataContext = viewModel
                };
                //this seems hacky, but the window is required to pass to ShowDialog to get something returned
                //to wait on the window to close so things are disposed of properly
                //if you pass in main window, then my lostfocus event on imageviewer doesn't work
                var window = new Window();
                await imageViewer.ShowDialog(window);
            });

            var isRetweetThatQuotes = tweet.IsRetweet && tweet.RetweetedTweet.QuotedTweet != null;
            if (isRetweetThatQuotes || tweet.QuotedTweet != null || tweet.RetweetedTweet != null)
            {
                ITweet useTweet = null;
                if (isRetweetThatQuotes)
                {
                    useTweet = tweet.RetweetedTweet.QuotedTweet;
                }
                else if (tweet.QuotedTweet != null)
                {
                    useTweet = tweet.QuotedTweet;
                }
                else if (tweet.RetweetedTweet != null)
                {
                    useTweet = tweet.RetweetedTweet;
                }

                if (useTweet != null)
                {
                    EmbeddedControl =
                        new TweetTile
                        {
                            DataContext = new TweetTileViewModel(useTweet)
                        };
                }
            }
            else
            {
                if (photos.Count() == 1)
                {
                    ReactiveCommand.Create(async () =>
                    {
                        var bytes = await Helpers.HttpClient.GetByteArrayAsync(photos.Select(u => u.MediaURL).First());
                        using var memoryStream = new MemoryStream(bytes);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        EmbeddedControl = new Viewbox
                        {
                            Stretch = Stretch.Uniform,
                            Child = new Image
                            {
                                Source = new Bitmap(memoryStream)
                            }
                        };
                    }).Execute().Subscribe();
                }
            }
        }

        public void LaunchUrl(string url) => Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });

        //todo these characters are used a bunch from EU twitter accounts, but if the character
        //     does not exist in the font you're using, it breaks stuff when you come across one
        //     emoji stop printing in color, formatting is off, probably a windows bug
        //     this is just a stop gap btw, and it works eh
        //private Range badRange = new Range(new Codepoint("U+1D400"), new Codepoint("U+1D7FF"));
        //private string FilterTweetText(string input)
        //{
        //    var filteredString = string.Empty;
        //    foreach (var c in input.Codepoints())
        //    {
        //        if (c.IsIn(badRange))
        //        {
        //            try
        //            {
        //                var newCodepoint = new Codepoint(c.Value - 120211);
        //                filteredString += newCodepoint.AsString();
        //            }
        //            catch
        //            {
        //                //for now we just drop it
        //            }
        //        }
        //        else
        //        {
        //            filteredString += c.AsString();
        //        }
        //    }
        //    return filteredString;
        //}

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
        //     need to ask before, but maybe a combo of wrappanels and stackpanels could highlight and
        //     handle newline issue that comes with using a bunch of textblocks to get the formatting
        //private static List<TextBlock> TweetTextToBlocks(string tweet)
        //{
        //    try
        //    {
        //        var blocks = new List<TextBlock>();
        //        var highlight = new List<string>
        //        {
        //            @"#\w*",
        //            @"\@\w*",
        //            @"htt(p://|ps://)\w*\.\w*(/\w*| )"
        //        };
        //        var matches = GetMatches(highlight, tweet)
        //            .ToList()
        //            .OrderBy(m => m.Index);
        //        if (!matches.Any())
        //        {
        //            blocks.Add(new TextBlock { Text = tweet });
        //            return blocks;
        //        }

        //        var lastIndex = 0;
        //        foreach (var m in matches)
        //        {
        //            blocks.Add(new TextBlock { Text = tweet.Substring(lastIndex, m.Index - lastIndex - 1) });
        //            blocks.Add(new TextBlock { Text = tweet.Substring(m.Index, m.Length), Foreground = Brushes.OrangeRed });
        //            lastIndex = m.Index + m.Length;
        //        }

        //        var lastLength = tweet.Length - lastIndex - 1;
        //        if (lastLength > 0)
        //        {
        //            blocks.Add(new TextBlock { Text = tweet.Substring(lastIndex, lastLength) });
        //        }
        //        return blocks;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}
    }
}
