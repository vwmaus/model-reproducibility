using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WebInterface.Classes;
using WebInterface.Models;

namespace WebInterface.Services
{
    public class HomeControllerService : ControllerBase
    {
        public const string TemplatePath = "./Docker-Templates/";

        public string GamsDockerfilePath { get; set; }

        public string ModelDockerfilePath { get; set; }

        public void CreateGamsDockerfile(string licencePath = null, string outputFolder = "")
        {
            Debug.WriteLine("Create Gams Dockerfile");

            const string licencePlaceholder = "$GAMS_LICENSE";

            string dockerfileContent;

            const string dockerTemplate = TemplatePath + "gams-dockerfile";

            using (var reader = new StreamReader(dockerTemplate))
            {
                dockerfileContent = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(licencePath))
            {
                dockerfileContent = dockerfileContent.Replace(@"COPY $GAMS_LICENSE /opt/gams/gamslice.txt", string.Empty);
            }
            else
            {
                licencePath = licencePath.Replace(@"\", "/");

                dockerfileContent = dockerfileContent.Replace(licencePlaceholder, licencePath);
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = @"./Output/";
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            var outputfile = Path.Combine(outputFolder, "gams-dockerfile");
            System.IO.File.CreateText(outputfile);

            System.IO.File.WriteAllText(outputfile, dockerfileContent);

            this.GamsDockerfilePath = outputfile;
        }

        public void CreateModelDockerfile(UserConfiguration userConfiguration, string outputFolder = "", string templateFileName = "transport-model-dockerfile")
        {
            Debug.WriteLine("Create Model Dockerfile");

            const string modelPlaceholder = "${MODEL}";
            const string modelversionPlaceholder = "${MODEL_VERSION}";

            string dockerfileContent;

            var dockerTemplate = TemplatePath + templateFileName;

            using (var reader = new StreamReader(dockerTemplate))
            {
                dockerfileContent = reader.ReadToEnd();
            }

            if (userConfiguration.ModelVersion.IsNullOrEmpty())
            {
                throw new Exception("Version must not be null or empty!");
            }

            if (userConfiguration.Model.IsNullOrEmpty())
            {
                throw new Exception("Model must not be null or empty!");
            }

            dockerfileContent = dockerfileContent
                .Replace(modelversionPlaceholder, userConfiguration.ModelVersion)
                .Replace(modelPlaceholder, userConfiguration.Model);

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = @"./Output/";
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            var outputfile = Path.Combine(outputFolder, templateFileName);
            System.IO.File.CreateText(outputfile);

            System.IO.File.WriteAllText(outputfile, dockerfileContent);

            this.GamsDockerfilePath = outputfile;
        }

        public string CreateDockerZipFile()
        {
            const string filename = "dockerfiles.zip";
            string outputfile = $@"./OutputZip/{filename}";
            const string inputPath = "./Output/";

            if (System.IO.File.Exists(outputfile))
            {
                System.IO.File.Delete(outputfile);
            }

            ZipFile.CreateFromDirectory(inputPath, outputfile);

            return outputfile;
        }

        public IActionResult DownloadFile(string filepath, string downloadFilename)
        {
            // https://stackoverflow.com/questions/317315/asp-net-mvc-relative-paths
            //https://stackoverflow.com/questions/35237863/download-file-using-mvc-core

            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
            return File(fileBytes, "application/x-msdownload", downloadFilename);
        }
    }
}
