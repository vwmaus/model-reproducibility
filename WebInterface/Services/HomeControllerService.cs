using Microsoft.IdentityModel.Protocols;

namespace WebInterface.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using WebInterface.Classes;
    using WebInterface.Models;
    using System.Net;
    using Newtonsoft.Json;

    public class HomeControllerService : ControllerBase
    {
        public new string User => "User" + new Random().Next();

        public const string TemplatePath = "./Docker-Templates/";

        public string GamsDockerfilePath { get; set; }

        public string ModelDockerfilePath { get; set; }

        public void CreateGamsDockerfile(string version, string architecture, string licencePath = null, string outputFolder = "")
        {
            Debug.WriteLine("Create Gams Dockerfile...");

            const string licencePlaceholder = "#GAMS_LICENSE#";
            const string bitArchitecturePlaceholder = "#BIT_ARC#";
            const string gamsVersionPlaceholder = "#GAMS_VERSION#";

            string dockerfileContent;

            const string dockerTemplate = TemplatePath + "gams-dockerfile";

            using (var reader = new StreamReader(dockerTemplate))
            {
                dockerfileContent = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(licencePath))
            {
                dockerfileContent = dockerfileContent.Replace(@"COPY ${GAMS_LICENSE} /opt/gams/gamslice.txt", "# No licence file found or entered!");
            }
            else
            {
                licencePath = licencePath.Replace(@"\", "/");

                dockerfileContent = dockerfileContent.Replace(licencePlaceholder, licencePath);
            }

            dockerfileContent = dockerfileContent
                .Replace(bitArchitecturePlaceholder, architecture)
                .Replace(gamsVersionPlaceholder, version);

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

        public void CreateModelDockerfile(UserConfiguration userConfiguration, string outputFolder = "", string templateFileName = "Dockerfile-model")
        {
            Debug.WriteLine("Create Model Dockerfile");

            const string modelPlaceholder = "#MODEL#";
            const string modelversionPlaceholder = "#MODEL_VERSION#";

            string dockerfileContent;

            var dockerTemplate = TemplatePath + templateFileName;

            using (var reader = new StreamReader(dockerTemplate))
            {
                dockerfileContent = reader.ReadToEnd();
            }

            if (userConfiguration.SelectedGithubRepositoryVersion.IsNullOrEmpty())
            {
                throw new Exception("Version must not be null or empty!");
            }

            if (userConfiguration.SelectedGithubRepositoryVersion.IsNullOrEmpty())
            {
                throw new Exception("Model must not be null or empty!");
            }

            dockerfileContent = dockerfileContent
                .Replace(modelversionPlaceholder, userConfiguration.SelectedGithubRepositoryVersion)
                .Replace(modelPlaceholder, userConfiguration.SelectedGithubRepository);

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
            var outputfile = $@"./OutputZip/{filename}";
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

        public async Task<GeoNodeDocument> GetGeonodeData()
        {
            if (!(WebRequest.Create(@"http://geonode_geonode_1/api/documents/") is HttpWebRequest request))
            {
                return null;
            }

            request.UserAgent = this.User;

            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);

                if (response == null)
                {
                    return null;
                }

                var reader = new StreamReader(response.GetResponseStream());
                var responseData = reader.ReadToEnd();
                var document = JsonConvert.DeserializeObject<GeoNodeDocument>(responseData);

                return document;
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return null;
        }

        public async Task<List<GithubRepository>> GetGithubRepositories(string user)
        {
            if (!(WebRequest.Create("https://api.github.com/users/" + user + "/repos") is HttpWebRequest request))
            {
                return null;
            }

            request.UserAgent = this.User;

            try
            {

                var response = await request.GetResponseAsync().ConfigureAwait(false);

                if (response == null)
                {
                    return null;
                }

                var reader = new StreamReader(response.GetResponseStream());
                var responseData = reader.ReadToEnd();
                var document = JsonConvert.DeserializeObject<List<GithubRepository>>(responseData);

                return document;
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return null;
        }

        public async Task<DockerhubRepositoryTags> GetDockerhubRepositoryTags(string user, string repo)
        {
            // https://hub.docker.com/v2/repositories/iiasa/gams/tags/
            if (!(WebRequest.Create("https://hub.docker.com/v2/repositories/" + user + "/" + repo + "/tags") is HttpWebRequest request))
            {
                return null;
            }

            request.UserAgent = this.User;

            try
            {

                var response = await request.GetResponseAsync().ConfigureAwait(false);

                if (response == null)
                {
                    return null;
                }

                var reader = new StreamReader(response.GetResponseStream());
                var responseData = reader.ReadToEnd();
                var document = JsonConvert.DeserializeObject<DockerhubRepositoryTags>(responseData);

                return document;
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return null;
        }

        public async Task<List<GithubContent>> GetGithubRepoContents(string user, string repo)
        {
            if (!(WebRequest.Create("https://api.github.com/repos/" + user + "/" + repo + "/contents") is HttpWebRequest request))
            {
                return null;
            }

            request.UserAgent = this.User;

            try
            {

                var response = await request.GetResponseAsync().ConfigureAwait(false);

                if (response == null)
                {
                    return null;
                }

                var reader = new StreamReader(response.GetResponseStream());
                var responseData = reader.ReadToEnd();
                var document = JsonConvert.DeserializeObject<List<GithubContent>>(responseData);

                return document;
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return null;
        }

        public async Task<List<GithubRepositoryVersion>> GetGithubRepoVersions(string user, string repository)
        {
            if (!(WebRequest.Create("https://api.github.com/repos/" + user + "/" + repository + "/git/refs/tags") is
                HttpWebRequest request))
            {
                return null;
            }

            request.UserAgent = this.User;

            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);

                if (response == null)
                {
                    return null;
                }

                var reader = new StreamReader(response.GetResponseStream());
                var responseData = reader.ReadToEnd();
                var document = JsonConvert.DeserializeObject<List<GithubRepositoryVersion>>(responseData);

                return document;
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return null;
        }
    }
}
