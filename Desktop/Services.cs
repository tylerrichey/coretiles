using CoreTiles.Desktop.InternalServices;
using CoreTiles.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreTiles.Desktop
{
    public class Services
    {
        private List<Type> tilePluginsList = new List<Type>();
        public List<Tile> Tiles { get; }
        public Subject<bool> TilesInitialized { get; } = new Subject<bool>();

        public Services()
        {
            try
            {
                tilePluginsList.Add(typeof(SystemTile));
                tilePluginsList.Add(typeof(Weather));
                tilePluginsList.AddRange(GetTilePlugins().Distinct());
                Tiles = tilePluginsList.Select(p => Activator.CreateInstance(p) as Tile)
                    .ToList();
            }
            catch (AggregateException ae)
            {
                //todo log these
                var excs = ae.Flatten().InnerExceptions;
                throw new ApplicationException("Error initializing plugins", excs[0]);
            }
            
        }

        private List<Type> GetTilePlugins()
        {
            var match = typeof(Tile);
            var possiblePlugins = new List<Type>();

            var tilePath = Environment.GetEnvironmentVariable("TILEPATH") ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var f in Directory.GetFiles(tilePath, "*.dll", SearchOption.AllDirectories)
                     .Where(d => Path.GetFileName(d).StartsWith("Tiles.", StringComparison.CurrentCulture)))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(f);
                    possiblePlugins.AddRange(assembly.GetTypes().Where(t => match.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass));
                }
                catch
                {
                    //log something?
                    //log something!
                }
            }

            return possiblePlugins;
        }
    }
}
