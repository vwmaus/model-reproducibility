﻿using System;
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

        [Required(ErrorMessage = "The program architecture is required.")]
        public string ProgramArchitecture { get; set; }

        [Required(ErrorMessage = "The model is required.")]
        public string Model { get; set; }

        [Required(ErrorMessage = "The model version is required.")]
        public string ModelVersion { get; set; }

        public string GitHubUser { get; set; }

        public bool SaveToDatabase { get; set; }

        public bool DownloadResult { get; set; }

        [Required(ErrorMessage = "The geonode model data is required.")]
        public string GeonodeModelData { get; set; }

        [Required(ErrorMessage = "The geonode model tag is required.")]
        public string GeonodeModelTag { get; set; }

        public UserConfiguration()
        {
            //https://stackoverflow.com/questions/26585495/there-is-no-viewdata-item-of-type-ienumerableselectlistitem-that-has-the-key

            this.LicencePath = string.Empty;
            this.Program = string.Empty;
            this.ProgramVersion = string.Empty;
            this.Model = string.Empty;
            this.ModelVersion = string.Empty;
            this.GeonodeModelData = string.Empty;
            this.GeonodeModelTag = string.Empty;

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