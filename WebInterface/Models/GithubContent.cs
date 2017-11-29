namespace WebInterface.Models
{
    using Newtonsoft.Json;

    public class GithubContent
    {
        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("git_url")]
        public string GitUrl { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public partial class Links
    {
        [JsonProperty("git")]
        public string Git { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }
    }
}