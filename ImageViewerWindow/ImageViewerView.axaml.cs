using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CoreTiles.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            this.Closed += (s, e) => ((ImageViewerViewModel)DataContext)?.Dispose();
        }

        public static ImageViewer GetImageViewerWithUrls(List<string> urls)
        {
            if (urls.Count == 0) { throw new ArgumentException("No URLs supplied."); }

            var vm = new ImageViewerViewModel(urls);
            return new ImageViewer(vm);
        }

        public static void Show(IEnumerable<string> urls) => GetImageViewerWithUrls(urls.ToList()).Show();
        public static void Show(string url) => GetImageViewerWithUrls(new List<string> { url });
    }
}
