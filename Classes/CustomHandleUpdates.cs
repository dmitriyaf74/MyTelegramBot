using MyTelegramBot.DapperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTelegramBot.Classes
{
    public delegate void ProcShowMessage(string message);

    internal class CustomHandleUpdates
    {
        public static string ConnectionString;
        public static CustomQuery Query;
        public static ProcShowMessage procShowMessage { get; set; }

        protected static void DoConShowMessage(string message)
        {
            if (procShowMessage is not null)
                procShowMessage(message);
        }
    }
}
