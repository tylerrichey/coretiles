using CoreTiles.Tiles;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Tiles.FeedHandler
{

    public class FeedHandlerConfigWindowViewModel : ReactiveObject
    {
        private const string configName = "FeedHandlerConfig";

        public ObservableCollection<FeedHandlerConfig> Feeds { get; set; } = new ObservableCollection<FeedHandlerConfig>();

        public ReactiveCommand<Unit, Unit> AddItem { get; }

        public ReactiveCommand<string, Unit> DelItem { get; }

        public ReactiveCommand<Unit, Task> SaveItems { get; }

        public FeedHandlerConfigWindowViewModel()
        {
            AddItem = ReactiveCommand.Create(() => Feeds.Add(
                new FeedHandlerConfig
                {
                    CheckEveryMinutes = 15,
                    Regex = ".*"
                }));
            //todo this doesn't work right now
            DelItem = ReactiveCommand.Create<string>(url => Feeds.Remove(Feeds.First(f => f.Url == url)));
            SaveItems = ReactiveCommand.Create(async () => await Helpers.SaveConfigFile(Feeds, configName));

            ReactiveCommand.Create(async () =>
            {
                try
                {
                    var config = await Helpers.LoadConfigFile<List<FeedHandlerConfig>>(configName);
                    config.ForEach(c => Feeds.Add(c));
                }
                catch
                {
                    AddItem.Execute().Subscribe();
                }
            }).Execute().Subscribe();
        }
    }
}
