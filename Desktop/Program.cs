using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using CoreTiles.Tiles;
using Serilog;

namespace CoreTiles.Desktop
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var logDirectory = Path.Combine(Helpers.GetConfigDirectory(), "Logs");
                Directory.CreateDirectory(logDirectory);
                var fileName = Path.Combine(logDirectory, $"CoreTiles-{DateTime.Now:yyyyMMdd_hhmmss}.log");
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.File(fileName,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{SourceContext}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Main loop exception");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                //.LogToDebug(Avalonia.Logging.LogEventLevel.Verbose)
                .UseReactiveUI();
    }
}
