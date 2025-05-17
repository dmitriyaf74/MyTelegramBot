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
    public delegate void ProcShowError(string message);
    public delegate Task UpdateReceivedDelegate(ITelegramBotClient botClient, Update update, CancellationToken token);
    public delegate Task UpdateCallBackDelegate(ITelegramBotClient botClient, Update update, CancellationToken token);

    internal class CustomHandleUpdates
    {

        public ProcShowMessage? procShowMessage { get; set; }

        protected void DoConShowMessage(string message)
        {
            if (procShowMessage is not null)
                procShowMessage(message);
        }
        public ProcShowMessage? procShowError { get; set; }

        protected void DoConShowError(string message)
        {
            if (procShowError is not null)
                procShowError(message);
        }
        public virtual void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        { 
        }

        public virtual void RegisterCallBackUpdates(ref UpdateCallBackDelegate? updCallBackDelegate)
        { 
        }


    }
}
