using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tweetinvi;
using Tweetinvi.Models.DTO;

namespace CoreTiles.Tiles
{
    public static class DesignData
    {
        private static TwitterClient twitterClient = new TwitterClient(string.Empty, string.Empty);

        public static TweetTileViewModel TweetTileViewModel { get; } =
            new TweetTileViewModel
            {
                ScreenName = "@LFC",
                Name = "✔️Liverpool FC",
                TweetText = @"We’re back for our final pre-season friendly! 🔴
 
🗓️ Saturday 5th September 
🆚 @BlackpoolFC
🕒 15:00 BST 

You can watch the game 𝗟𝗜𝗩𝗘 on 𝗟𝗙𝗖𝗧𝗩 and 𝗟𝗙𝗖𝗧𝗩 𝗚𝗢📺💻",
                // ^something with these emojis sometimes causes all emoji written
                // after that point to the same color as the font
                TweetTime = "⏰9:00AM",
                StatsCount = "🔁2540",
                FavoriteCount = "❤️5291",
                PhotoButtonEnabled = true,
                VideoButtonEnabled = true,
                ShowAll = true
            };

        public static TweetTileViewModel TweetTileViewModelFile
        {
            get
            {
                //todo replace with file with single tweet
                const string dataFile = @"C:\Users\Tyler\source\repos\CoreTiles\Tiles\Twitter\sample.json";
                var json = File.ReadAllText(dataFile);
                var tweetDTO = twitterClient.Json.Deserialize<ITweetDTO[]>(json)[12];
                return new TweetTileViewModel(twitterClient.Factories.CreateTweet(tweetDTO));
            }
        }

        public static TwitterConfigViewModel TwitterConfigViewModel { get; } =
            new TwitterConfigViewModel(new TwitterConfig
            {
                UserAccessSecret = "Secret",
                UserAccessToken = "Token",
                ConsumerKey = "Consumer Key",
                ConsumerSecret = "Consumer Secret"
            }, new LogViewer
            {
                DataContext = new LogViewerViewModel(
                    new List<(DateTime, string)>
                    {
                        (DateTime.Now, "Hello this is log"),
                        (DateTime.Now, "hi this is another log"),
                        (DateTime.Now, "bye this is log no more")
                    })
            });
    }
}
