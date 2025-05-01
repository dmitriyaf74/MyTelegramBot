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

namespace MyTelegramBot
{
    internal class HandleUpdates : CustomHandleUpdates
    {
        static List<CustomRole> RoleList;

        public static void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        {
            updRecDelegate += UpdateReceivedStart;
            //updRecDelegate += UpdateReceivedStart;
        }

        public static bool UpdateReceivedStart(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            void SelectRole(ITelegramBotClient botClient, Update update, CancellationToken token)
            {
                var buttons = new ReplyKeyboardMarkup();
                buttons.ResizeKeyboard = true;

                var info = "Выберите доступную роль";
                foreach (var item in RoleList)
                {
                    buttons.AddButton(new KeyboardButton($"{item.Id}.{item.Name}"));
                }
                botClient.SendMessage(update.Message.Chat.Id, info, replyMarkup: buttons);
            }


            if (update.Message.Text == "/start")
            {
                var info = $"{update.Message.From.Username}({update.Message.From.Id})({update.Message.From.FirstName} {update.Message.From.LastName}) : {update.Message.Text}";
                DoConShowMessage(info);

                var vuser = Query.SelectUser(ConnectionString, update.Message.From.Id);
                var vUserId = vuser.Id;
                if (vuser == null)
                {
                    vuser = new CustomUser();
                    vuser.UserName = update.Message.From.Username;
                    vuser.User_Ident = update.Message.From.Id;
                    vuser.FirstName = update.Message.From.FirstName;
                    vuser.LastName = update.Message.From.LastName;
                    vuser.Roles_id = 0;
                    vUserId = Query.InsertUser(ConnectionString, vuser);
                }
                if (RoleList is null)
                    RoleList = Query.SelectRoles(ConnectionString, vUserId);

                SelectRole(botClient, update, token);

                return true;
            }
            return false;
        }
    }
}
