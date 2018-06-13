//#define testrun
//#define exchangeEnvironmentVariables

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
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading;
    using WebInterface.Classes;
    using Docker.DotNet;
    using Docker.DotNet.Models;

    public class HomeController : Controller
    {
        public static DockerClient Client;

        private readonly IHostingEnvironment hostingEnvironment;

        public string OutputPath { get; set; }

        public HomeController(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }

        public List<GithubRepository> GithubRepositories { get; set; }

        public async Task<IActionResult> Index()
        {
            var userConfig = await this.GetUserConfig(new UserConfiguration());

            if (userConfig.GithubRepositories.Count == 0)
            {
                this.SetDefaultConfig(userConfig);
            }

            //https://github.com/Microsoft/Docker.DotNet/blob/master/README.md

            if (Client != null)
            {
                return this.View(userConfig);
            }

            var dockerAddress = Environment.GetEnvironmentVariable("DOCKER_REMOTE_API");
            Client = new DockerClientConfiguration(new Uri(dockerAddress))
                .CreateClient();

            return this.View(userConfig);
        }

        public void SetDefaultConfig(UserConfiguration config)
        {
            config.GithubRepositories.Add(new SelectListItem { Text = "transport-model", Value = "transport-model" });
            config.GithubRepositoryVersions.Add(new SelectListItem { Text = "v1.0", Value = "v1.0" });
            config.GithubRepositoryVersions.Add(new SelectListItem { Text = "v2.0", Value = "v2.0" });
            config.GithubRepositoryVersions.Add(new SelectListItem { Text = "v3.0", Value = "v3.0" });
        }

        public ActionResult Messages()
        {
            return this.PartialView("Messages", GlobalData.Messages);
        }

        public async Task<CreateContainerResponse> CreateDockerModelContainer(UserConfiguration config)
        {
            // https://github.com/Microsoft/Docker.DotNet/issues/134
            // https://github.com/Microsoft/Docker.DotNet/issues/270
            // https://github.com/Microsoft/Docker.DotNet/issues/212

            // TODO: Share network (docker build --network geonode_default -t test_iiasagams .)


            string fileContent;

#if testrun
            // Read Dockerfile Content
            using (var reader = new StreamReader(@"./Output/test/Dockerfile/Dockerfile"))
            {
                fileContent = reader.ReadToEnd();
            }
#else
            // Read Dockerfile Content
            using (var reader = new StreamReader(Path.Combine(HomeControllerService.DockerFilePath,
                    HomeControllerService.DockerFileName)))
            {
                fileContent = reader.ReadToEnd();
            }
#endif

            // Parse Dockerfile Content
            var lines = Regex.Split(fileContent, "\r\n|\r|\n").ToList();

            var image = lines.First(x => x.StartsWith("FROM")).Split("FROM")[1].Trim();
            var parent = image.Split(":")[0].Trim();
            var tag = image.Split(":")[1].Trim();

            var report = new Progress<JSONMessage>(msg =>
            {
                var message = $"{msg.Status}|{msg.ProgressMessage}|{msg.ErrorMessage}";

                Logger.Log(message);
            });

            // Pull Dockerfile Reference image
            await Client.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = image,
            }, new AuthConfig(), report
            );

            //new ImageLoadParameters() {Parent = parent, Tag = tag}, new AuthConfig());

            // Get stream output
            //using (var reader = new StreamReader(statusUpdate))
            //{
            //    string line;
            //    while ((line = reader.ReadLine()) != null)
            //    {
            //        Logger.Log(line);
            //    }
            //}

            var containerName = $"c_{image.Split(":")[0].Replace("/", "_")}";
            var containers =
                await Client.Containers.ListContainersAsync(new ContainersListParameters(),
                    CancellationToken.None);

            if (containers.ToList().Exists(x => x.Names.Contains(containerName)))
            {
                var container = containers.FirstOrDefault(x => x.Names.Contains(containerName));
                if (container != null)
                {
                    await Client.Containers.RemoveContainerAsync(container.ID,
                        new ContainerRemoveParameters { Force = true });
                }
            }

            //var maintainer = lines.First(x => x.StartsWith("MAINTAINER")).Split("MAINTAINER")[1].Trim();
            var envVariables = lines.Where(x => x.StartsWith("ENV"))
                .Select(envVariable => envVariable.Split("ENV")[1].Trim()).ToList();
            var workingdir = lines.FirstOrDefault(x => x.StartsWith("WORKDIR"))?.Split("WORKDIR")[1].Trim();

            // Brackets regex
            //var pattern = @"^(\[){1}(.*?)(\]){1}$";

            // Get entrypoint line & remove brackets
            var entrypoint = lines.FirstOrDefault(x =>
                    x.StartsWith("ENTRYPOINT"))
                ?.Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("\"", string.Empty)
                .Replace(",", string.Empty)
                .Split("ENTRYPOINT")[1]
                .Trim()
                .Split(" ")
                .ToList();

            const string pattern = @"([""'])(?:(?=(\\?))\2.)*?\1";
            var first = lines.FirstOrDefault(x => x.StartsWith("ENTRYPOINT"));
            if (first != null)
            {
                entrypoint = Regex.Matches(first, pattern)
                    .Select(match => match.Value.Replace("\"", string.Empty)).ToList();
            }

            // Todo: add copy licence

            // https://docs.docker.com/v17.09/engine/userguide/eng-image/dockerfile_best-practices/#build-cache
            var cmds = new[] { "RUN" }; //, "COPY", "ADD"};

            //var result = lines.Where(l => cmds.All(l.StartsWith)).ToList();

            var linesNew = new List<string>();
            foreach (var l in lines)
            {
                var l1 = l.Replace("[", string.Empty).Replace("]", string.Empty).Replace("\"", string.Empty)
                    .Replace(",", string.Empty);
                linesNew.AddRange(from cmd in cmds where l1.StartsWith(cmd) select l);
            }

            var runCmds = new List<string>();
            foreach (var item in linesNew.Where(x => x.StartsWith("RUN"))
                .Select(envVariable => envVariable.Split("RUN")[1].Trim().Split(" ").ToList()))
            {
                runCmds.AddRange(item);
            }

            var env = envVariables.Select(str => str.Split("="))
                .Select(envSplit => new KeyValuePair<string, string>(envSplit[0], envSplit[1])).ToList();

