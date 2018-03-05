#define testrun
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

            //https://github.com/Microsoft/Docker.DotNet/blob/master/README.md

            if (Client == null)
            {
                var dockerAddress = Environment.GetEnvironmentVariable("DOCKER_REMOTE_API");
                Client = new DockerClientConfiguration(new Uri(dockerAddress))
                    .CreateClient();

                //var report = new Progress<JSONMessage>(msg =>
                //{
                //    Debug.WriteLine($"{msg.Status}|{msg.ProgressMessage}|{msg.ErrorMessage}");
                //});

                //await Client.System.MonitorEventsAsync(new ContainerEventsParameters(), report, CancellationToken.None);
            }

            //var info = await Client.System.GetSystemInfoAsync(CancellationToken.None);
            //Debug.WriteLine(info);

            //var c = await Client.Containers.ListContainersAsync(new ContainersListParameters(), CancellationToken.None);

            //var list = await this.DockerService.GetContainerList();

            //var icp = new ImagesCreateParameters();
            //var res = await this.DockerService.DockerClient.Images.CreateImageAsync(icp, new AuthConfig() { });

            return this.View(userConfig);
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
                Debug.WriteLine($"{msg.Status}|{msg.ProgressMessage}|{msg.ErrorMessage}");
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
            //        Debug.WriteLine(line);
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

            var pattern = @"([""'])(?:(?=(\\?))\2.)*?\1";
            string first = lines.FirstOrDefault(x => x.StartsWith("ENTRYPOINT"));
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
                Debug.WriteLine(e.Message);
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

            await this.RunScriptAsync(config);

            return this.View("Index", config);
        }

        public async Task RunScriptAsync(UserConfiguration config)
        {
            var hs = new HomeControllerService();

            // https://stackoverflow.com/questions/43387693/build-docker-in-asp-net-core-no-such-file-or-directory-error
            // https://stackoverflow.com/questions/2849341/there-is-no-viewdata-item-of-type-ienumerableselectlistitem-that-has-the-key

            this.UploadFile(config);

            // Todo: change x64 -> parse from program version of form
            var programDockerfilePath = hs.CreateGamsDockerfile(config);
            var fullpat = Path.GetFullPath(programDockerfilePath);
            //hs.CreateModelDockerfile(config);

            // docker compose yml

            // build docker image of program from dockerfile
            var programDockerfile =
                $@"{HomeControllerService.OutputFilePath}/gams-dockerfile"; //"./Output/gams-dockerfile";

            var fullpath = Path.GetFullPath(programDockerfile);

            // ------- CREATE IMAGE FROM DOCKERFILE

            try
            {
                //// https://github.com/Microsoft/Docker.DotNet/issues/197

                const string path = @"./Output/test/DockerfileOutput/Dockerfile";

                const string pathTar = @"./OutputTar/Dockerfile.tar";

                var tar = Path.GetFullPath(pathTar);
                var dockpath = Path.GetFullPath(path);

                if (!System.IO.File.Exists(pathTar))
                {
                    System.IO.File.Delete(pathTar);
                }

                hs.CreateTarGz(pathTar, path);

                //hs.CreateTarGz("./Output/test/Dockerfile/Dockerfile.tar", dockpath);

                using (var sr = new StreamReader(path))
                {
                    var content = sr.ReadToEnd();
                    Debug.WriteLine("======== DOCKERFILE START ======");
                    Debug.WriteLine("Dockerfile Content: \n" + content);
                    Debug.WriteLine("========= DOCKERFILE END =======");
                }

                var networks = await Client.Networks.ListNetworksAsync();
                var geonodeNetwork = networks.First(x => x.Name.Contains("geonode"));

                var imageBuildParameters = new ImageBuildParameters()
                {
                    Remove = true,
                    ForceRemove = true,
                    Tags = new List<string> { $@"gams/iiasa:latest"}, //{DateTime.Now.ToShortDateString()}" },
                    BuildArgs = new Dictionary<string, string> { { "--network", "geonode_default" }},//geonodeNetwork.Name } }
                    //Dockerfile = @"./Output/test/Dockerfile/Dockerfile"
                };

                using (var fs = new FileStream(pathTar, FileMode.Open))
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
                            Debug.WriteLine(line);
                        }
                    }

                    fs.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "=================== ERROR ===========================================================================");
                Debug.WriteLine(ex.Message); //ex.FullMessage());

                if (ex.InnerException != null)
                {
                    Debug.WriteLine(ex.InnerException.Message);
                }

                Debug.WriteLine(
                    "=====================================================================================================");
            }

            /*
            // Create Model Container using the docker dotnet service
            var response = await this.CreateDockerModelContainer(config);

            if (response != null)
            {
                //// https://github.com/Microsoft/Docker.DotNet/issues/212

                // https://github.com/Microsoft/Docker.DotNet/issues/184
                //await this.DockerService.DockerClient.Networks.ConnectNetworkAsync(geonodeNetwork.ID, new NetworkConnectParameters {Container = "c_iiasa_gams"});

                try
                {
                    //Debug.WriteLine("-------------------- START CONTAINER ---------------------");
                    //var containerStarted =
                    //    await client.Containers.StartContainerAsync(response.ID,
                    //        new HostConfig { });

                    // strange exit status code behavior #3379
                    // https://github.com/docker/compose/issues/3379
                    //

                    // docker logs c_iiasa_gams
                    // --> get output of container

                    //https://github.com/Microsoft/Docker.DotNet/issues/100
                    /*
                    var log = await this.DockerService.DockerClient.Containers.GetContainerLogsAsync(response.ID,
                        new ContainerLogsParameters { ShowStderr = true, ShowStdout = true, Timestamps = true }, CancellationToken.None);

                    // Get stream output
                    using (var reader = new StreamReader(log))
                    {
                        string line;
                        Debug.WriteLine("==== LOG ======================");
                        while ((line = reader.ReadLine()) != null)
                        {

                            Debug.WriteLine(line);
                        }
                        Debug.WriteLine("===============================");
                    }
                    */


            //if (containerStarted)
            //{
            //    Debug.WriteLine("Container Started!!!");

            //    // todo: copy output
            //    // docker cp d07f55ec3f0f:/ output C:/ temp
            //}

            // Wait for container to be stopped
            /*
            var containerInspectStats = await this.DockerService.DockerClient.Containers.InspectContainerAsync(response.ID);
            while (string.IsNullOrEmpty(containerInspectStats.State.FinishedAt))
            {
                Thread.Sleep(1000);
                containerInspectStats = await this.DockerService.DockerClient.Containers.InspectContainerAsync(response.ID);
            }

            //var container = this.DockerService.GetContainerList().Result.FindAll(x => x.Name.Equals("iiasa_gams")).First();

            //var x = await this.DockerService.DockerClient.Containers.GetArchiveFromContainerAsync(
            //    response.ID,
            //    new GetArchiveFromContainerParameters
            //    {
            //        Path = "/output/output.gdx"
            //    }, 
            //    false, 
            //    CancellationToken.None);

            //GetArchiveFromContainerResponse result;
            ////while (true)
            ////{
            //    //var resultA = client.System.GetVersionAsync().Result;
            //    result = this.DockerService.DockerClient.Containers.GetArchiveFromContainerAsync(
            //        response.ID,
            //        new GetArchiveFromContainerParameters() { Path = "/output/output.gdx" },
            //        false,
            //        CancellationToken.None).Result;

            //    result.Stream.Dispose();
            //    Thread.Sleep(1);
            ////}


            //containerStarted =
            //    await this.DockerService.DockerClient.Containers.StartContainerAsync(response.ID,
            //        new HostConfig { });

            //https://stackoverflow.com/questions/22907231/copying-files-from-host-to-docker-container

            //var stream =
            //    await this.DockerService.DockerClient.Containers.GetContainerStatsAsync(response.ID,
            //        new ContainerStatsParameters(),
            //        CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                "=================== ERROR ===========================================================================");
            Debug.WriteLine(ex.Message); //ex.FullMessage());

            if (ex.InnerException != null)
            {
                Debug.WriteLine(ex.InnerException.Message);
            }

            Debug.WriteLine(
                "=====================================================================================================");
        }

        }
                */
        }

        public bool CheckContainerRun()
        {
            return true;
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

                config.FileName = "gamslice.txt"; //"licence";//config.File.FileName;
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

                //process.OutputDataReceived += this.Process_OutputDataReceived;

                Debug.WriteLine(process.StandardOutput.ReadToEnd());

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

                var geoNodeDocumentData =
                    await homeControllerService.GetGeoNodeDocumentData(userConfig.SelectedGeoNodeDocument);
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