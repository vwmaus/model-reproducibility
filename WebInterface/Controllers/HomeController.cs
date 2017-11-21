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
            var repositories = new List<string>();

            // Github Repositories
            // https://stackoverflow.com/questions/28781345/listing-all-repositories-using-github-c-sharp

            //Task.Factory.StartNew(async () =>
            //{
            //await this.GetGithubRepositoriesAsync("vwmaus");
            var x = await this.GetGeonodeData();
            var repos = GetGithubRepositories("vwmaus");


            //repositories = this.GithubRepositories.Select(repo => repo.Name).ToList();
            //});

            //t.ContinueWith(task =>
            //{
            //    var x = task.Result;
            //    var y = t.Result;

            //});

            //t.ContinueWith(task =>
            //    {
            //        var repos = t.Result;

            //        // Repositories = Models
            //        if (repos != null)
            //        {
            //            //ViewBag.selectList = repos.
            //        }
            //    }
            //);
            var l = repositories.Select(item => new SelectListItem
            {
                Value = item,
                Text = item
            })
                .ToList();

            this.ViewBag.selectList = l.ToAsyncEnumerable();

            Debug.Write(x);
            Debug.Write(repos);

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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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

            if (!ModelState.IsValid)
            {
                return View("Index");
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


            return View("Index");
        }

        public async Task<GeoNodeDocument> GetGeonodeData()
        {
            var req = WebRequest.Create(@"http://geonode_geonode_1/api/documents/");
            var response = await req.GetResponseAsync().ConfigureAwait(false);

            var responseReader = new StreamReader(response.GetResponseStream());
            var responseData = await responseReader.ReadToEndAsync();
            var document = JsonConvert.DeserializeObject<GeoNodeDocument>(responseData);

            return document;
        }

        public async Task<List<GithubRepository>> GetGithubRepositoriesAsync(string user)
        {
            var url = "https://api.github.com/users/" + user + "/repos";
            var req = WebRequest.Create(url);
            var response = await req.GetResponseAsync().ConfigureAwait(false);

            var responseReader = new StreamReader(response.GetResponseStream());
            var responseData = await responseReader.ReadToEndAsync();
            var document = JsonConvert.DeserializeObject<List<GithubRepository>>(responseData);

            return document;
        }

        public List<GithubRepository> GetGithubRepositories(string user)
        {
            HttpWebRequest request = WebRequest.Create("https://api.github.com/users/" + user + "/repos") as HttpWebRequest;
            request.UserAgent = "TestApp";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                var responseData = reader.ReadToEnd();

                //var url = "https://api.github.com/users/" + user + "/repos";
                //var req = WebRequest.Create(url);
                //var response = req.GetResponse();


                //var responseReader = new StreamReader(response.GetResponseStream());
                //var responseData = responseReader.ReadToEnd();
                var document = JsonConvert.DeserializeObject<List<GithubRepository>>(responseData);

                return document;
            }
        }

        public async Task<GithubRepositoryVersion> GetGithubRepoVersions(string user, string repository)
        {
            var url = "https://api.github.com/repos/" + user + "/" + repository + "/git/refs/tags";
            var req = WebRequest.Create(url);
            var response = await req.GetResponseAsync().ConfigureAwait(false);

            var responseReader = new StreamReader(response.GetResponseStream());
            var responseData = await responseReader.ReadToEndAsync();

            var document = JsonConvert.DeserializeObject<GithubRepositoryVersion>(responseData);

            return document;
        }
    }
}
