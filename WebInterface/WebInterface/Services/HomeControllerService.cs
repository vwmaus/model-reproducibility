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
    public class HomeControllerService
    {
        public string GamsDockerfilePath { get; set; }

        public string ModelDockerfilePath { get; set; }

        public void CreateGamsDockerfile(string licencePath = null, string outputFolder = "")
        {
            Debug.WriteLine("Create Gams Dockerfile");

            const string licencePlaceholder = "$GAMS_LICENSE";

            string dockerfileContent;

            const string dockerTemplate = @"./Docker-Templates/gams-dockerfile";

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

            try
            {
                if (string.IsNullOrEmpty(outputFolder))
                {
                    outputFolder = @"./Output/";
                }

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                var outputfile = Path.Combine(outputFolder, "gams-dockerfile");
                File.CreateText(outputfile);

                File.WriteAllText(outputfile, dockerfileContent);

                this.GamsDockerfilePath = outputfile;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public void CreateModelDockerfile(UserConfiguration userConfiguration, string outputFolder = "", string templateFileName = "transport-model-dockerfile")
        {
            Debug.WriteLine("Create Model Dockerfile");

            const string modelPlaceholder = "${MODEL}";
            const string modelversionPlaceholder = "${MODEL_VERSION}";

            string dockerfileContent;

            var dockerTemplate = $@"./Docker-Templates/{templateFileName}";

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

            try
            {
                if (string.IsNullOrEmpty(outputFolder))
                {
                    outputFolder = @"./Output/";
                }

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                var outputfile = Path.Combine(outputFolder, templateFileName);
                File.CreateText(outputfile);

                File.WriteAllText(outputfile, dockerfileContent);

                this.GamsDockerfilePath = outputfile;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public void CreateModelDockerfile(string model, string modelversion, string outputFolder = "", string templateFileName = "transport-model-dockerfile")
        {
            CreateModelDockerfile(new UserConfiguration
            {
                Model = model,
                ModelVersion = modelversion
            });
        }
    }
}
