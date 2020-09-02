using FeedParserCore;
using ReactiveUI;
using System;

namespace Tiles.FeedHandler
{
    public class FeedHandlerViewModel : ReactiveObject
    {
        public string Link { get; }
        public string Title { get; }
        public string Content { get; }
        public string PublishDate { get; }

        public FeedHandlerViewModel(FeedItem feedItem)
        {
            var content = feedItem.Content.Length > 400 ? feedItem.Content.Substring(0, 397) + "..." : feedItem.Content;
            Link = feedItem.Link;
            Title = feedItem.Title;
            Content = content;
            PublishDate = "⏰ " + feedItem.PublishDate.ToString();
        }
    }
}
