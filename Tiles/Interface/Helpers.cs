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

        private static ConcurrentDictionary<Type, object> cachedConfigs = new ConcurrentDictionary<Type, object>();

        public static async Task SaveConfigFile<T>(object config) where T : Tile
        {
            try
            {
                var data = JsonConvert.SerializeObject(config);
                Directory.CreateDirectory(GetConfigDirectory());
                using var streamwriter = new StreamWriter(typeof(T).TileTypeToConfigFileName());
                await streamwriter.WriteAsync(data);
                cachedConfigs.AddOrUpdate(typeof(T), config, (t, o) => config);
            }
            catch
            {
                throw;
            }
        }

        public static async Task<T> LoadConfigFile<P, T>() where P : Tile
        {
            try
            {
                if (cachedConfigs.TryGetValue(typeof(P), out object value))
                {
                    return (T)value;
                }
                using var streamReader = new StreamReader(typeof(P).TileTypeToConfigFileName());
                var data = await streamReader.ReadToEndAsync();
                var config = JsonConvert.DeserializeObject<T>(data);
                cachedConfigs.TryAdd(typeof(P), config);
                return config;
            }
            catch
            {
                throw;
            }
        }
    }
}
