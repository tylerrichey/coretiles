using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CoreTiles.Desktop.Views
{
    public class WeatherTileView : UserControl
    {
        public WeatherTileView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
