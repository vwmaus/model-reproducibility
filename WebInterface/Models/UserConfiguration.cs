namespace WebInterface.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class UserConfiguration
    {
        public List<SelectListItem> GithubRepositories { get; set; }

        [Required(ErrorMessage = "The model is required.")]
        public string SelectedGithubRepository { get; set; }

        public List<SelectListItem> GeoNodeDocuments { get; set; }

        [Required(ErrorMessage = "The geonode model data is required.")]
        public string SelectedGeoNodeDocument { get; set; }

        public List<SelectListItem> GithubRepositoryVersions { get; set; }

        [Required(ErrorMessage = "The model version is required.")]
        public string SelectedGithubRepositoryVersion { get; set; }

        public List<SelectListItem> ProgramVersions { get; set; }

        [Required(ErrorMessage = "The program version is required.")]
        public string SelectedProgramVersion { get; set; }

        //[Required(ErrorMessage = "The geonode model tag is required.")]
        //public string GeonodeModelTag { get; set; }

        // --------------------------------------------------------------

        public string LicencePath { get; set; }

        [Required(ErrorMessage = "The program is required.")]
        public string Program { get; set; }

        [Required(ErrorMessage = "The program architecture is required.")]
        public string ProgramArchitecture { get; set; }

        private string githubUser;

        public string GitHubUser
        {
            get => string.IsNullOrEmpty(this.githubUser) ? "vwmaus" : this.githubUser;
            set => this.githubUser = value;
        }

        public bool SaveToDatabase { get; set; }

        public bool DownloadResult { get; set; }

        public UserConfiguration()
        {
            //https://stackoverflow.com/questions/26585495/there-is-no-viewdata-item-of-type-ienumerableselectlistitem-that-has-the-key

            this.GithubRepositories = new List<SelectListItem>();
            this.GeoNodeDocuments = new List<SelectListItem>();
            this.GithubRepositoryVersions = new List<SelectListItem>();
            this.ProgramVersions = new List<SelectListItem>();

            this.LicencePath = string.Empty;
            this.Program = string.Empty;
            this.SelectedProgramVersion = string.Empty;
            this.SelectedGeoNodeDocument = string.Empty;
            this.SelectedGithubRepository = string.Empty;
            this.SelectedGithubRepositoryVersion = string.Empty;
            //this.GeonodeModelTag = string.Empty;

            this.GitHubUser = string.Empty;
            this.SaveToDatabase = false;
            this.DownloadResult = false;
        }
    }

    public enum Architectures
    {
        x86_32,
        x64_64
    }
}
