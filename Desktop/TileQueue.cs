using Desktop.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Desktop
{
    public static class TileQueue
    {
        internal static ConcurrentQueue<Tile> Items = new ConcurrentQueue<Tile>();

        public static void Push(Tile t) => Items.Enqueue(t);
    }
}
