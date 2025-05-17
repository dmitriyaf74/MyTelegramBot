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
        private const string _userkeybord = "userkeybord";
        private const string _userquerysbord = "userquerysbord";

        private const string _adminkeybord = "adminkeybord";
        public enum AdminButtons
        {
            _ShowNewUsers,
            _ShowInfo,
        }

        private List<UserQueriesTree>? QuerysTreeList = null;

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
            updCallBackDelegate += UpdateCallBaclUserKeyboard;
        }

        private bool UserQuerysExists(int ALevel)
        {
            if (QuerysTreeList is null)
                QuerysTreeList = UserQuery?.GetUserQuerysTree();
            var filtered = QuerysTreeList?.Where(p => p.Parent_Id == ALevel).ToList();
            return filtered?.Count > 0;
        }

        private string GetQuerysTreeName(int AId)
        {
            if (QuerysTreeList is null)
                QuerysTreeList = UserQuery?.GetUserQuerysTree();
            var filtered = QuerysTreeList?.Where(p => p.Id == AId).ToList();
            if (filtered?.Count > 0) 
                return filtered[0].Name ?? "";
            return "";
        }
        public async Task ShowUserQueries(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            if (!UserQuerysExists(ALevel))
                return;
            Dictionary<int, string> keyboardList = new();
            if (QuerysTreeList != null)
                foreach (var r in QuerysTreeList)
                    if (r.Parent_Id == ALevel)
                      keyboardList.Add((int)r.Id, r?.Name ?? "");

            if (Aupdate?.Message is not null)
              await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Выберите раздел:", replyMarkup:
                                    GetKeyBoard(keyboardList, _userquerysbord));
            else
            if (Aupdate?.CallbackQuery?.Message is not null)
                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, "Выберите раздел:", replyMarkup:
                                      GetKeyBoard(keyboardList, _userquerysbord));
        }

        public async Task ShowAdminQueries(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            Dictionary<int, string> keyboardList = new();
            keyboardList.Add((int)AdminButtons._ShowNewUsers, "10 новых пользователей");
            keyboardList.Add((int)AdminButtons._ShowInfo, "Другая информация");

            if (Aupdate?.Message is not null)
                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Выберите раздел:", replyMarkup:
                                      GetKeyBoard(keyboardList, _adminkeybord));
            else
            if (Aupdate?.CallbackQuery?.Message is not null)
                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, "Выберите раздел:", replyMarkup:
                                      GetKeyBoard(keyboardList, _adminkeybord));
        }

        public async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, long? ARole_Id)
        {
            DoConShowMessage($"{ARole_Id}");
            switch (ARole_Id)
            {
                case 0: //User
                    await ShowUserQueries(AbotClient, Aupdate, 0);
                    break; 
                case 1: //Admin
                    await ShowAdminQueries(AbotClient, Aupdate, 0);
                    break;
                case 2: //Boss
                    break;
                case 3: //Operator
                    break;
                case 4: //Developer
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

                            await AbotClient.SendMessage(Aupdate.Message.Chat.Id, $"Здравствуйте {Aupdate?.Message?.From?.FirstName}");

                            var info = $"{Aupdate?.Message?.From?.Username}({Aupdate?.Message?.From?.Id})" +
                                    $"({Aupdate?.Message?.From?.FirstName} {Aupdate?.Message?.From?.LastName}) : {Aupdate?.Message?.Text}";
                            DoConShowMessage(info);
                                                            
                            var vuser = UserQuery?.SelectUser(Aupdate?.Message.From.Id); 
                            long? vUserId = vuser?.Id;
                            if ((vuser == null) && (Aupdate?.Message?.From is not null))
                            {
                                vuser = new CustomUser();
                                vuser.UserName = Aupdate.Message.From.Username ?? "";
                                vuser.User_Ident = Aupdate.Message.From.Id;
                                vuser.FirstName = Aupdate.Message.From.FirstName ?? "";
                                vuser.LastName = Aupdate.Message.From.LastName ?? "";
                                vuser.Roles_id = -1;
                                vUserId = UserQuery?.InsertUser(vuser);
                            }
                            else
                                UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, -1);

                            var keyboardList = GetRoleKeyboardList(vUserId);
                            if ((keyboardList?.Count > 1) && (Aupdate?.Message?.Chat is not null))
                            {
                                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Выберите роль:", replyMarkup:
                                    GetKeyBoard(keyboardList, _userkeybord));
                            }
                            else
                            {
                                if (vuser is not null)
                                    vuser.Roles_id = 0;
                                UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, vuser?.Roles_id);
                                await DoGetMenuRole(AbotClient, Aupdate, vuser?.Roles_id);
                            }

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
                var vuser = UserQuery?.SelectUser(Aupdate?.Message?.From?.Id);
                UserQuery?.AddMessage(vuser?.Id, Aupdate?.Message?.Text);
                DoConShowMessage($"Привет");
                /*
                 * Проверить права
                 * для нового пользователя установить роль
                 * ограничить 10 новыми сообщениями
                 */
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

        public async Task UpdateCallBaclUserKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null)) 
                {
                    switch (strs[0])
                    {
                        case _userkeybord: 
                            var vRoleId = int.Parse(strs[1]);
                            var vRoleName = BotSession?.Roles?[vRoleId] ?? "";
                            if (vRoleName != string.Empty)
                                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваша роль {vRoleName}");
                            UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, vRoleId);
                            HideKeyboard(AbotClient, Aupdate);
                            
                            await DoGetMenuRole(AbotClient, Aupdate, vRoleId);
                            break;
                        case _userquerysbord:
                            HideKeyboard(AbotClient, Aupdate);
                            var vLevel = int.Parse(strs[1]);
                            if (UserQuerysExists(vLevel))
                            {
                                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваш раздел {GetQuerysTreeName(vLevel)}");
                                await ShowUserQueries(AbotClient, Aupdate, vLevel);
                            }
                            else
                            {
                                UserQuery?.SetUserQueryId(Aupdate?.CallbackQuery?.From?.Id, vLevel);
                                DoConShowMessage($"раздел {GetQuerysTreeName(vLevel)}");
                                if (Aupdate is not null)
                                    await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваш раздел {GetQuerysTreeName(vLevel)}, задайте свой вопрос");
                            }
                            break;
                        default:
                            DoConShowMessage($"Некорректная кнопка");
                            break;
                    }
                }

            }
        }

        public async Task UpdateCallBaclAdminKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null))
                {
                    switch (strs[0])
                    {
                        case _adminkeybord:
                            HideKeyboard(AbotClient, Aupdate);
                            switch (int.Parse(strs[1]))
                            { 
                                case (int)AdminButtons._ShowNewUsers:
                                    await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id,
                                        $"Тут будет таблица с новыми пользователями!");
                                    break;
                                case (int)AdminButtons._ShowInfo:
                                    await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, 
                                        $"Привет!");
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            DoConShowMessage($"Некорректная кнопка");
                            break;
                    }
                }

            }
        }

    }
}
