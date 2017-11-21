namespace WebInterface.Models
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class GeoNodeDocument
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("objects")]
        public List<Object> Documents { get; set; }
    }

    public partial class Object
    {
        [JsonProperty("abstract")]
        public string Abstract { get; set; }

        [JsonProperty("category__gn_description")]
        public string CategoryGnDescription { get; set; }

        [JsonProperty("csw_type")]
        public string CswType { get; set; }

        [JsonProperty("csw_wkt_geometry")]
        public string CswWktGeometry { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("detail_url")]
        public string DetailUrl { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("owner__username")]
        public string OwnerUsername { get; set; }

        [JsonProperty("popular_count")]
        public long PopularCount { get; set; }

        [JsonProperty("rating")]
        public long Rating { get; set; }

        [JsonProperty("share_count")]
        public long ShareCount { get; set; }

        [JsonProperty("srid")]
        public string Srid { get; set; }

        [JsonProperty("supplemental_information")]
        public string SupplementalInformation { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("limit")]
        public long Limit { get; set; }

        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("total_count")]
        public long TotalCount { get; set; }
    }
}
