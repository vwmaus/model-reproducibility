namespace WebInterface.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using WebInterface.Models;
    using WebInterface.Services;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class HomeController : Controller
    {
        public List<GithubRepository> GithubRepositories { get; set; }

        public async Task<IActionResult> Index()
        {
            var userConfig = new UserConfiguration();

            await this.LoadConfigData();

            return this.View(userConfig);
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
        public IActionResult DownloadDockerfiles(UserConfiguration config)
        {
            var hs = new HomeControllerService();
            hs.CreateGamsDockerfile(config.ProgramVersion, config.ProgramArchitecture, config.LicencePath);
            hs.CreateModelDockerfile(config);

            var dlFile = hs.CreateDockerZipFile();

            return hs.DownloadFile(dlFile, "dockerfile.zip");
        }

        [HttpPost]
        public IActionResult RunScript(UserConfiguration config)
        {
            // https://stackoverflow.com/questions/43387693/build-docker-in-asp-net-core-no-such-file-or-directory-error

            if (!this.ModelState.IsValid)
            {
                return this.View("Index");
            }

            var hs = new HomeControllerService();
            hs.CreateGamsDockerfile(config.ProgramVersion, config.ProgramArchitecture, config.LicencePath);
            hs.CreateModelDockerfile(config);

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


            return this.View("Index");
        }

        public async Task LoadConfigData(string user = "vwmaus", string repoName = "")
        {
            var homeControllerService = new HomeControllerService();

            var geoNodeDocuments = await homeControllerService.GetGeonodeData();

            var geonodeDocumentList = new List<SelectListItem>();
            if (geoNodeDocuments != null)
            {
                geonodeDocumentList = geoNodeDocuments.Documents.Select(document => new SelectListItem
                {
                    Value = document.Title,
                    Text = document.Title
                })
                    .ToList();
            }

            // TODO: Get GeoNode Document Tags
            //var geoNodeDocumentTags = 
            //var geonodeDocumentTagList = geoNodeDocumentTags.Select(document => new SelectListItem
            //    {
            //        Value = document.Title,
            //        Text = document.Title
            //    })
            //    .ToList();

            // https://stackoverflow.com/questions/28781345/listing-all-repositories-using-github-c-sharp
            var repositories = await homeControllerService.GetGithubRepositories(user);

            var repoList = new List<SelectListItem>();
            var repositoryVersionList = new List<SelectListItem>();

            if (repositories != null)
            {
                repoList = repositories.Select(repository => new SelectListItem
                {
                    Value = repository.Name,
                    Text = repository.Name
                })
                    .ToList();

                if (string.IsNullOrEmpty(repoName))
                {
                    repoName = repositories.First().Name;
                }

                var repositoryVersions = await homeControllerService.GetGithubRepoVersions(user, repoName);

                if (repositoryVersions != null)
                {
                    repositoryVersionList = repositoryVersions.Select(version => new SelectListItem
                    {
                        Value = version.Url.Substring(version.Url.LastIndexOf('/') + 1),
                        Text = version.Url.Substring(version.Url.LastIndexOf('/') + 1)
                    })
                        .ToList();
                }
            }

            this.ViewBag.repositories = repoList.ToAsyncEnumerable();
            this.ViewBag.geonodeDocuments = geonodeDocumentList.ToAsyncEnumerable();
            this.ViewBag.repositoryVersionList = repositoryVersionList.ToAsyncEnumerable();
            this.ViewBag.geonodeDocumentTags = new List<SelectListItem>().ToAsyncEnumerable();
        }
    }
}
