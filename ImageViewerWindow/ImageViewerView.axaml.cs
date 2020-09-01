using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ImageViewerWindow
{
    public class ImageViewer : Window
    {
        public ImageViewer() => this.InitializeComponent();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.LostFocus += (s, e) => this.Close();
            this.PointerPressed += (s, e) => ((ImageViewerViewModel)DataContext)?.Next();
        }
    }
}
