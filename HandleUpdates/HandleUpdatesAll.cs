using MyTelegramBot.DapperClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using MyTelegramBot.Classes;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesAll : CustomHandleUpdates
    {
        public override void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        {
            updRecDelegate += UpdateReceivedStart;
            //updRecDelegate += UpdateReceivedRole;
        }

        public override void RegisterCallBackUpdates(ref UpdateCallBackDelegate? updCallBackDelegate)
        {

            updCallBackDelegate += UpdateCallBaclHideKeyboard;
        }


        protected static void DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, long? ARole_Id)
        {
            void ShowUserMenu0()
            {
                var buttons = new ReplyKeyboardMarkup();
                buttons.ResizeKeyboard = true;

                var info = "Выберите тему";

                buttons.AddButton(new KeyboardButton($"Ресторан"));
                buttons.AddButton(new KeyboardButton($"Тихий час"));
                buttons.AddButton(new KeyboardButton($"Трансфер"));
                if (Aupdate?.Message?.Chat is not null)
                    AbotClient.SendMessage(Aupdate.Message.Chat.Id, info, replyMarkup: buttons);


            }

            if (ARole_Id == 0)
            {
                ShowUserMenu0();
            }
        }

        public async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;

            InlineKeyboardMarkup GetKeyBoard(List<CustomRole> ARoleList)
            {
                var keyboard = new InlineKeyboardMarkup();
                //keyboard.ResizeKeyboard = true;

                //var info = "Выберите доступную роль";

                foreach (var item in ARoleList)
                {
                    //var button = new KeyboardButton($"{item.Id}.{item.Name}");// { RequestUsers = "param1=value1" };                    
                    var button = new InlineKeyboardButton($"{item.Id}.{item.Name}", $"mainkeybord.{item.Id}");// { RequestUsers = "param1=value1" };                    
                    keyboard.AddButton(button);
                }


                return keyboard;

            };
            if (Aupdate?.Message?.Text?[0] == '/')
            {
                switch (Aupdate.Message.Text)
                {
                    case "/start":
                    {
                        if (Aupdate.Message.Text == "/start")
                        {
                            if (Aupdate.Message.From is null)
                                return;

                            var info = $"{Aupdate?.Message?.From?.Username}({Aupdate?.Message?.From?.Id})({Aupdate?.Message?.From?.FirstName} {Aupdate?.Message?.From?.LastName}) : {Aupdate?.Message?.Text}";
                            DoConShowMessage(info);
                                                            
                            var vuser = Query?.SelectUser(Aupdate?.Message.From.Id); 
                            long? vUserId = vuser?.Id;
                            if ((vuser == null) && (Aupdate?.Message?.From is not null))
                            {
                                vuser = new CustomUser();
                                vuser.UserName = Aupdate.Message.From.Username;
                                vuser.User_Ident = Aupdate.Message.From.Id;
                                vuser.FirstName = Aupdate.Message.From.FirstName;
                                vuser.LastName = Aupdate.Message.From.LastName;
                                vuser.Roles_id = 0;
                                vUserId = Query?.InsertUser(vuser);
                            }
                            //if (RoleList is null)
                            var vRoleList = Query?.SelectRoles(vUserId);

                            if (vRoleList?.Count > 1)
                            {
                                    if (Aupdate?.Message?.Chat is not null)
                                        await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Жамкни2!", replyMarkup: GetKeyBoard(vRoleList));
                            }
                            else
                                DoGetMenuRole(AbotClient, Aupdate, vRoleList?[0].Id);

                        }
                        break;
                    }
                    default:
                        break;

                }


            } 
            else
            {

            }


        }

        /*public async Task UpdateReceivedRole(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            //if (BeginUpdate) return;
            int ARole_Id = 0;
            bool HasRole()
            {
                string[]? vArrWord = Aupdate?.Message?.Text?.Split('.', StringSplitOptions.None);

                if (vArrWord?.Length > 1 && int.TryParse(vArrWord[0], out ARole_Id))
                {
                    var vuser = Query?.SelectUser(Aupdate.Message.From.Id);
                    return Query.HasRole(vuser.Id, ARole_Id);
                }
                return false;
            }

            if (HasRole())
            {
                if (ARole_Id == 0)
                {
                    DoGetMenuRole(AbotClient, Aupdate, ARole_Id);
                }
            }
            else
            {
                DoConShowMessage("NoHas");
            }

        }*/

        public static bool UpdateCallBaclHideKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
              AbotClient.EditMessageReplyMarkup(Aupdate.CallbackQuery.Message.Chat.Id, Aupdate.CallbackQuery.Message.Id);
            return false;
        }
    }
}
