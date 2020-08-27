using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using CoreTiles.Tiles;
using System.Reactive.Linq;
using System.Linq;
using System;
using Avalonia.Controls.Templates;

namespace CoreTiles.Desktop.Tiles
{
    public class BaseTile : UserControl
    {
        public BaseTile() => InitializeComponent();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.GetObservable(Control.DataContextProperty)
                .OfType<Tile>()
                .Subscribe(t =>
                {
                    this.DataTemplates.Add(t.DataTemplate);
                });
        }
    }
}