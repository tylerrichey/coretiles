using Desktop.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop
{
    public class Services
    {
        public Services()
        {
            //testing
            var mock = Task.Run(async () =>
            {
                var rando = new Random();
                while (true)
                {
                    Tile tile;
                    if (rando.Next(0, 100) > 50)
                    {
                        tile = new TweetTile { TweetText = "Testing" };
                    }
                    else
                    {
                        tile = new KeyValueTile { Key = "Test", Value = "Ing" };
                    }
                    TileQueue.Push(tile);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }
    }
}
