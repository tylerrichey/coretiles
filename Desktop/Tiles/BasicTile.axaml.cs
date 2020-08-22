using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Desktop.Models;

namespace Desktop.Tiles
{
    public class BasicTile : UserControl
    {
        public Tile Tile { get; }

        public BasicTile() => InitializeComponent();

        public BasicTile(Tile tile) => Tile = tile;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}