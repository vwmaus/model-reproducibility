namespace WebInterface.Classes
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Models;

    public static class JsonHelper
    {
        public static List<GithubRepository> GithubRepositoriesFromJson(string json) => JsonConvert.DeserializeObject<List<GithubRepository>>(json, Converter.Settings);

        public static string GithubRepositoriesToJson(List<GithubRepository> self) => JsonConvert.SerializeObject(self, Converter.Settings);

        public static List<GithubRepositoryVersion> GithubRepositoryVersionFromJson(string json) => JsonConvert.DeserializeObject<List<GithubRepositoryVersion>>(json, Converter.Settings);

        public static string GithubRepositoryVersionToJson(List<GithubRepositoryVersion> self) => JsonConvert.SerializeObject(self, Converter.Settings);

        public static GeoNodeDocument GeoNodeDocumentFromJson(string json) => JsonConvert.DeserializeObject<GeoNodeDocument>(json, Converter.Settings);

        public static string GeoNodeDocumentToJson(GeoNodeDocument self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
