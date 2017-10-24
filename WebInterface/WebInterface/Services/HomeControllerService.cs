using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WebInterface.Models;

namespace WebInterface.Services
{
    public class HomeControllerService
    {
        public string GamsDockerFilePath { get; set; }

        public void CreateGamsDockerfile(string licencePath = null, string outputFolder = "")
        {
            Debug.WriteLine("CreateGamsDockerfile");

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

                dockerfileContent = dockerfileContent.Replace("$GAMS_LICENSE", licencePath);
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

                this.GamsDockerFilePath = outputfile;
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public string CreateDockerfile(DockerfileDataModel dockerfileDataModel)
        {
            return string.Empty;
        }
    }
}
