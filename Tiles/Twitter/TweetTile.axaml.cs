using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System.Reactive.Linq;
using System;
namespace CoreTiles.Tiles
{
    public class TweetTile : UserControl
    {
        public TweetTile() => this.InitializeComponent();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.GetObservable(Control.DataContextProperty)
                .OfType<TweetTileViewModel>()
                .Subscribe(vm =>
                {
                    this.FindControl<WrapPanel>("NamePanel").PointerPressed += (s, e) => vm.LaunchUrl(vm.ProfileUrl);
                    this.FindControl<WrapPanel>("TweetPanel").PointerPressed += (s, e) => vm.LaunchUrl(vm.TweetUrl);
                    this.FindControl<WrapPanel>("StatsPanel").PointerPressed += (s, e) => vm.LaunchUrl(vm.StatsUrl);
                });
        }
    }
}
