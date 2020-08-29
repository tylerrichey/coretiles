using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace ImageViewerWindow
{
    public class ImageViewer : Window
    {
        public ImageViewer() => this.InitializeComponent();
        public ImageViewer(ImageViewerViewModel imageViewerViewModel)
        {
            this.InitializeComponent();
            DataContext = imageViewerViewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.LostFocus += (s, e) => this.Close();
            this.PointerPressed += (s, e) => ((ImageViewerViewModel)DataContext)?.Next();
        }

        public static ImageViewer Get(List<string> urls)
        {
            if (urls.Count == 0) { throw new ApplicationException(); }

            var vm = new ImageViewerViewModel(urls);
            return new ImageViewer(vm);
        }

        public static void Show(List<string> urls) => Get(urls).Show();
        public static void Show(string url) => Get(new List<string> { url });
    }
}
