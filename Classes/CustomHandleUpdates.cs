using MyTelegramBot.DapperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace MyTelegramBot.Classes
{
    public delegate void ProcShowMessage(string message);
    public delegate Task UpdateReceivedDelegate(ITelegramBotClient botClient, Update update, CancellationToken token);
    public delegate bool UpdateCallBackDelegate(ITelegramBotClient botClient, Update update, CancellationToken token);

    internal class CustomHandleUpdates
    {
        public CustomQuery? Query;
        public ProcShowMessage? procShowMessage { get; set; }

        protected void DoConShowMessage(string message)
        {
            if (procShowMessage is not null)
                procShowMessage(message);
        }
        public virtual void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        { 
        }

        public virtual void RegisterCallBackUpdates(ref UpdateCallBackDelegate? updCallBackDelegate)
        { 
        }


    }
}
