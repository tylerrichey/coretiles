using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tiles.FeedHandler
{
    public class FeedHandlerTile : UserControl
    {
        public FeedHandlerTile()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
