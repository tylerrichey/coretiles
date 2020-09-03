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

        public static string GetConfigDirectory()
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CoreTiles");

        public static async Task SaveConfigFile<T>(T config, string name)
        {
            try
            {
                var fileName = Path.Combine(GetConfigDirectory(), name + ".json");
                var data = JsonConvert.SerializeObject(config);
                Directory.CreateDirectory(GetConfigDirectory());
                using var streamwriter = new StreamWriter(fileName);
                await streamwriter.WriteAsync(data);
            }
            catch
            {

            }
        }

        public static async Task<T> LoadConfigFile<T>(string name)
        {
            try
            {
                var fileName = Path.Combine(GetConfigDirectory(), name + ".json");
                using var streamReader = new StreamReader(fileName);
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
