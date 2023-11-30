using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV2.Web
{
    public class TaskResult
    {
        public string Info { get; set; }
        public bool Succeeded { get; set; }

        public TaskResult(bool success, string info)
        {
            Info = info;
            Succeeded = success;
        }

        public TaskResult()
        {
            Succeeded = false;
            Info = "An unknown error occured, or the task was never flagged as successful.";
        }
    }
}