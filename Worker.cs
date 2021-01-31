using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerServiceTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IRemoteCommandProcessor processor;

        public Worker(ILogger<Worker> logger, IRemoteCommandProcessor processor)
        {
            this.logger = logger;
            this.processor = processor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                processor.DoQuery();
                await Task.Delay(4000, stoppingToken);
            }
        }
    }
}
