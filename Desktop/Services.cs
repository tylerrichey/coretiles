using CoreTiles.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreTiles.Desktop
{
    public class Services
    {
        private List<Type> tilePluginsList = new List<Type>();
        public List<Tile> Tiles { get; }

        public Services()
        {
            tilePluginsList = GetTilePlugins();
            Tiles = tilePluginsList.Select(p => Activator.CreateInstance(p) as Tile)
                .ToList();
            Tiles.ForEach(t => t.Initialize());
        }

        private List<Type> GetTilePlugins()
        {
            var match = typeof(Tile);
            var possiblePlugins = new List<Type>();

            var tilePath = Environment.GetEnvironmentVariable("TILEPATH") ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var f in Directory.GetFiles(tilePath, "*.dll", SearchOption.AllDirectories)
                     .Where(d => Path.GetFileName(d).StartsWith("Tiles.", StringComparison.CurrentCulture)))
            {
                var assembly = Assembly.LoadFrom(f);
                possiblePlugins.AddRange(assembly.GetTypes().Where(t => match.IsAssignableFrom(t) && !t.IsAbstract && t.IsClass));
            }

            return possiblePlugins.Distinct()
                .ToList();
        }
    }
}
