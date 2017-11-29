namespace WebInterface.Models
{
    using Newtonsoft.Json;

    public partial class DockerhubRepositoryTags
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public Result[] Results { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("creator")]
        public long Creator { get; set; }

        [JsonProperty("full_size")]
        public long FullSize { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("image_id")]
        public object ImageId { get; set; }

        [JsonProperty("images")]
        public Image[] Images { get; set; }

        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; }

        [JsonProperty("last_updater")]
        public long LastUpdater { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("repository")]
        public long Repository { get; set; }

        [JsonProperty("v2")]
        public bool V2 { get; set; }
    }

    public partial class Image
    {
        [JsonProperty("architecture")]
        public string Architecture { get; set; }

        [JsonProperty("features")]
        public object Features { get; set; }

        [JsonProperty("os")]
        public string Os { get; set; }

        [JsonProperty("os_features")]
        public object OsFeatures { get; set; }

        [JsonProperty("os_version")]
        public object OsVersion { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("variant")]
        public object Variant { get; set; }
    }
}
