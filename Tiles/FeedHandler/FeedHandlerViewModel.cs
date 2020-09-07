using FeedParserCore;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;

namespace Tiles.FeedHandler
{
    public class FeedHandlerViewModel : ReactiveObject
    {
        public string Link { get; }
        public string Title { get; }
        public string Content { get; }
        public string PublishDate { get; }

        public ReactiveCommand<string, Unit> LaunchUrl { get; }

        public FeedHandlerViewModel(FeedItem feedItem)
        {
            //todo do something with content? it's often html...
            var content = feedItem.Content.Length > 400 ? feedItem.Content.Substring(0, 397) + "..." : feedItem.Content;
            Link = feedItem.Link;
            Title = feedItem.Title;
            Content = content;
            PublishDate = "⏰ " + feedItem.PublishDate.ToString();
            LaunchUrl = ReactiveCommand.Create<string>(url => Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            }));
        }
    }
}
