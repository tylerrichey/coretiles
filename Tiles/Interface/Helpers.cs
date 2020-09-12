using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{
    public static class Helpers
    {
        public static HttpClient HttpClient = new HttpClient();

        public static string GetConfigDirectory()
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoreTiles");

        private static string TileTypeToConfigFileName(this Type t)
            => Path.Combine(GetConfigDirectory(), t.Name + "Config.json");

        private const string systemTileType = "SystemTile";

        private static ConcurrentDictionary<string, object> cachedConfigs = new ConcurrentDictionary<string, object>();

        public static T GetConfig<P, T>() where P : Tile => GetConfig<T>(typeof(P).Name);

        private static T GetConfig<T>(string tileName)
        {
            if (cachedConfigs.TryGetValue(tileName, out object value))
            {
                return (T)value;
            }

            return (T)Activator.CreateInstance(typeof(T));
        }

        public static SystemTileConfig SystemConfig => GetConfig<SystemTileConfig>(systemTileType);

        public static async Task SaveConfigFile<T>(object config) where T : Tile
        {
            try
            {
                var data = JsonConvert.SerializeObject(config);
                Directory.CreateDirectory(GetConfigDirectory());
                using var streamwriter = new StreamWriter(typeof(T).TileTypeToConfigFileName());
                await streamwriter.WriteAsync(data);
                cachedConfigs.AddOrUpdate(typeof(T).Name, config, (t, o) => config);
            }
            catch
            {
                throw;
            }
        }

        public static async Task LoadConfigFile(Type tileType, Type configType)
        {
            try
            {
                using var streamReader = new StreamReader(tileType.TileTypeToConfigFileName());
                var data = await streamReader.ReadToEndAsync();
                var config = JsonConvert.DeserializeObject(data, configType);
                cachedConfigs.AddOrUpdate(tileType.Name, config, (t, o) => config);
            }
            catch (FileNotFoundException)
            {
                //do nothing
            }
            catch
            {
                throw;
            }
        }
    }
}
