using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Models;
using WebInterface.Services;

namespace WebInterface.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
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

        public IActionResult DownloadDockerfile(string licence) //, string model, string modelversion)
        {
            var hs = new HomeControllerService();
            hs.CreateGamsDockerfile(licence);
            //hs.CreateModelDockerfile(model, modelversion);

            return RedirectToAction("DownloadGamsDockerfile");
        }

        public IActionResult DownloadGamsDockerfile()
        {
            //https://stackoverflow.com/questions/35237863/download-file-using-mvc-core

            var filename = "gams-dockerfile";
            var filepath = "./Output/" + filename;
            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
            return File(fileBytes, "application/x-msdownload", filename);
        }
    }
}
