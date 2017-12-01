namespace WebInterface.Models
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class GeoNodeDocumentData
    {
        [JsonProperty("abstract")]
        public string Abstract { get; set; }

        [JsonProperty("abstract_en")]
        public string AbstractEn { get; set; }

        [JsonProperty("bbox_x0")]
        public string BboxX0 { get; set; }

        [JsonProperty("bbox_x1")]
        public string BboxX1 { get; set; }

        [JsonProperty("bbox_y0")]
        public string BboxY0 { get; set; }

        [JsonProperty("bbox_y1")]
        public string BboxY1 { get; set; }

        [JsonProperty("category")]
        public Category Category { get; set; }

        [JsonProperty("constraints_other")]
        public string ConstraintsOther { get; set; }

        [JsonProperty("constraints_other_en")]
        public string ConstraintsOtherEn { get; set; }

        [JsonProperty("csw_anytext")]
        public string CswAnytext { get; set; }

        [JsonProperty("csw_insert_date")]
        public string CswInsertDate { get; set; }

        [JsonProperty("csw_mdsource")]
        public string CswMdsource { get; set; }

        [JsonProperty("csw_schema")]
        public string CswSchema { get; set; }

        [JsonProperty("csw_type")]
        public string CswType { get; set; }

        [JsonProperty("csw_typename")]
        public string CswTypename { get; set; }

        [JsonProperty("csw_wkt_geometry")]
        public string CswWktGeometry { get; set; }

        [JsonProperty("data_quality_statement")]
        public string DataQualityStatement { get; set; }

        [JsonProperty("data_quality_statement_en")]
        public string DataQualityStatementEn { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("date_type")]
        public string DateType { get; set; }

        [JsonProperty("detail_url")]
        public string DetailUrl { get; set; }

        [JsonProperty("doc_file")]
        public string DocFile { get; set; }

        [JsonProperty("doc_type")]
        public string DocType { get; set; }

        [JsonProperty("doc_url")]
        public string DocUrl { get; set; }

        [JsonProperty("edition")]
        public string Edition { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("is_published")]
        public bool IsPublished { get; set; }

        [JsonProperty("keywords")]
        public List<object> Keywords { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("maintenance_frequency")]
        public object MaintenanceFrequency { get; set; }

        [JsonProperty("metadata_uploaded")]
        public bool MetadataUploaded { get; set; }

        [JsonProperty("metadata_uploaded_preserve")]
        public bool MetadataUploadedPreserve { get; set; }

        [JsonProperty("metadata_xml")]
        public string MetadataXml { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("popular_count")]
        public long PopularCount { get; set; }

        [JsonProperty("purpose")]
        public string Purpose { get; set; }

        [JsonProperty("purpose_en")]
        public string PurposeEn { get; set; }

        [JsonProperty("rating")]
        public long Rating { get; set; }

        [JsonProperty("regions")]
        public List<string> Regions { get; set; }

        [JsonProperty("resource_uri")]
        public string ResourceUri { get; set; }

        [JsonProperty("share_count")]
        public long ShareCount { get; set; }

        [JsonProperty("srid")]
        public string Srid { get; set; }

        [JsonProperty("supplemental_information")]
        public string SupplementalInformation { get; set; }

        [JsonProperty("supplemental_information_en")]
        public string SupplementalInformationEn { get; set; }

        [JsonProperty("temporal_extent_end")]
        public object TemporalExtentEnd { get; set; }

        [JsonProperty("temporal_extent_start")]
        public object TemporalExtentStart { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_en")]
        public string TitleEn { get; set; }

        [JsonProperty("tkeywords")]
        public List<object> Tkeywords { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }

    public partial class Owner
    {
        [JsonProperty("area")]
        public object Area { get; set; }

        [JsonProperty("city")]
        public object City { get; set; }

        [JsonProperty("count")]
        public object Count { get; set; }

        [JsonProperty("country")]
        public object Country { get; set; }

        [JsonProperty("date_joined")]
        public string DateJoined { get; set; }

        [JsonProperty("delivery")]
        public object Delivery { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("fax")]
        public object Fax { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("organization")]
        public object Organization { get; set; }

        [JsonProperty("position")]
        public object Position { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }

        [JsonProperty("resource_uri")]
        public string ResourceUri { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("voice")]
        public object Voice { get; set; }

        [JsonProperty("zipcode")]
        public object Zipcode { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("count")]
        public object Count { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("description_en")]
        public string DescriptionEn { get; set; }

        [JsonProperty("fa_class")]
        public string FaClass { get; set; }

        [JsonProperty("gn_description")]
        public string GnDescription { get; set; }

        [JsonProperty("gn_description_en")]
        public string GnDescriptionEn { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("is_choice")]
        public bool IsChoice { get; set; }

        [JsonProperty("resource_uri")]
        public string ResourceUri { get; set; }
    }
}
