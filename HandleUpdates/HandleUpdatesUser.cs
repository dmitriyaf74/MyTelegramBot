using MyTelegramBot.DapperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using MyTelegramBot.Classes;
using static System.Net.Mime.MediaTypeNames;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesUser : CustomHandleUpdates
    {
        //static List<CustomRole> RoleList;

        public override void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        {
            //updRecDelegate += HandleUpdateAsyncTest;

            //updRecDelegate += UpdateReceivedStart;
            //updRecDelegate += UpdateReceivedRole;
        }

        public override void RegisterCallBackUpdates(ref UpdateCallBackDelegate? updCallBackDelegate)
        {
            //updCallBackDelegate += UpdateReceivedBegin;

            //updCallBackDelegate += UpdateCallBaclHideKeyboard;
        }



        





    }
}
