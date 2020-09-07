using Avalonia;
using Avalonia.Controls.Templates;
using CoreTiles.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreTiles.Desktop.InternalServices
{
    public class SystemTile : Tile
    {
        public override IDataTemplate DataTemplate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        Services services;
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
                .Select(l => string.Join(" - ", l.Item1, l.Item2, l.Item3)))
            {
                logs.AppendLine(s);
            }
            return new LogViewer
            {
                DataContext = new LogViewerViewModel(logs.ToString())
            };
        }

        public override Task InitializeDebug() => Initialize();
    }
}
