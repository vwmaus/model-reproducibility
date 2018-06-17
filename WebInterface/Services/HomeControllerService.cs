﻿using WebInterface.Classes;

namespace WebInterface.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using WebInterface.Models;
    using System.Net;
    using ICSharpCode.SharpZipLib.GZip;
    using ICSharpCode.SharpZipLib.Tar;
    using Newtonsoft.Json;

    public class HomeControllerService : ControllerBase
    {
        private static string outputFolderName;

        private static string dockerFileName;

        private static string dockerFileZipName;

        private static string outputFilePath;

        private static string dockerFilePath;

        private static string resultFilePath;

        private static string dockerFileZipPath;

        public new string User => "User" + new Random().Next();

        public const string TemplatePath = "./docker_templates/";

        public static string OutputFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(outputFilePath))
                {
                    return outputFilePath;
                }

                outputFilePath = $@"./Output/{OutputFolderName}/";

                if (!Directory.Exists(outputFilePath))
                {
                    Directory.CreateDirectory(outputFilePath);
                }

                return outputFilePath;
            }
        }

        public static string DockerFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(dockerFilePath))
                {
                    return dockerFilePath;
                }

                dockerFilePath = $@"{OutputFilePath}/DockerfileOutput/";

                if (!Directory.Exists(dockerFilePath))
                {
                    Directory.CreateDirectory(dockerFilePath);
                }

                return dockerFilePath;
            }
        }

        public static string ResultFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(resultFilePath))
                {
                    return resultFilePath;
                }

                resultFilePath = $@"{OutputFilePath}/output/";

                if (!Directory.Exists(resultFilePath))
                {
                    Directory.CreateDirectory(resultFilePath);
                }

                return resultFilePath;
            }
        }

        public static string OutputFolderName
        {
            get
            {
                if (string.IsNullOrEmpty(outputFolderName))
                {
                    outputFolderName = DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                return outputFolderName;
            }
        }

        public static string DockerFileName
        {
            get
            {
                if (string.IsNullOrEmpty(dockerFileName))
                {
                    dockerFileName = "Dockerfile"; // "Dockerfile-model"
                }

                return dockerFileName;
            }
            set => dockerFileName = value;
        }

        public static string DockerFileZipPath
        {
            get
            {
                if (!string.IsNullOrEmpty(dockerFileZipPath))
                {
                    return dockerFileZipPath;
                }

                dockerFileZipPath = $@"./OutputZip/{OutputFolderName}";

                if (!Directory.Exists(dockerFileZipPath))
                {
                    Directory.CreateDirectory(dockerFileZipPath);
                }

                return dockerFileZipPath;
            }
        }

        public static string DockerFileZipName
        {
            get
            {
                if (string.IsNullOrEmpty(dockerFileZipName))
                {
                    dockerFileZipName = "dockerfiles.zip";
                }

                return dockerFileZipName;
            }
            set => dockerFileZipName = value;
        }

        public string GamsDockerfilePath { get; set; }

        public string ModelDockerfilePath { get; set; }

        public void DownloadZipDataToFolder(string url, string outputFile)
        {
            byte[] data;
            using (var client = new WebClient())
            {
                data = client.DownloadData(url);
            }

            System.IO.File.WriteAllBytes(outputFile, data);
        }

        public void CreateTarGz(string tgzFilename, string fileName)
        {
            using (var outStream = System.IO.File.Create(tgzFilename))
            using (var gzipStream = new GZipOutputStream(outStream))
            using (var tarArchive = TarArchive.CreateOutputTarArchive(gzipStream))
            {
                var tarEntry = TarEntry.CreateEntryFromFile(fileName);
                tarEntry.Name = Path.GetFileName(fileName);

                tarArchive.WriteEntry(tarEntry, true);
            }
        }

        public string CreateGamsDockerfile(UserConfiguration config)
        {
            if (config == null)
            {
                Logger.Log("Config file is null");
                return string.Empty;
            }

            Logger.Log("Creating Gams Dockerfile...");

            const string gamsVersionPlaceholder = "#GAMS_VERSION#";
            const string githubUserPlaceholder = "#GITHUB_USER#";
            const string modelPlaceholder = "#MODEL#";
            const string modelVersionPlaceholder = "#MODEL_VERSION#";
            const string inputDataFilePathPlaceholder = "#INPUT_DATA_FILE_PATH#";
            const string licencePlaceholder = "#LICENSE_PATH#";

            var dockerTemplate = TemplatePath + DockerFileName;

            Logger.Log("Dockerfile-Path: " + Path.GetFullPath(dockerTemplate));

            var dockerfileContent = new List<string>();
            using (var reader = new StreamReader(dockerTemplate))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                        dockerfileContent.Add(line);
                }
            }

            dockerfileContent = dockerfileContent.Select(s => s
                    .Replace(gamsVersionPlaceholder, config.SelectedProgramVersion)
                    .Replace(githubUserPlaceholder, config.GitHubUser)
                    .Replace(modelPlaceholder, config.SelectedGithubRepository)
                    .Replace(modelVersionPlaceholder, config.SelectedGithubRepositoryVersion)
                    .Replace(inputDataFilePathPlaceholder, config.ModelInputDataFile)
                    .Replace(licencePlaceholder, config.LicencePath)).ToList();

            string altMsg;
            if (string.IsNullOrEmpty(config.LicencePath))
            {
                // No license provided
                altMsg = "# No licence file found or entered!";

                dockerfileContent = dockerfileContent.Select(s => s
                    .Replace(@"COPY ${LICENSE_PATH} /opt/gams/gamslice.txt", altMsg)
                    .Replace(@"RUN curl -SL ${LICENSE_PATH} --create-dirs -o /opt/gams/gamslice.txt", altMsg)
                    .Replace(dockerfileContent.FirstOrDefault(x => x.StartsWith("CMD")), altMsg)
                ).ToList();
            }
            else
            {
                config.LicencePath = config.LicencePath.Replace(@"\", "/");
            }

            if (string.IsNullOrEmpty(config.ModelInputDataFile))
            {
                // No input data provided
                altMsg = "# No model input data file uploaded";

                dockerfileContent = dockerfileContent.Select(s => s
                    .Replace(@"COPY ${INPUT_DATA_FILE_PATH} /workspace/data.zip", altMsg)
                    .Replace(@"RUN curl -SL ${INPUT_DATA_FILE_PATH} --create-dirs -o /workspace/data.zip", altMsg)
                    .Replace(@"RUN unzip -o /workspace/data.zip -d /workspace/data", altMsg))
                    .ToList();
            }

            var outputfile = Path.Combine(DockerFilePath, DockerFileName);

            System.IO.File.CreateText(outputfile);
            System.IO.File.WriteAllLines(outputfile, dockerfileContent);

            this.GamsDockerfilePath = outputfile;

            return this.GamsDockerfilePath;
        }

        public string CreateDockerZipFile()
        {
            var outputfile = Path.Combine(DockerFileZipPath, DockerFileZipName);

            if (System.IO.File.Exists(outputfile))
            {
                System.IO.File.Delete(outputfile);
            }

            ZipFile.CreateFromDirectory(OutputFilePath, outputfile);

            return outputfile;
        }

        public IActionResult DownloadFile(string filepath, string downloadFilename)
        {
            // https://stackoverflow.com/questions/317315/asp-net-mvc-relative-paths
            //https://stackoverflow.com/questions/35237863/download-file-using-mvc-core

            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
            return this.File(fileBytes, "application/x-msdownload", downloadFilename);
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
                Logger.Log(ex.Message);
            }

            return default(T);
        }
    }
}
