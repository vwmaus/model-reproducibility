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

        public const string TemplatePath = "./docker_templates/";

        public string GamsDockerfilePath { get; set; }

        public string ModelDockerfilePath { get; set; }

        public void CreateGamsDockerfile(UserConfiguration config, string outputFolder = "")
        {
            if (config == null)
            {
                Debug.WriteLine("Config file is null");
                return;
            }

            Debug.WriteLine("Create Gams Dockerfile...");


            const string gamsVersionPlaceholder = "#GAMS_VERSION#";
            const string githubUserPlaceholder = "#GITHUB_USER#";
            const string modelPlaceholder = "#MODEL#";
            const string modelVersionPlaceholder = "#MODEL_VERSION#";
            const string geonodeDataVersionPlaceholder = "#DATA_VERSION#";

            const string dockerFileName = "Dockerfile-model";

            const string licencePlaceholder = "#GAMS_LICENSE#";

            string dockerfileContent;

            const string dockerTemplate = TemplatePath + dockerFileName;

            using (var reader = new StreamReader(dockerTemplate))
            {
                dockerfileContent = reader.ReadToEnd();
            }

            dockerfileContent = dockerfileContent
                .Replace(gamsVersionPlaceholder, config.SelectedProgramVersion)
                .Replace(githubUserPlaceholder, config.GitHubUser)
                .Replace(modelPlaceholder, config.SelectedGithubRepository)
                .Replace(modelVersionPlaceholder, config.SelectedGithubRepositoryVersion)
                .Replace(geonodeDataVersionPlaceholder, config.SelectedGeoNodeDocument);

            //if (string.IsNullOrEmpty(licencePath))
            //{
            //    dockerfileContent = dockerfileContent.Replace(@"COPY ${GAMS_LICENSE} /opt/gams/gamslice.txt", "# No licence file found or entered!");
            //}
            //else
            //{
            //    licencePath = licencePath.Replace(@"\", "/");

            //    dockerfileContent = dockerfileContent.Replace(licencePlaceholder, licencePath);
            //}

            //dockerfileContent = dockerfileContent
            //    .Replace(bitArchitecturePlaceholder, architecture)
            //    .Replace(gamsVersionPlaceholder, version);

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = @"./Output/";
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            var outputfile = Path.Combine(outputFolder, dockerFileName);
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
            var outputFolder = $@"./OutputZip/";
            const string filename = "dockerfiles.zip";
            var outputfile = Path.Combine(outputFolder, filename);
            const string inputPath = "./Output/";

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

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
            var url = "http://geonode_geonode_1/api/documents/";
            return await this.GetWebRequestContent<GeoNodeDocument>(url);
        }

        public async Task<GeoNodeDocumentData> GetGeoNodeDocumentData(string documentId)
        {
            var url = "http://geonode_geonode_1/api/documents/" + documentId;
            return await this.GetWebRequestContent<GeoNodeDocumentData>(url);
        }

        public async Task<List<GithubRepository>> GetGithubRepositories(string user)
        {
            var url = "https://api.github.com/users/" + user + "/repos";
            return await this.GetWebRequestContent<List<GithubRepository>>(url);
        }

        public async Task<DockerhubRepository> GetDockerhubRepositories(string user)
        {
            // https://hub.docker.com/v2/repositories/iiasa/
            var url = "https://hub.docker.com/v2/repositories/" + user;
            return await this.GetWebRequestContent<DockerhubRepository>(url);
        }

        public async Task<DockerhubRepositoryTags> GetDockerhubRepositoryTags(string user, string repo)
        {
            // https://hub.docker.com/v2/repositories/iiasa/gams/tags/
            var url = "https://hub.docker.com/v2/repositories/" + user + "/" + repo + "/tags";
            return await this.GetWebRequestContent<DockerhubRepositoryTags>(url);
        }

        public async Task<List<GithubContent>> GetGithubRepoContents(string user, string repo)
        {
            var url = "https://api.github.com/repos/" + user + "/" + repo + "/contents";
            return await this.GetWebRequestContent<List<GithubContent>>(url);
        }

        public async Task<List<GithubRepositoryVersion>> GetGithubRepoVersions(string user, string repository)
        {
            var url = "https://api.github.com/repos/" + user + "/" + repository + "/git/refs/tags";
            return await this.GetWebRequestContent<List<GithubRepositoryVersion>>(url);
        }

        public async Task<List<GithubBranch>> GetGithubBranches(string user, string repository)
        {
            var url = "https://api.github.com/repos/" + user + "/" + repository + "/branches";
            return await this.GetWebRequestContent<List<GithubBranch>>(url);
        }

        public async Task<T> GetWebRequestContent<T>(string url)
        {
            if (!(WebRequest.Create(url) is HttpWebRequest request))
            {
                return default(T);
            }

            request.UserAgent = this.User;

            try
            {

                var response = await request.GetResponseAsync().ConfigureAwait(false);

                if (response == null)
                {
                    return default(T);
                }

                var reader = new StreamReader(response.GetResponseStream());
                var responseData = reader.ReadToEnd();
                var document = JsonConvert.DeserializeObject<T>(responseData);

                return document;
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return default(T);
        }
    }
}
