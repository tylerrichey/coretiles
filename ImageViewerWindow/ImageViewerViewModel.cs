using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageViewerWindow
{
    public sealed class ImageViewerViewModel : ReactiveObject, IDisposable
    {
        private Bitmap currentImage;
        private int index = 1;
        private List<string> _urls;

        public Bitmap CurrentImage
        {
            get => currentImage;
            set => this.RaiseAndSetIfChanged(ref currentImage, value);
        }

        private string windowTitle;
        public string WindowTitle
        {
            get => windowTitle;
            set => this.RaiseAndSetIfChanged(ref windowTitle, value);
        }

        public ImageViewerViewModel(List<string> urls)
        {
            _urls = urls;

            SetImageByIndex(index);
        }

        public void Next()
        {
            if (_urls.Count > 1)
            {
                index = index + 1 > _urls.Count ? 1 : index + 1;
                SetImageByIndex(index);
            }
        }
        
        private void SetImageByIndex(int index)
        {
            var bytes = Helpers.HttpClient.GetByteArrayAsync(_urls[index - 1]).Result;
            using var memoryStream = new MemoryStream(bytes);
            memoryStream.Seek(0, SeekOrigin.Begin);
            CurrentImage = new Bitmap(memoryStream);
            WindowTitle = index + " / " + _urls.Count;
        }

        public void Dispose() => CurrentImage?.Dispose();
    }
}
