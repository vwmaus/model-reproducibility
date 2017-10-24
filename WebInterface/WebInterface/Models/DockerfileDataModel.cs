using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Models
{
    public class DockerfileDataModel
    {
        public string LicencePath { get; set; }

        public string Program { get; set; }

        public string ProgramVersion { get; set; }

        public string Model { get; set; }

        public string ModelVersion { get; set; }

        public string GitHubUser { get; set; }
    }
}
