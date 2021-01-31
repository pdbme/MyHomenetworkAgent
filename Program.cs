using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using Serilog.Events;

namespace WorkerServiceTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File("worker.log")
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting up service");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "There was a problem with the service");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IRemoteCommandProcessor, RemoteCommandProcessor>();
                    //services.AddTransient<IRemoteCommandActions, RemoteCommandActions>();
                    services.AddHostedService<Worker>();
                }).UseSerilog();
    }
}
