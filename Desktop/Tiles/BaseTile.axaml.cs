using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Desktop.Models;

namespace Desktop.Tiles
{
    public class BaseTile : UserControl
    {
        public Tile Tile { get; }

        public BaseTile() => InitializeComponent();

        public BaseTile(Tile tile) => Tile = tile;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}