#if exchangeEnvironmentVariables
            foreach (var envVar in env)
            {
                for (var index = 0; index < runCmds.Count; index++)
                {
                    var command = runCmds[index];

                    if (!command.Contains(envVar.Key))
                    {
                        continue;
                    }

                    runCmds[index] = command.Replace("${" + envVar.Key + "}", envVar.Value);
                    //index = 0; // check from beginning 
                    //i = 0;
                }
            }
#endif

            // Remove license copy-command if the user didn't provide a license
            foreach (var item in env)
            {
                if (item.Key.ToUpper().Contains("LICEN")) // LICENSE or LICENCE
                {
                    if (item.Value.Contains("#LICEN"))
                    {
                        foreach (var cmd in runCmds.ToList())
                        {
                            if (cmd.ToUpper().StartsWith("COPY ${LICEN"))
                            {
                                runCmds.Remove(cmd);
                            }
                        }
                    }
                }
            }

            CreateContainerResponse response = null;

            //entrypoint.AddRange(new List<string> {"-v", "C:Temp/output:/output"});
            var newEntry = new List<string>();
            newEntry.AddRange(runCmds);
            newEntry.AddRange(entrypoint.IsNull() ? new List<string>() : entrypoint);

            // https://stackoverflow.com/questions/42857897/execute-a-script-before-cmd/42858351
            // CMD & ENTRYPOINT

            try
            {
                //https://github.com/Microsoft/Docker.DotNet/issues/212
                // https://docs.docker.com/v17.09/engine/userguide/eng-image/dockerfile_best-practices/#build-cache

                var networks = await Client.Networks.ListNetworksAsync();
                var geonodeNetwork = networks.First(x => x.Name.Contains("geonode"));

                response = Client.Containers.CreateContainerAsync(
                    new CreateContainerParameters
                    {
                        Image = image,
                        AttachStderr = true,
                        AttachStdin = true,
                        AttachStdout = true,
                        Tty = true,
                        NetworkingConfig = new NetworkingConfig
                        {
                            //EndpointsConfig = 
                        },
                        OnBuild = new List<string>(),
                        Env = envVariables,
                        WorkingDir = workingdir,
                        Entrypoint = newEntry, //entrypoint,
                        //Cmd = runCmds,
                        Name = containerName,
                        HostConfig = new HostConfig()
                        //User = maintainer
                    }).Result;

                this.ViewBag.Message = "Done!";
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);

                this.ViewBag.Message = e.Message;

                this.ViewBag.Message = "Error creating container!";
            }

            return response;
        }

        public IActionResult About()
        {
            //ViewData["Message"] = "Your application description page.";

            return this.View();
        }

        public IActionResult Contact()
        {
            return this.View();
        }

        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult UploadLicence(UserConfiguration config)
        {
            this.UploadFile(config);

            return this.RedirectToAction("Index", config);
            //return View("Index", config);
        }

        [HttpPost]
        public IActionResult UploadModelData(UserConfiguration config)
        {
            this.UploadModelInputData(config);

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
            if (!this.ModelState.IsValid)
            {
                return this.View("Index", config);
            }

            var success = await this.RunScriptAsync(config);

            if (success)
            {
                return this.DownloadResult();
            }

            // todo: error
            return this.View("Index", config);
        }

        [HttpPost]
        public IActionResult DownloadResult()
        {
            var hs = new HomeControllerService();

            var downloadPath = $@"./Result/result.tar";

            return System.IO.File.Exists(downloadPath) ? hs.DownloadFile(downloadPath, "result.tar") : this.View("Index");
        }

        public async Task<bool> RunScriptAsync(UserConfiguration config)
        {
            var hs = new HomeControllerService();

            // https://stackoverflow.com/questions/43387693/build-docker-in-asp-net-core-no-such-file-or-directory-error
            // https://stackoverflow.com/questions/2849341/there-is-no-viewdata-item-of-type-ienumerableselectlistitem-that-has-the-key

            // Upload License if provided
            this.UploadFile(config);

            // Upload model input data if provided
            this.UploadModelInputData(config);

            // Generate Dockerfile
            var programDockerfilePath = hs.CreateGamsDockerfile(config);
            var fullpat = Path.GetFullPath(programDockerfilePath);

            // ------- CREATE IMAGE FROM DOCKERFILE
            try
            {
                //// https://github.com/Microsoft/Docker.DotNet/issues/197
                var path = fullpat;

                const string tarFile = "Dockerfile.tar";

                if (!System.IO.File.Exists(tarFile))
                {
                    System.IO.File.Delete(tarFile);
                }

                hs.CreateTarGz(tarFile, path);

                Logger.Log("\n\n\n\n======== DOCKERFILE START =======================================");
                using (var sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Logger.Log(line);
                    }
                }
                Logger.Log("========= DOCKERFILE END ======================================\n\n\n\n\n");

                //hs.DownloadZipDataToFolder("http://geonode_geonode_1/documents/3/download/", @"./OutputZip/data.zip");

                var networks = await Client.Networks.ListNetworksAsync();
                var network = networks.First(x => x.Name.Contains("webinterface_default")); //networks.First(x => x.Name.Contains("geonode"));

                var imageName = "gams/iiasa";
                var tag = "latest";

                // https://docs.docker.com/edge/engine/reference/commandline/build/
                // https://docs.docker.com/engine/api/v1.25/#operation/ImageList
                var imageBuildParameters = new ImageBuildParameters()
                {
                    Remove = true,
                    ForceRemove = true,
                    Tags = new List<string> { imageName + ":" + tag },
                    NetworkMode = network.Name,
                    NoCache = true
                };

                var errorDetected = false;
                using (var fs = new FileStream(tarFile, FileMode.Open))
                {
                    // https://stackoverflow.com/questions/33997089/how-can-i-create-a-stream-for-dockerdotnet-buildimagefromdockerfile-method
                    var statusUpdate = await Client.Images.BuildImageFromDockerfileAsync(
                        fs,
                        imageBuildParameters,
                        CancellationToken.None);

                    using (var streamReader = new StreamReader(statusUpdate))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            Logger.Log(line);
                            if (line.ToLower().Contains("error"))
                            {
                                errorDetected = true;
                            }
                        }
                    }

                    fs.Dispose();
                }

                if (errorDetected)
                {
                    Logger.Log("!!! ERRORS DETECTED!!!\nContainer Creation Aborted!");
                    return false;
                }

                var containerName = imageName.Replace("/", "_") + "_container";

                var containers =
                    await Client.Containers.ListContainersAsync(new ContainersListParameters
                    {
                        All = true,
                    },
                        CancellationToken.None);

                var containerList = containers.ToList();

                foreach (var container in containerList)
                {
                    foreach (var name in container.Names)
                    {
                        if (name.Contains(containerName))
                        {
                            await Client.Containers.RemoveContainerAsync(container.ID,
                                new ContainerRemoveParameters { Force = true });
                        }
                    }
                }

                var containerResponse = await Client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    AttachStderr = true,
                    AttachStdin = true,
                    AttachStdout = true,
                    Image = imageName,
                    Name = containerName,
                },
                CancellationToken.None);

                var res = await Client.Containers.StartContainerAsync(containerResponse.ID, new ContainerStartParameters(),
                    CancellationToken.None);

                if (res)
                {
                    Logger.Log("=== Container Created and Started ===");

                    var outputResponse = await Client.Containers.GetArchiveFromContainerAsync(containerResponse.ID,
                        new GetArchiveFromContainerParameters
                        {
                            Path = "/output"
                        },
                        false,
                        CancellationToken.None);

                    using (Stream s = System.IO.File.Create("./Result/result.tar"))
                    {
                        outputResponse.Stream.CopyTo(s);
                    }

                    Logger.Log("output finished!");

                    return true;
                }

                Logger.Log("ERROR\n=== Container Could Not Be Created! ===");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(
                    "=================== ERROR ===========================================================================");
                Logger.Log(ex.Message); //ex.FullMessage());

                if (ex.InnerException != null)
                {
                    Logger.Log(ex.InnerException.Message);
                }

                Logger.Log(
                    "=====================================================================================================");

                return false;
            }
        }

        public async void UploadFile(UserConfiguration config)
        {
            //https://stackoverflow.com/questions/35379309/how-to-upload-files-in-asp-net-core
            if (config.File == null)
            {
                return;
            }

            var uploads = Path.Combine(this.hostingEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            const string filename = "gamslice.txt"; //"licence";//config.File.FileName;
            var filePath = Path.Combine(uploads, filename);

            config.FileName = "http://webinterface/uploads/model_input_data/" + filename;
            config.LicencePath = config.FileName;
            //Path.Combine(Path.GetDirectoryName(filePath), filename);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            if (config.File.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await config.File.CopyToAsync(stream);
                }
            }
        }

        public async void UploadModelInputData(UserConfiguration config)
        {
            //https://stackoverflow.com/questions/35379309/how-to-upload-files-in-asp-net-core
            if (config.ModelDataFile == null)
            {
                return;
            }

            var uploads = Path.Combine(this.hostingEnvironment.WebRootPath, "uploads/model_input_data");

            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            const string filename = "modelInputData.zip"; //"licence";//config.File.FileName;
            var filePath = Path.Combine(uploads, filename);

            config.ModelInputDataFile = "http://webinterface/uploads/model_input_data/" + filename;

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            if (config.ModelDataFile.Length > 0)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await config.ModelDataFile.CopyToAsync(stream);
                }
            }
        }

        public IActionResult BuildImage(UserConfiguration config, string imageName = "")
        {
            // https://docs.docker.com/engine/reference/builder/

            var hs = new HomeControllerService();

            hs.CreateGamsDockerfile(config);

            if (imageName == string.Empty)
            {
                imageName = config.SelectedProgram + config.SelectedGithubRepository +
                            DateTime.Now.ToShortDateString().Replace(".", string.Empty);
            }

            imageName = imageName.ToLower() + Guid.NewGuid().ToString().Substring(0, 4);

            var outputPath = Path.GetFullPath(HomeControllerService.OutputFilePath); //"./Output/");

            var files = Directory.GetFiles(outputPath);

            var dockerfile = Path.GetFileName(files.FirstOrDefault(file => file.ToLower().Contains("docker")));

            if (!string.IsNullOrEmpty(dockerfile))
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    //WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments =
                        $@"docker build -t webinterface/{imageName} - < /app/Output/{
                                HomeControllerService.OutputFolderName
                            }/{dockerfile}",
                    //Arguments = $@"docker build -t test/{imageName} Dockerfile-model",
                    RedirectStandardOutput = true
                };

                process.StartInfo = startInfo;
                process.Start(); // no such file or directory

                Logger.Log(process.StandardOutput.ReadToEnd());

                process.WaitForExit();
            }

            return this.View("Index", config);

            // Todo: Get user info: dockerfile not available
        }

        public async Task<UserConfiguration> GetUserConfig(UserConfiguration userConfig,
            string programRepo = "gams-docker")
        {
            if (userConfig == null)
            {
                userConfig = new UserConfiguration();
            }

            var homeControllerService = new HomeControllerService();

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
                        Value = branch.Name,
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

            userConfig.ProgramVersions = programVersions.Results.OrderByDescending(x => x.Name).Select(version =>
                    new SelectListItem
                    {
                        Value = version.Name,
                        Text = version.Name + " [" + version.Images.First().Architecture + "]"
                    })
                .ToList();

            return userConfig;
        }
    }
}