// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using WebInterface.Models;
//
//    var data = DockerhubRepository.FromJson(jsonString);
//
namespace WebInterface.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class DockerhubRepository
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public List<RepoResult> Results { get; set; }
    }

    public class RepoResult
    {
        [JsonProperty("can_edit")]
        public bool CanEdit { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("is_automated")]
        public bool IsAutomated { get; set; }

        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }

        [JsonProperty("last_updated")]
        public string LastUpdated { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("pull_count")]
        public long PullCount { get; set; }

        [JsonProperty("repository_type")]
        public string RepositoryType { get; set; }

        [JsonProperty("star_count")]
        public long StarCount { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }
    }
}
