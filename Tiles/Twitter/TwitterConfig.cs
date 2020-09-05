using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{
    public class TwitterConfig
    {
        public string UserAccessToken { get; set; }
        public string UserAccessSecret { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        public bool IsPopulated() => !string.IsNullOrWhiteSpace(UserAccessToken)
            && !string.IsNullOrWhiteSpace(UserAccessSecret)
            && !string.IsNullOrWhiteSpace(ConsumerKey)
            && !string.IsNullOrWhiteSpace(ConsumerSecret);
    }
}
