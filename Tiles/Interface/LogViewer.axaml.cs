using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CoreTiles.Tiles
{
    public class LogViewer : UserControl
    {
        public LogViewer()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
