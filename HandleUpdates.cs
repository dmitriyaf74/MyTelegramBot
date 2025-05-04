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

namespace MyTelegramBot
{
    internal class HandleUpdates : CustomHandleUpdates
    {
        //static List<CustomRole> RoleList;
        public static bool BeginUpdate = true;

        public static void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        {
            updRecDelegate += UpdateReceivedBegin;
            //updRecDelegate += HandleUpdateAsyncTest;

            updRecDelegate += UpdateReceivedStart;
            updRecDelegate += UpdateReceivedRole;
        }

        public static void RegisterCallBackUpdates(ref UpdateReceivedDelegate? updCallBackDelegate)
        {
            updCallBackDelegate += UpdateReceivedBegin;

            updCallBackDelegate += UpdateCallBaclHideKeyboard;
        }

        
        public static async Task UpdateCallBaclHideKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            AbotClient.EditMessageReplyMarkupAsync(Aupdate.CallbackQuery.Message.Chat.Id, Aupdate.CallbackQuery.Message.Id);
        }

        public static async Task UpdateReceivedBegin(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            BeginUpdate = false;
        }


        public static async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (BeginUpdate) return;

            InlineKeyboardMarkup GetKeyBoard(List<CustomRole> ARoleList)
            {
                var keyboard = new InlineKeyboardMarkup();
                //keyboard.ResizeKeyboard = true;

                var info = "Выберите доступную роль";

                foreach (var item in ARoleList)
                {
                    //var button = new KeyboardButton($"{item.Id}.{item.Name}");// { RequestUsers = "param1=value1" };                    
                    var button = new InlineKeyboardButton($"{item.Id}.{item.Name}", "qweqwe");// { RequestUsers = "param1=value1" };                    
                    keyboard.AddButton(button);
                }


                return keyboard;

            };

            if (Aupdate.Message.Text == "/start")
            {
                var info = $"{Aupdate.Message.From.Username}({Aupdate.Message.From.Id})({Aupdate.Message.From.FirstName} {Aupdate.Message.From.LastName}) : {Aupdate.Message.Text}";
                DoConShowMessage(info);

                var vuser = Query.SelectUser(Aupdate.Message.From.Id);
                var vUserId = vuser.Id;
                if (vuser == null)
                {
                    vuser = new CustomUser();
                    vuser.UserName = Aupdate.Message.From.Username;
                    vuser.User_Ident = Aupdate.Message.From.Id;
                    vuser.FirstName = Aupdate.Message.From.FirstName;
                    vuser.LastName = Aupdate.Message.From.LastName;
                    vuser.Roles_id = 0;
                    vUserId = Query.InsertUser(vuser);
                }
                //if (RoleList is null)
                var vRoleList = Query.SelectRoles(vUserId);

                if (vRoleList.Count > 1)
                {
                    //SelectRole(vRoleList);
                    await AbotClient.SendTextMessageAsync(Aupdate.Message.Chat.Id, "Жамкни2!", replyMarkup: GetKeyBoard(vRoleList));
                }
                else
                    DoGetMenuRole(AbotClient, Aupdate, vRoleList[0].Id);

                BeginUpdate = true;
                return;
            }
            BeginUpdate = false;
            
        }

        protected static void DoGetMenuRole(ITelegramBotClient AbotClient, Update Aupdate, long ARole_Id)
        {
            void ShowUserMenu0()
            {
                var buttons = new ReplyKeyboardMarkup();
                buttons.ResizeKeyboard = true;

                var info = "Выберите тему";

                buttons.AddButton(new KeyboardButton($"Ресторан"));
                buttons.AddButton(new KeyboardButton($"Тихий час"));
                buttons.AddButton(new KeyboardButton($"Трансфер"));
                AbotClient.SendMessage(Aupdate.Message.Chat.Id, info, replyMarkup: buttons);


            }

            if (ARole_Id == 0)
            {
                ShowUserMenu0();
            }
        }
        public static async Task UpdateReceivedRole(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (BeginUpdate) return;
            /*if (RoleList is null) return;
            bool HasRole()
            {
                string[] vArrWord = Aupdate.Message.Text.Split('.', StringSplitOptions.None);
                int vId = 0;

                if ((vArrWord.Length > 1) && (int.TryParse(vArrWord[0], out vId)))
                {
                    foreach (var vRole in RoleList)
                    {
                        if ((vRole.Id == vId) && (vRole.Name == vArrWord[1]))
                            return true;
                    }
                    return false;
                }
                return false;*/

            int ARole_Id = 0;
            bool HasRole()
            {
                string[] vArrWord = Aupdate.Message.Text.Split('.', StringSplitOptions.None);

                if ((vArrWord.Length > 1) && (int.TryParse(vArrWord[0], out ARole_Id)))
                {
                    var vuser = Query.SelectUser(Aupdate.Message.From.Id);
                    return (Query.HasRole(vuser.Id, ARole_Id));
                }
                return false;
            }

            if (HasRole())
            {
                if (ARole_Id == 0)
                {
                    DoGetMenuRole(AbotClient, Aupdate, ARole_Id);
                }
                BeginUpdate = true;
            }
            else
            {
                DoConShowMessage("NoHas");
                BeginUpdate = false;
            }

        }
        


    }
}
