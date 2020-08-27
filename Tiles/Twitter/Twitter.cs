using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using CoreTiles.Tiles;
using System;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{
    public class Twitter : Tile
    {
        public override IDataTemplate DataTemplate { get; set; } = new FuncDataTemplate<Twitter>((t, s) =>
            new TextBlock { Text = "Twitter!" });

        public override void Initialize()
        {
            var mock = Task.Run(async () =>
            {
                while (true)
                {

                    TileQueue.Enqueue(new Twitter());
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }
    }
}
