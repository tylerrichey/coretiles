using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using CoreTiles.Desktop.ViewModels;
using CoreTiles.Desktop.Views;
using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.InternalServices
{
    public class SystemTile : Tile
    {
        public override IDataTemplate DataTemplate { get => throw new NotImplementedException(); }

        public override Task Initialize()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    UpdateMiniTileText(DateTime.Now.ToShortTimeString().Replace(" ", ""));
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

            return Task.CompletedTask;
        }

        public override Type ConfigType => typeof(SystemTileConfig);

        public override MenuItem MiniTile
        {
            get
            {
                var baseMini = base.MiniTile;
                baseMini.Command = ReactiveCommand.Create(async () =>
                {
                    var window = new SystemTileConfigWindow
                    {
                        DataContext = new SystemTileViewModel(this.GetLogViewerControl())
                    };
                    if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        await window.ShowDialog(desktop.MainWindow);

                        //todo probably refine and/or abstract this idea
                        foreach (var tile in services.Tiles)
                        {
                            if (tile is Weather weather)
                            {
                                await weather.ReloadConfig();
                                break;
                            }
                        }
                    }
                });
                return baseMini;
            }
        }

        private static Services services;
        internal void SetServices(ref Services servicesRef) => services = servicesRef;

        //todo improve log viewer control in general
        public override LogViewer GetLogViewerControl()
        {
            var logData = new List<(string, DateTime, string)>();
            foreach (var tile in services.Tiles)
            {
                tile.GetLog()
                    .ForEach(l => logData.Add((tile.GetType().Name, l.Item1, l.Item2)));
            }
            var logs = new StringBuilder();
            foreach (var s in logData.OrderByDescending(l => l.Item2)
                .Select(l => string.Join(" - ", l.Item2, l.Item1, l.Item3)))
            {
                logs.AppendLine(s);
            }
            return new LogViewer
            {
                Height = 400,
                Width = 1100,
                DataContext = new LogViewerViewModel(logs.ToString())
            };
        }

        public override Task InitializeDebug() => Initialize();
    }
}
