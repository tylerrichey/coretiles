using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;

namespace ImageViewerWindow
{
    public class ImageViewer : Window
    {
        public ImageViewer() => this.InitializeComponent();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.LostFocus += (s, e) => this.Close();

            var nextCommand = ReactiveCommand.Create(async () => await ((ImageViewerViewModel)DataContext)?.Next());
            this.PointerPressed += (s, e) => nextCommand.Execute().Subscribe();
        }
    }
}
