using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Tiles.FeedHandler
{
    public class FeedHandlerConfig
    {
        public string Url { get; set; }
        public int CheckEveryMinutes { get; set; }
        public bool Enabled { get; set; }
        public string Regex { get; set; }
        public string Name { get; set; }
    }
}
