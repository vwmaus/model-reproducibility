using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Classes
{
    public static class Extensions
    {
        public static bool IsNull(this object o)
        {
            return o == null;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static string GetUniqueName(this string fileName)
        {
            if (File.Exists(fileName))
            {
                fileName = Path.GetFileName(fileName);

                return Path.GetFileNameWithoutExtension(fileName)
                       + "_"
                       + Guid.NewGuid().ToString().Substring(0, 4)
                       + Path.GetExtension(fileName);
            }

            return fileName + "_"
                   + Guid.NewGuid().ToString().Substring(0, 4);
        }

        public static string FullMessage(this Exception ex)
        {
            var message = string.Empty;

            message += ex.Message + "\n";

            if (ex.InnerException != null)
            {
                message += ex.InnerException.Message;
            }

            return message;
        }
    }
}
