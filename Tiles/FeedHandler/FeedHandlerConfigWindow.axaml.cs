using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tiles.FeedHandler
{
    public class FeedHandlerConfigWindow : Window
    {
        public FeedHandlerConfigWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
