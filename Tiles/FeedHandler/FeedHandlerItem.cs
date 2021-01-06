using System;
using System.Collections.Generic;
using System.Text;

namespace Tiles.FeedHandler
{
    public class FeedHandlerItem
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public string FeedName { get; set; }
    }
}
