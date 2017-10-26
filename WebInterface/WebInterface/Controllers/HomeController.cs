using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Classes;
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

        [HttpPost]
        public IActionResult DownloadDockerfiles(UserConfiguration config) //, string model, string modelversion)
        {
            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            var hs = new HomeControllerService();
            hs.CreateGamsDockerfile(config.LicencePath);

            if (!config.Model.IsNull() && !config.ModelVersion.IsNull())
            {
                hs.CreateModelDockerfile(config);
                //return DownloadGamsDockerfile();
                return DownloadDockerFiles();
            }

            return View("Index");
        }

        public IActionResult DownloadGamsDockerfile()
        {
            // https://stackoverflow.com/questions/317315/asp-net-mvc-relative-paths
            //https://stackoverflow.com/questions/35237863/download-file-using-mvc-core

            var filename = "gams-dockerfile";
            var filepath = "./Output/" + filename;
            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);

            return File(fileBytes, "application/x-msdownload", filename);
        }

        public IActionResult DownloadDockerFiles()
        {
            var files = Directory.GetFiles("./Output/");

            //var hs = new HomeControllerService();
            //hs.CreateZipFile(files);

            //var filesWithoutExtension = System.IO.Directory.GetFiles(@"D:\temp\").Where(filPath => String.IsNullOrEmpty(System.IO.Path.GetExtension(filPath)));
            //var filename = "dockerfiles.zip";
            //var filepath = "./Output/" + filename;
            //byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);

            //return File(fileBytes, "application/x-msdownload", filename);

            return CreateZipFile(files);
        }

        [HttpPost]
        public IActionResult RunScript(UserConfiguration config)
        {
            if (ModelState.IsValid)
            {
            }

            return View("Index");
        }

        public IActionResult CreateZipFile(string[] files, string outputPath = "./Output/")
        {
            const string zipTempPath = "./Output/ZipTemp/";

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var newFilePath = Path.Combine(zipTempPath, fileName);

                if (System.IO.File.Exists(newFilePath))
                {
                    System.IO.File.Delete(newFilePath);
                }

                System.IO.File.Move(file, newFilePath);
            }

            var outputFile = Path.Combine(outputPath + "dockerfiles.zip");

            if (System.IO.File.Exists(outputFile))
            {
                System.IO.File.Delete(outputFile);
            }

            var p = Path.GetFullPath(zipTempPath);
            var y = Path.GetFullPath(outputFile);

            var fs = Directory.GetFiles(p).ToList();

            for (int i = 0; i < 100000000; i++)
            {
                i = i++;
            }

            var fileList = new List<byte[]>();
            foreach (var file in fs)
            {
                var bytes = System.IO.File.ReadAllBytes(file);
                fileList.Add(bytes);
            }

            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        var fileName = Path.GetFileName(fs[i]);
                        var zipArchiveEntry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                        using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(fileList[i], 0, fileList[i].Length);
                    }

                    foreach (var file in Directory.GetFiles(zipTempPath))
                    {
                        System.IO.File.Delete(file);
                    }

                    return File(ms.ToArray(), "application/zip", "Archive.zip");
                }

            }

            //ZipFile.ExtractToDirectory(zipPath, extractPath);
        }

        public void CreateZipfile(string outputPath, params string[] files)
        {
            CreateZipFile(files, outputPath);
        }
    }
}
