using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Classes
{
    using System.Diagnostics;
    using WebInterface.Models;

    public static class Logger
    {
        public static void Log(string message)
        {
            if (message == null)
            {
                return;
            }

            Debug.WriteLine(message);

            GlobalData.Messages.Add(
                new MessageModel
                {
                    Message = message
                }
            );
        }
    }
}
