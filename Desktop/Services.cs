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
                while (true)
                {
                    TileQueue.Push(Tile.New());
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }
    }
}
