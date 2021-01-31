namespace WorkerServiceTest
{
    public class RemoteCommandProcessorOptions
    {
        public const string SectionName = "RemoteApi";

        public string BaseUrl { get; set; }
        public string UrlTemplate { get; set; }
        public string ApiKey { get; set; }
        public string UserAgent { get; set; }
    }
}
