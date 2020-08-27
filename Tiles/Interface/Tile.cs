using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Concurrent;

namespace CoreTiles.Tiles
{
    public abstract class Tile : ITile
    {
        public Guid Id = Guid.NewGuid();
        public ConcurrentQueue<Tile> TileQueue { get; set; } = new ConcurrentQueue<Tile>();


        public abstract void Initialize();
        public abstract IDataTemplate DataTemplate { get; set; }
    }

    public interface ITile
    {
        void Initialize();
    }
}
