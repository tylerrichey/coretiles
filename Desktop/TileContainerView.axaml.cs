using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using CoreTiles.Tiles;
using System.Reactive.Linq;
using System.Linq;
using System;
using Avalonia.Controls.Templates;
using CoreTiles.Desktop.Views;

namespace CoreTiles.Desktop
{
    public class TileContainerView : UserControl
    {
        public TileContainerView() => InitializeComponent();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.PointerMoved += (s, e) => Height = double.NaN;
            this.PointerLeave += (s, e) => Height = MinHeight;
        }
    }
}