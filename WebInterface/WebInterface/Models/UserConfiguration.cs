using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Models
{
    public class UserConfiguration
    {
        public string LicencePath { get; set; }

        [Required(ErrorMessage = "The program is required.")]
        public string Program { get; set; }

        [Required(ErrorMessage = "The program version is required.")]
        public string ProgramVersion { get; set; }

        [Required(ErrorMessage = "The model is required.")]
        public string Model { get; set; }

        [Required(ErrorMessage = "The model version is required.")]
        public string ModelVersion { get; set; }

        public string GitHubUser { get; set; }

        public bool SaveToDatabase { get; set; }

        public bool DownloadResult { get; set; }

        public UserConfiguration()
        {
            this.LicencePath = string.Empty;
            this.Program = string.Empty;
            this.ProgramVersion = string.Empty;
            this.Model = string.Empty;
            this.ModelVersion = string.Empty;

            this.GitHubUser = string.Empty;
            this.SaveToDatabase = false;
            this.DownloadResult = false;
        }
    }
}
