using Microsoft.VisualBasic;
using MyTelegramBot.Classes;
using MyTelegramBot.DapperClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesAll : CustomHandleUpdates
    {
        public const string _mainkeybord = "mainkeybord";

        public TelegramSession BotSession;
        public HandleUpdatesAll(TelegramSession ABotSession)
        {
            BotSession = ABotSession;
        }

        public pgQueryUser? UserQuery;
        public override void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        {
            updRecDelegate += UpdateReceivedStart;
        }

        public override void RegisterCallBackUpdates(ref UpdateCallBackDelegate? updCallBackDelegate)
        {
            updCallBackDelegate += UpdateCallBaclHideKeyboard;
        }

        protected void ShowUserQuerys(int ALevel)
        {
        }
        protected void DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, long? ARole_Id)
        {
            DoConShowMessage($"{ARole_Id}");
            switch (ARole_Id)
            {
                case 1: //User
                    ShowUserQuerys(0);
                    break; 
                case 2: //Admin
                    break;
                case 3: //Boss
                    break;
                case 4: //Operator
                    break;
                case 5: //Developer
                    break;                                    
                default:
                    DoConShowMessage($"Некорректная роль {ARole_Id}");
                    break;
            }
        }

        private InlineKeyboardMarkup GetKeyBoard(Dictionary<int, string> AkeyboardList, string prefix)
        {
            //var keyboard = new InlineKeyboardMarkup();
            var keyboardRows = new List<List<InlineKeyboardButton>>();
            foreach (var item in AkeyboardList)
            {
                //keyboard.AddButton(new InlineKeyboardButton($"{item.Key}.{item.Value}", $"{prefix}.{item.Key}"));
                var button = new InlineKeyboardButton($"{item.Key}.{item.Value}", $"{prefix}.{item.Key}");
                var row = new List<InlineKeyboardButton> { button };
                keyboardRows.Add(row);
            }
            return new InlineKeyboardMarkup(keyboardRows);
        }

        private void HideKeyboard(ITelegramBotClient AbotClient, Update? Aupdate)
        {
            #pragma warning disable CS8602
            AbotClient.EditMessageReplyMarkup(Aupdate.CallbackQuery.Message.Chat.Id, Aupdate.CallbackQuery.Message.Id);
            #pragma warning restore CS8602
        }

        private Dictionary<int, string> GetRoleKeyboardList(long? AUserId)
        {
            var vRoleList = UserQuery?.SelectRoles(AUserId);
            Dictionary<int, string> keyboardList = new();
            if (vRoleList != null) 
              foreach (var r in vRoleList)
                  keyboardList.Add((int)r.Id, r?.Name ?? "");
            return keyboardList;
        }

        public async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;

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

                            await AbotClient.SendMessage(Aupdate.Message.Chat.Id, $"Здравствуйте{Aupdate?.Message?.From?.FirstName}");

                            var info = $"{Aupdate?.Message?.From?.Username}({Aupdate?.Message?.From?.Id})" +
                                    $"({Aupdate?.Message?.From?.FirstName} {Aupdate?.Message?.From?.LastName}) : {Aupdate?.Message?.Text}";
                            DoConShowMessage(info);
                                                            
                            var vuser = UserQuery?.SelectUser(Aupdate?.Message.From.Id); 
                            long? vUserId = vuser?.Id;
                            if ((vuser == null) && (Aupdate?.Message?.From is not null))
                            {
                                vuser = new CustomUser();
                                vuser.UserName = Aupdate.Message.From.Username;
                                vuser.User_Ident = Aupdate.Message.From.Id;
                                vuser.FirstName = Aupdate.Message.From.FirstName;
                                vuser.LastName = Aupdate.Message.From.LastName;
                                vuser.Roles_id = -1;
                                vUserId = UserQuery?.InsertUser(vuser);
                            }
                            else
                                UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, -1);

                            var keyboardList = GetRoleKeyboardList(vUserId);
                            if ((keyboardList?.Count > 1) && (Aupdate?.Message?.Chat is not null))
                            {
                                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Выберите роль:", replyMarkup: 
                                    GetKeyBoard(keyboardList, _mainkeybord));
                            }
                            else 
                            if (keyboardList?.Count == 1)
                                UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, vuser?.Roles_id);
                            DoGetMenuRole(AbotClient, Aupdate, vuser?.Roles_id);

                        }
                        break;
                    }
                    case "/hide":
                        {
                            ReplyKeyboardRemove replyKeyboardRemove = new();

                            await AbotClient.SendMessage(
                                chatId: Aupdate.Message.Chat.Id,
                                text: "Клавиатура скрыта.",
                                replyMarkup: replyKeyboardRemove);
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

        public bool UpdateCallBaclHideKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if (strs?.Length > 1)
                {
                    if ((strs[0] == _mainkeybord) && (BotSession is not null))
                    {
                        var vRoleId = int.Parse(strs[1]);
                        var vRoleName = BotSession?.Roles?[vRoleId] ?? "";
                        if (vRoleName != string.Empty)
                            AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваша роль {vRoleName}");
                        UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, vRoleId);
                        HideKeyboard(AbotClient, Aupdate);

                        DoGetMenuRole(AbotClient, Aupdate, vRoleId);
                        return true;
                    }
                }

            }
            return false;
        }


    }
}
