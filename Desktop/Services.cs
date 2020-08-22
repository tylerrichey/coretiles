using Desktop.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop
{
    public class Services
    {
        public IEnumerable<Tile> GetTiles() => Enumerable.Range(0, 20)
            .Select(i => Tile.New());
    }
}
