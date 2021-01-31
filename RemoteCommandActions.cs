using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace MyHomenetworkAgent
{
    public class RemoteCommandActions
    {
        private readonly ILogger<RemoteCommandProcessor> logger;
        private readonly RemoteCommandProcessorOptions configOptions;
        private readonly HttpClient client;

        public RemoteCommandActions(ILogger<RemoteCommandProcessor> logger, IConfiguration config)
        {
            this.logger = logger;

            configOptions = new RemoteCommandProcessorOptions();
            config.GetSection(RemoteCommandProcessorOptions.SectionName).Bind(configOptions);
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", configOptions.UserAgent);
        }

        public void GetFiles(string id, string filepath, string searchpattern)
        {
            logger.LogInformation("GetFiles called: {id}, {filepath} {searchpattern}", id, filepath, searchpattern);

            GetFilesResult fileResult =new GetFilesResult()
            {
                Id = id,
                ApiKey = configOptions.ApiKey
            };

            DirectoryInfo dirInfo = new DirectoryInfo(filepath);
            if (dirInfo.Exists)
            {
                List<FileInfo> fileInfos = dirInfo.EnumerateFiles(searchpattern, SearchOption.AllDirectories).ToList();
                List<GetFilesFileResult> results = fileInfos.Select(x => new GetFilesFileResult()
                {
                    Filename = x.Name,
                    Filesize = x.Length,
                    Path = x.DirectoryName
                }).ToList();
                fileResult.Files = results;
            }

            string callbackUrl = configOptions.BaseUrl + $"queuecallback";
            
            HttpResponseMessage response = Postdata(callbackUrl, JsonConvert.SerializeObject(fileResult)).Result;
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Postdata called {callbackUrl} succesfully.", callbackUrl);
            }

        }

        public void GetUrl(string id, string url)
        {
            logger.LogInformation("GetUrl called: {id}, {url}", id, url);

            string result = GetStringFromUrl(url).Result;

            string callbackUrl = configOptions.BaseUrl + $"queuecallback";

            string jsonBodyString = JsonConvert.SerializeObject(new GetUrlResult()
            {
                Id = id,
                ApiKey = configOptions.ApiKey,
                ResultBody = result
            });

            HttpResponseMessage response = Postdata(callbackUrl, jsonBodyString).Result;
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Postdata called {callbackUrl} succesfully.", callbackUrl);
            }

        }

        private async Task<string> GetStringFromUrl(string uri)
        {
            logger.LogInformation("Request: " + uri);

            try
            {
                return await client.GetStringAsync(uri);
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

            return "";
        }

        private async Task<HttpResponseMessage> Postdata(string uri, string jsonBodystring)
        {
            logger.LogInformation("POST-Request: " + uri + " Body-String: " + jsonBodystring);

            try
            {
                var stringContent = new StringContent(jsonBodystring, Encoding.UTF8, "application/json");
                return await client.PostAsync(uri, stringContent);
            }
            catch (HttpRequestException) // Non success
            {
                logger.LogError("An error occurred requesting {uri}.", uri);
            }
            catch (NotSupportedException) // When content type is not valid
            {
                logger.LogError("An error occurred requesting {uri}. The content type is not supported.", uri);
            }

            return default;
        }
    }

    public class GetUrlResult
    {
        public string Id { get; set; }
        public string ApiKey { get; set; }
        public string ResultBody { get; set; }
    }

    public class GetFilesResult
    {
        public string Id { get; set; }
        public string ApiKey { get; set; }
        public List<GetFilesFileResult> Files { get; set; }
    }

    public class GetFilesFileResult
    {
        public string Filename { get; set; }
        public string Path { get; set; }
        public long Filesize { get; set; }
    }
}
