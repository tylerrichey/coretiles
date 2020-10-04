﻿using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewerWindow
{
    public sealed class ImageViewerViewModel : ReactiveObject, IDisposable
    {
        private int index = 1;
        private List<string> _urls;
        private Bitmap[] bitmaps;

        private Bitmap currentImage;
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

        public ImageViewerViewModel(IEnumerable<string> urls)
        {
            _urls = urls.ToList();
            bitmaps = new Bitmap[_urls.Count];

            ReactiveCommand.Create(async () => await SetImageByIndex(index))
                .Execute()
                .Subscribe();
        }

        public async Task Next()
        {
            if (_urls.Count > 1)
            {
                index = index + 1 > _urls.Count ? 1 : index + 1;
                await SetImageByIndex(index);
            }
        }
        
        private async Task SetImageByIndex(int index)
        {
            var arrayIndex = index - 1;
            if (bitmaps[arrayIndex] == null)
            {
                var bytes = await Helpers.HttpClient.GetByteArrayAsync(_urls[arrayIndex]);
                using var memoryStream = new MemoryStream(bytes);
                memoryStream.Seek(0, SeekOrigin.Begin);
                bitmaps[arrayIndex] = new Bitmap(memoryStream);
            }
            CurrentImage = bitmaps[arrayIndex];
            WindowTitle = index + " / " + _urls.Count;
        }
        
        public void Dispose()
        {
            CurrentImage?.Dispose();
            foreach (var b in bitmaps)
            {
                b?.Dispose();
            }
        }
    }
}
