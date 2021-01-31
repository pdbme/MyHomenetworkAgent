using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MyHomenetworkAgent
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IRemoteCommandProcessor processor;
        private readonly int pollingInterval;

        public Worker(ILogger<Worker> logger, IRemoteCommandProcessor processor, IConfiguration config)
        {
            this.logger = logger;
            this.processor = processor;

            pollingInterval = config.GetSection("Agent").GetValue<int>("PollingInterval");
            if (pollingInterval < 1000)
            {
                pollingInterval = 1000; // Minimum polling interval is 1000 milliseconds
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogDebug("Agent running at: {time}", DateTimeOffset.Now);
                processor.DoQuery();
                await Task.Delay(pollingInterval, stoppingToken);
            }
        }
    }
}
