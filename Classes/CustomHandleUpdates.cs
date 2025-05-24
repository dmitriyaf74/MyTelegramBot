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
    public delegate Task DoGetMenuRoleDelegate(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id);

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
        public void RegisterHandlesAll(ref UpdateReceivedDelegate? updRecDelegate,
            ref UpdateCallBackDelegate? updCallBackDelegate,
            ref DoGetMenuRoleDelegate? GetMenuRoleDelegate)
        {
            //////////////////Передавать сюда объект

            updRecDelegate += UpdateReceivedStart;
            updCallBackDelegate += UpdateCallBackKeyboard;
            GetMenuRoleDelegate += DoGetMenuRole;
        }
        protected virtual async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            await Task.Delay(0);
        }
        protected virtual async Task UpdateCallBackKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            await Task.Delay(0);
        }
        protected virtual async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
        {
            await Task.Delay(0);
        }
        

    }
}
