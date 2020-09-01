using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{
    public abstract class Tile : ITile
    {
        public Guid Id = Guid.NewGuid();
        public ConcurrentQueue<Tile> TileQueue { get; set; } = new ConcurrentQueue<Tile>();

        public abstract Task Initialize();
        public abstract Window GetConfigurationWindow();
        public abstract IDataTemplate DataTemplate { get; set; }
        public abstract MenuItem MiniTile { get; }
    }

    public interface ITile
    {
        Task Initialize();
        Window GetConfigurationWindow();
    }
}
