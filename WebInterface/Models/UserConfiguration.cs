using Microsoft.AspNetCore.Http;

namespace WebInterface.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class UserConfiguration
    {
        public IFormFile File { set; get; }
        public IFormFile ModelDataFile { set; get; }

        public string FileName { get; set; }

        public List<SelectListItem> GithubRepositories { get; set; }

        [Required(ErrorMessage = "The model is required.")]
        public string SelectedGithubRepository { get; set; }

        public List<SelectListItem> GeoNodeDocuments { get; set; }

        //[Required(ErrorMessage = "The geonode model data is required.")]
        //public string SelectedGeoNodeDocument { get; set; }

        public List<SelectListItem> GithubRepositoryVersions { get; set; }

        public List<SelectListItem> GithubRepositoryBranches { get; set; }

        [Required(ErrorMessage = "The model version is required.")]
        public string SelectedGithubRepositoryVersion { get; set; }

        public List<SelectListItem> ProgramVersions { get; set; }

        [Required(ErrorMessage = "The program version is required.")]
        public string SelectedProgramVersion { get; set; }

        public List<SelectListItem> GeonodeModelTags { get; set; }

        //[Required(ErrorMessage = "The geonode model tag is required.")]
        //public string SelectedGeonodeModelTag { get; set; }

        public List<SelectListItem> Programs { get; set; }

        [Required(ErrorMessage = "The program is required.")]
        public string SelectedProgram { get; set; }

        // --------------------------------------------------------------

        public string LicencePath { get; set; }

        //[Required(ErrorMessage = "The program architecture is required.")]
        //public string ProgramArchitecture { get; set; }

        private string githubUser;

        private string dockerhubUser;

        private string dockerhubProgramRepository;

        public string GitHubUser
        {
            get => string.IsNullOrEmpty(this.githubUser) ? "ptrkrnstnr" : this.githubUser; // vwmaus
            set => this.githubUser = value;
        }

        public string DockerhubUser
        {
            get => string.IsNullOrEmpty(this.dockerhubUser) ? "iiasa" : this.dockerhubUser;
            set => this.dockerhubUser = value;
        }

        public string DockerhubProgramRepository
        {
            get => string.IsNullOrEmpty(this.dockerhubProgramRepository) ? "gams" : this.dockerhubProgramRepository;
            set => this.dockerhubProgramRepository = value;
        }

        public string ModelInputDataFile { get; set; }

        public bool SaveToDatabase { get; set; }

        public bool DownloadResult { get; set; }

        public UserConfiguration()
        {
            //https://stackoverflow.com/questions/26585495/there-is-no-viewdata-item-of-type-ienumerableselectlistitem-that-has-the-key

            this.GithubRepositories = new List<SelectListItem>();
            this.GeoNodeDocuments = new List<SelectListItem>();
            this.GithubRepositoryVersions = new List<SelectListItem>();
            this.GithubRepositoryBranches = new List<SelectListItem>();
            this.ProgramVersions = new List<SelectListItem>();
            this.Programs = new List<SelectListItem>();
            this.GeonodeModelTags = new List<SelectListItem>();

            this.ModelInputDataFile = string.Empty;
            this.LicencePath = string.Empty;
            this.SelectedProgramVersion = string.Empty;
            //this.SelectedGeoNodeDocument = string.Empty;
            this.SelectedGithubRepository = string.Empty;
            this.SelectedGithubRepositoryVersion = string.Empty;
            this.SelectedProgram = string.Empty;
            //this.GeonodeModelTag = string.Empty;

            this.GitHubUser = string.Empty;
            this.DockerhubUser = string.Empty;
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
