using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyHomenetworkAgent
{
    public class RemoteCommandProcessor : IRemoteCommandProcessor
    {
        private readonly ILogger<RemoteCommandProcessor> logger;
        private readonly RemoteCommandProcessorOptions configOptions;
        private readonly IConfiguration config;
        private readonly HttpClient client;

        public RemoteCommandProcessor(ILogger<RemoteCommandProcessor> logger, IConfiguration config)
        {
            this.logger = logger;
            this.config = config;

            configOptions = new RemoteCommandProcessorOptions();
            config.GetSection(RemoteCommandProcessorOptions.SectionName).Bind(configOptions);
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", configOptions.UserAgent);
        }

        public void DoQuery()
        {
            List<RemoteCommand> commands = GetJson<List<RemoteCommand>>(configOptions.BaseUrl +
                                   $"getqueue?apikey={configOptions.ApiKey}").Result;
            if (commands == null || !commands.Any()) return;
            foreach (var command in commands)
            {
                logger.LogInformation("Result: {command} Arguments: {argLength}", command.Command, command.Arguments.Count);

                RemoteCommandActions actions = new RemoteCommandActions(logger, config);
                var actionsType = actions.GetType();
                var theMethod = actionsType.GetMethod(command.Command);

                // ReSharper disable once CoVariantArrayConversion
                object[] myobj = command.Arguments.ToArray();
                if (theMethod != null)
                {
                    try
                    {
                        theMethod.Invoke(actions, myobj);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Invoke of method {command} failed - arguments may not match.", command.Command);
                    }
                }
                else
                {
                    logger.LogError("Invoke of unkown method {command} failed", command.Command);
                }
            }
        }

        private async Task<T> GetJson<T>(string uri)
        {
            logger.LogInformation("Request: " + uri);

            try
            {
                return await client.GetFromJsonAsync<T>(uri);
            }
            catch (HttpRequestException) // Non success
            {
                logger.LogError("An error occurred requesting {uri}.", uri);
            }
            catch (NotSupportedException) // When content type is not valid
            {
                logger.LogError("An error occurred requesting {uri}. The content type is not supported.", uri);
            }
            catch (JsonException) // Invalid JSON
            {
                logger.LogError("An error occurred requesting {uri}.  Invalid JSON.", uri);
            }

            return default;
        }
    }
}
