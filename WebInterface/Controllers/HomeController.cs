using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Hosting;
using WebInterface.Classes;
using WebInterface.Docker;

namespace WebInterface.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using WebInterface.Models;
    using WebInterface.Services;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.AspNetCore.Hosting.Server;

    public class HomeController : Controller
    {
        public readonly DockerService DockerService;

        private readonly IHostingEnvironment hostingEnvironment;

        public HomeController(DockerService dockerService, IHostingEnvironment environment)
        {
            this.DockerService = dockerService;
            this.hostingEnvironment = environment;
        }

        public List<GithubRepository> GithubRepositories { get; set; }

        public async Task<IActionResult> Index()
        {
            var userConfig = await this.GetUserConfig(new UserConfiguration());

            //var list = await this.DockerService.GetContainerList();

            //var icp = new ImagesCreateParameters();
            //var res = await this.DockerService.DockerClient.Images.CreateImageAsync(icp, new AuthConfig() { });

            return this.View(userConfig);
        }

        public async void CreateModelContainer(UserConfiguration config)
        {
            // https://github.com/Microsoft/Docker.DotNet/issues/134
            // https://github.com/Microsoft/Docker.DotNet/issues/270
            // https://github.com/Microsoft/Docker.DotNet/issues/212

            string fileContent;
            using (var reader = new StreamReader("./Output/Dockerfile-model"))
            {
                fileContent = reader.ReadToEnd();
            }

            var lines = Regex.Split(fileContent, "\r\n|\r|\n").ToList();

            var image = lines.First(x => x.StartsWith("FROM")).Split("FROM")[1].Trim();
            var parent = image.Split(":")[0].Trim();
            var tag = image.Split(":")[1].Trim();

            var statusUpdate = await this.DockerService.DockerClient.Images.PullImageAsync(new ImagesPullParameters { Parent = parent, Tag = tag }, new AuthConfig());

            using (var reader = new StreamReader(statusUpdate))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Debug.WriteLine(line);
                }
            }

            var containerName = image.Split(":")[0].Replace("/", "_");

            var containers = await this.DockerService.GetContainerList();

            if (containers.Exists(x => x.Name == containerName))
            {
                var container = containers.FirstOrDefault(x => x.Name == containerName);
                if (container != null)
                {
                    await this.DockerService.DockerClient.Containers.RemoveContainerAsync(container.Id, new ContainerRemoveParameters {Force = true});
                }
            }

            var maintainer = lines.First(x => x.StartsWith("MAINTAINER")).Split("MAINTAINER")[1].Trim();
            var envVariables = lines.Where(x => x.StartsWith("ENV")).Select(envVariable => envVariable.Split("ENV")[1].Trim()).ToList();
            var workingdir = lines.First(x => x.StartsWith("WORKDIR")).Split("WORKDIR")[1].Trim();
            var entrypoint = lines.Where(x => x.StartsWith("ENTRYPOINT")).Select(x => x.Split("ENTRYPOINT")[1].Trim()).ToList();
            var runCmds = lines.Where(x => x.StartsWith("RUN")).Select(envVariable => envVariable.Split("RUN")[1].Trim()).ToList();

            try
            {
                var response = this.DockerService.DockerClient.Containers.CreateContainerAsync(
                    new CreateContainerParameters
                    {
                        Image = image,
                        AttachStderr = true,
                        AttachStdin = true,
                        AttachStdout = true,
                        Env = envVariables,
                        WorkingDir = workingdir,
                        //Entrypoint = entrypoint,
                        Cmd = runCmds,
                        Name = containerName,
                        //User = maintainer,
                    }).Result;

                this.DockerService.DockerClient.Containers.StartContainerAsync(response.ID, new HostConfig { }).Wait();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                this.ViewBag.Message = e.Message;
            }

            this.ViewBag.Message = "Done!";
        }

        public IActionResult About()
        {
            //ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult UploadLicence(UserConfiguration config)
        {
            this.UploadFile(config);

            return this.RedirectToAction("Index", config);
            //return View("Index", config);
        }

        [HttpPost]
        public IActionResult DownloadDockerfiles(UserConfiguration config)
        {
            var hs = new HomeControllerService();

            this.UploadFile(config);

            hs.CreateGamsDockerfile(config);
            //hs.CreateModelDockerfile(config);

            var dlFile = hs.CreateDockerZipFile();

            return hs.DownloadFile(dlFile, "dockerfile.zip");
        }

        [HttpPost]
        public IActionResult DownloadGeonodeFile(string id)
        {
            var hs = new HomeControllerService();
            return hs.DownloadFile($@"http://localhost:8011/documents/{id}/download", "geonode.zip");
        }

        [HttpPost]
        public async Task<IActionResult> RunScript(UserConfiguration config)
        {
            this.CreateModelContainer(config);

            // https://stackoverflow.com/questions/43387693/build-docker-in-asp-net-core-no-such-file-or-directory-error
            // https://stackoverflow.com/questions/2849341/there-is-no-viewdata-item-of-type-ienumerableselectlistitem-that-has-the-key

            if (!this.ModelState.IsValid)
            {
                return this.View("Index", config);
            }

            this.UploadFile(config);

            var hs = new HomeControllerService();
            // Todo: change x64 -> parse from program version of form
            hs.CreateGamsDockerfile(config);
            //hs.CreateModelDockerfile(config);

            // docker compose yml

            // build docker image of program from dockerfile
            const string programDockerfile = "./Output/gams-dockerfile";

            var fullpath = Path.GetFullPath(programDockerfile);

            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "/bin/bash",
                Arguments = $@"docker build -f {fullpath} .",
                RedirectStandardOutput = true
            };

            process.StartInfo = startInfo;
            process.Start(); // no such file or directory

            Debug.WriteLine(process.StandardOutput.ReadToEnd());

            // build docker image of model

            return this.View("Index", config);
        }

        public async void UploadFile(UserConfiguration config)
        {
            //https://stackoverflow.com/questions/35379309/how-to-upload-files-in-asp-net-core
            if (config.File != null)
            {
                var uploads = Path.Combine(this.hostingEnvironment.WebRootPath, "uploads");

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                config.FileName = "licence";//config.File.FileName;
                var filePath = Path.Combine(uploads, config.FileName);

                if (config.File.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await config.File.CopyToAsync(stream);
                    }
                }
            }
        }

        //public async Task<IList<ContainerListResponse>> GetDockerContainers()
        //{
        //    // https://docs.docker.com/docker-for-windows/faqs/#how-do-i-connect-to-the-remote-docker-engine-api
        //    // https://github.com/stefanprodan/dockerdash
        // // https://medium.com/lucjuggery/about-var-run-docker-sock-3bfd276e12fd

        //    DockerClient client2 = new DockerClientConfiguration(new Uri("tcp://localhost:2375"))
        //        .CreateClient();

        //    return await client2.Containers.ListContainersAsync(
        //        new ContainersListParameters()
        //        {
        //            Limit = 10,
        //        });

        //    //var images2 = client2.Images.ListImagesAsync(new ImagesListParameters() { All = true }, CancellationToken.None);
        //}

        public IActionResult BuildImage(UserConfiguration config, string imageName = "")
        {
            // https://docs.docker.com/engine/reference/builder/

            HomeControllerService hs = new HomeControllerService();

            hs.CreateGamsDockerfile(config);

            if (imageName == string.Empty)
            {
                imageName = config.SelectedProgram + config.SelectedGithubRepository +
                            DateTime.Now.ToShortDateString().Replace(".", string.Empty);
            }

            imageName = imageName.ToLower() + Guid.NewGuid().ToString().Substring(0, 4);

            var outputPath = Path.GetFullPath("./Output/");

            var files = Directory.GetFiles(outputPath);

            var dockerfile = Path.GetFileName(files.FirstOrDefault(file => file.ToLower().Contains("docker")));

            if (!string.IsNullOrEmpty(dockerfile))
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $@"docker build -t webinterface/{imageName} - < /app/Output/{dockerfile}",
                    //Arguments = $@"docker build -t test/{imageName} Dockerfile-model",
                    RedirectStandardOutput = true
                };
                
                process.StartInfo = startInfo;
                process.Start(); // no such file or directory

                //process.OutputDataReceived += this.Process_OutputDataReceived;

                Debug.WriteLine(process.StandardOutput.ReadToEnd());

                process.WaitForExit();
            }

            return this.View("Index", config);

            // Todo: Get user info: dockerfile not available
        }

        public async Task<UserConfiguration> GetUserConfig(UserConfiguration userConfig, string programRepo = "gams-docker")
        {
            if (userConfig == null)
            {
                userConfig = new UserConfiguration();
            }

            var homeControllerService = new HomeControllerService();

            if (userConfig.GeoNodeDocuments.Count == 0)
            {
                var geoNodeDocuments = await homeControllerService.GetGeonodeData();
                if (geoNodeDocuments != null)
                {
                    userConfig.GeoNodeDocuments = geoNodeDocuments.Documents.Select(document => new SelectListItem
                    {
                        Value = document.Id.ToString(),
                        Text = document.Title
                    })
                        .ToList();
                }

                if (string.IsNullOrEmpty(userConfig.SelectedGeoNodeDocument))
                {
                    userConfig.SelectedGeoNodeDocument = geoNodeDocuments?.Documents.First().Title;
                }

                var geoNodeDocumentData = await homeControllerService.GetGeoNodeDocumentData(userConfig.SelectedGeoNodeDocument);
                if (geoNodeDocumentData != null)
                {
                    userConfig.GeonodeModelTags = geoNodeDocumentData.Keywords.Select(keyword => new SelectListItem
                        {
                            Value = keyword.ToString(),
                            Text = keyword.ToString()
                        }
                    ).ToList();
                }
            }

            // https://stackoverflow.com/questions/28781345/listing-all-repositories-using-github-c-sharp
            var repositories = await homeControllerService.GetGithubRepositories(userConfig.GitHubUser);
            if (repositories != null)
            {
                userConfig.GithubRepositories = repositories.Select(repository => new SelectListItem
                {
                    Value = repository.Name,
                    Text = repository.Name
                })
                    .ToList();

                if (string.IsNullOrEmpty(userConfig.SelectedGithubRepository))
                {
                    userConfig.SelectedGithubRepository = repositories.First().Name;
                }

                var repositoryVersions =
                    await homeControllerService.GetGithubRepoVersions(userConfig.GitHubUser,
                        userConfig.SelectedGithubRepository);

                var repositoryBranches =
                    await homeControllerService.GetGithubBranches(userConfig.GitHubUser,
                        userConfig.SelectedGithubRepository);

                if (repositoryBranches != null)
                {
                    userConfig.GithubRepositoryBranches = repositoryBranches.Select(branch => new SelectListItem
                        {
                            Value = branch.Name, //+ " " + "GithubBranch",
                            Text = branch.Name,
                        })
                        .ToList();
                }

                if (repositoryVersions != null)
                {
                    userConfig.GithubRepositoryVersions.Clear();
                    userConfig.GithubRepositoryVersions.AddRange(userConfig.GithubRepositoryBranches);
                    userConfig.GithubRepositoryVersions.AddRange(repositoryVersions.Select(version => new SelectListItem
                        {
                            Value = version.Url.Substring(version.Url.LastIndexOf('/') + 1),
                            Text = version.Url.Substring(version.Url.LastIndexOf('/') + 1)
                        })
                        .ToList());
                }
            }

            var programs = await homeControllerService.GetDockerhubRepositories(userConfig.DockerhubUser);

            userConfig.Programs = programs.Results.OrderByDescending(x => x.Name).Select(program => new SelectListItem
            {
                Value = program.Name,
                Text = program.Name
            })
                .ToList();

            var programVersions = await homeControllerService.GetDockerhubRepositoryTags(userConfig.DockerhubUser,
                userConfig.DockerhubProgramRepository);

            if (programVersions == null)
            {
                return userConfig;
            }

            userConfig.ProgramVersions = programVersions.Results.OrderByDescending(x => x.Name).Select(version => new SelectListItem
            {
                Value = version.Name,
                Text = version.Name + " [" + version.Images.First().Architecture + "]"
            })
            .ToList();

            return userConfig;
        }
    }
}
