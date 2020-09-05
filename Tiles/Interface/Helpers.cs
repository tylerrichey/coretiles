using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreTiles.Tiles
{
    public static class Helpers
    {
        public static HttpClient HttpClient = new HttpClient();

        private static string GetConfigDirectory()
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoreTiles");

        private static string TileTypeToConfigFileName(this Type t)
            => Path.Combine(GetConfigDirectory(), t.Name + "Config.json");

        public static async Task SaveConfigFile<T>(object config) where T : Tile
        {
            try
            {
                var data = JsonConvert.SerializeObject(config);
                Directory.CreateDirectory(GetConfigDirectory());
                using var streamwriter = new StreamWriter(typeof(T).TileTypeToConfigFileName());
                await streamwriter.WriteAsync(data);
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
                using var streamReader = new StreamReader(typeof(P).TileTypeToConfigFileName());
                var data = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch
            {
                throw;
            }
        }
    }
}
