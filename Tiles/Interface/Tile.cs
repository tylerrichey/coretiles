using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{
    public abstract class Tile : ITile
    {
        public ConcurrentQueue<TileData> TileQueue { get; } = new ConcurrentQueue<TileData>();

        private LimitedList<(DateTime, string)> logItems = new LimitedList<(DateTime, string)>(50);
        protected void Log(string message, params string[] args)
        {
            Serilog.Log.ForContext(GetType()).Information(message, args);
            logItems.TryAdd((DateTime.Now, string.Format(message, args)));
        }

        public List<(DateTime, string)> GetLog() => logItems.ToList();
        public virtual LogViewer GetLogViewerControl() => new LogViewer
        {
            DataContext = new LogViewerViewModel(GetLog())
        };

        protected HttpClient httpClient => Helpers.HttpClient;

        public abstract Task Initialize();
        public abstract Task InitializeDebug();
        public abstract IDataTemplate DataTemplate { get; }
        
        private Subject<string> headerSubject = new Subject<string>();
        protected void UpdateMiniTileText(string text) => headerSubject.OnNext(text);
        public virtual MenuItem MiniTile => new MenuItem
        {
            Header = this.GetType().Name,
            [!MenuItem.HeaderProperty] = headerSubject.ToBinding(),
            Command = ReactiveCommand.Create(async () =>
            {
                var window = new Window
                {
                    Height = 400,
                    Width = 1200,
                    Content = this.GetLogViewerControl()
                };
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    await window.ShowDialog(desktop.MainWindow);
                }
            })
        };

        protected void PushTileData(object data) => TileQueue.Enqueue(new TileData { Data = data });

        public abstract Type ConfigType { get; }
    }

    public struct TileData
    {
        public object Data { get; internal set; }
    }

    public interface ITile
    {
        Task Initialize();

        Task InitializeDebug();
    }
}
