namespace WebInterface.Models
{
    using Newtonsoft.Json;

    public partial class GithubBranch
    {
        [JsonProperty("commit")]
        public Commit Commit { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Commit
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
