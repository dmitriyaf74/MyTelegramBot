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
using Telegram.Bot.Types.Enums;
using System.Runtime.CompilerServices;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesAll : CustomHandleUpdates
    {
        private const string _userkeybord = "userkeybord";
        private const string _userquerysbord = "userquerysbord";

        private const string _adminkeybord = "adminkeybord";
        private enum AdminButtons
        {
            _ShowNewUsers,
            _ShowInfo,
        }
        private enum OperatorButtons
        {
            _QueryOldestMessage,
            _Queryestimation,
        }

        const string cstrGetOldestMessage = "/Получить обращение";
        const string cstrGetEstimation = "/Оценить оператора";
        const string cstrGetCloseDialog = "/Завершить диалог";

        private List<UserTopics>? TopicList = null;

        public TelegramSession BotSession;
        public HandleUpdatesAll(TelegramSession ABotSession)
        {
            BotSession = ABotSession;
        }

        public pgQueryUser? UserQuery;

        public override void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        {
            updRecDelegate += UpdateReceivedStart;
            updRecDelegate += UpdateReceivedOperator;
        }

        public override void RegisterCallBackUpdates(ref UpdateCallBackDelegate? updCallBackDelegate)
        {
            updCallBackDelegate += UpdateCallBackUserKeyboard;
            updCallBackDelegate += UpdateCallBackAdminKeyboard;
            updCallBackDelegate += UpdateCallBackOperatorKeyboard;
        }

        private List<UserTopics>? GetTopicList()
        {
            if (TopicList is null)
                TopicList = UserQuery?.GetTopics();
            return TopicList;
        }
        private bool UserQuerysExists(int ALevel)
        {
            var filtered = GetTopicList()?.Where(p => p.Parent_Id == ALevel).ToList();
            return filtered?.Count > 0;
        }

        private string GetTopicName(int AId)
        {
            var filtered = GetTopicList()?.Where(p => p.Id == AId).ToList();
            if (filtered?.Count > 0) 
                return filtered[0].Name ?? "";
            return "";
        }
        private async Task ShowUserButtons(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            if (!UserQuerysExists(ALevel))
                return;
            Dictionary<int, string> keyboardList = new();
            GetTopicList();
            if (TopicList is not null)
                foreach (var r in TopicList)
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

        private async Task ShowAdminButtons(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            const string cSelectReport = "Выберите отчет:";

            Dictionary<int, string> keyboardList = new();
            keyboardList.Add((int)AdminButtons._ShowNewUsers, "10 новых пользователей");
            keyboardList.Add((int)AdminButtons._ShowInfo, "Другая информация");

            long? vChatId = null;
            if (Aupdate?.Message is not null)
                vChatId = Aupdate?.Message?.Chat?.Id;
            else
                vChatId = Aupdate?.CallbackQuery?.Message?.Chat?.Id;

            if (vChatId != null)
                await AbotClient.SendMessage(vChatId, cSelectReport, replyMarkup:
                                  GetKeyBoard(keyboardList, _adminkeybord));
        }

        private async Task ShowOperatorButtons(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            var keyboard = new ReplyKeyboardMarkup();
            keyboard.ResizeKeyboard = true;

            var info = "Выберите доступную роль";

            keyboard.AddButton(new KeyboardButton(cstrGetOldestMessage));
            keyboard.AddButton(new KeyboardButton(cstrGetEstimation));
            keyboard.AddButton(new KeyboardButton(cstrGetCloseDialog));


            long? vChatId = null;
            if (Aupdate?.Message is not null)
                vChatId = Aupdate?.Message?.Chat?.Id;
            else
                vChatId = Aupdate?.CallbackQuery?.Message?.Chat?.Id;

            if (vChatId != null)
                await AbotClient.SendMessage(vChatId, info, replyMarkup: keyboard);
        }

        private async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
        {
            DoConShowMessage($"{ARole_Id}");
            switch (ARole_Id)
            {
                case RolesEnum.reUser: 
                    await ShowUserButtons(AbotClient, Aupdate, 0);
                    break; 
                case RolesEnum.reAdmin: 
                    await ShowAdminButtons(AbotClient, Aupdate, 0);
                    break;
                case RolesEnum.reOperator:
                    await ShowOperatorButtons(AbotClient, Aupdate, 0);
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

        private async Task HideInlineKeyboard(ITelegramBotClient AbotClient, Update? Aupdate)
        {
            #pragma warning disable CS8602
            await AbotClient.EditMessageReplyMarkup(Aupdate.CallbackQuery.Message.Chat.Id, Aupdate.CallbackQuery.Message.Id);
            #pragma warning restore CS8602
        }

        private async Task HideKeyboard(ITelegramBotClient AbotClient, Update? Aupdate)
        {
            ReplyKeyboardRemove replyKeyboardRemove = new();
            #pragma warning disable CS8602
            await AbotClient.SendMessage(
                chatId: Aupdate.Message.Chat.Id,
                text: "Клавиатура скрыта.",
                replyMarkup: replyKeyboardRemove);
            #pragma warning restore CS8602
        }

        private Dictionary<int, string> GetRoleKeyboardList(long? AUserId)
        {
            var vRoleList = UserQuery?.SelectRoles(AUserId);
            Dictionary<int, string> keyboardList = new();
            if (vRoleList != null)
            {
                if (vRoleList.Count > 0)
                    foreach (var r in vRoleList)
                        keyboardList.Add((int)r.Id, r?.Name ?? "");
            }
            return keyboardList;
        }

        private CustomUser? GetUser(Update? Aupdate)
        {
            var vuser = UserQuery?.SelectUserByIdent(Aupdate?.Message?.From?.Id);
            if ((vuser == null) && (Aupdate?.Message?.From is not null))
            {
                vuser = new CustomUser();
                vuser.UserName = Aupdate.Message.From.Username ?? "";
                vuser.User_Ident = Aupdate.Message.From.Id;
                vuser.FirstName = Aupdate.Message.From.FirstName ?? "";
                vuser.LastName = Aupdate.Message.From.LastName ?? "";
                vuser.Roles_id = RolesEnum.reUnknown;
                vuser.Topic_id = 0;
                vuser.Id = UserQuery?.InsertUser(vuser);
            }
            else
            if (Aupdate?.Message?.Text == "/start")
                UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, RolesEnum.reUnknown);
            return vuser;
        }

        /// <summary>
        /// Simple Commands
        /// </summary>
        /// <param name="AbotClient"></param>
        /// <param name="Aupdate"></param>
        /// <param name="Atoken"></param>
        /// <returns></returns>
        private async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;
            var vuser = GetUser(Aupdate);
            if (vuser?.Roles_id == RolesEnum.reUnknown)
                vuser.Roles_id = RolesEnum.reUser;

            if (Aupdate?.Message?.Text?[0] == '/')
            {
                switch (Aupdate.Message.Text)
                {
                    case "/start":
                    {
                        if (Aupdate.Message.From is null)
                            return;

                        await AbotClient.SendMessage(Aupdate.Message.Chat.Id, $"Здравствуйте {Aupdate?.Message?.From?.FirstName}");

                        var info = $"{Aupdate?.Message?.From?.Username}({Aupdate?.Message?.From?.Id})" +
                                $"({Aupdate?.Message?.From?.FirstName} {Aupdate?.Message?.From?.LastName}) : {Aupdate?.Message?.Text}";
                        DoConShowMessage(info);
                                                            
                        var keyboardList = GetRoleKeyboardList(vuser?.Id);
                        if ((keyboardList?.Count > 1) && (Aupdate?.Message?.Chat is not null))
                        {
                            await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Выберите роль:", replyMarkup:
                                GetKeyBoard(keyboardList, _userkeybord));
                        }
                        else
                        {
                            if (vuser is not null)
                                vuser.Roles_id = RolesEnum.reUser;
                            UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, vuser?.Roles_id);
                            await DoGetMenuRole(AbotClient, Aupdate, vuser?.Roles_id);
                        }
                        break;
                    }
                    case "/hide":
                        await HideKeyboard(AbotClient, Aupdate);
                        break;
                    default:
                        break;

                }
            } 
            else
            {
                if (vuser?.Roles_id != RolesEnum.reUser)
                    return;
                UserQuery?.AddMessage(vuser?.Id, Aupdate?.Message?.Text, vuser?.Topic_id);
                UserQuery?.SetUserChatId(vuser?.User_Ident, Aupdate?.Message?.Chat.Id);
                DoConShowMessage($"Оператор ответит вам в ближайшее время");
            }


        }

        private async Task UpdateReceivedOperator(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;
            var vuser = GetUser(Aupdate);

            if (Aupdate?.Message?.Text?[0] == '/')
            {
                switch (Aupdate.Message.Text)
                {
                    /*
                                запросить старый запрос, получить пользователя и выдать всю переписку с ним, отмечая вопрос-ответ
                                запросить у пользователя оценку работы оператора
                                отправлять сообщение пользователю в чат и сохранять в БД
                                код чата прописывать при старте и в момент отправки сообщения

                                 */
                    case cstrGetOldestMessage:
                        {
                            if (Aupdate.Message.From is null)
                                return;

                            
                            var vMessageUserId = UserQuery?.GetOldestMessageUserId();
                            var vUserMessages = UserQuery?.GetUserMessages(vMessageUserId);
                            if (vUserMessages?.Count > 0)
                            {
                                var vMessage = "";
                                var IsNewStr = "";
                                var vUser = UserQuery?.SelectUserById(vUserMessages[0].User_Id);
                                foreach (var item in vUserMessages)
                                {
                                    var vAnswerer = UserQuery?.SelectUserById(vUserMessages[0].Answerer_Id);
                                    if (item.IsNew)
                                        IsNewStr = "Новое\n";
                                    else
                                        IsNewStr = "";
                                    if (vAnswerer != null)
                                        vMessage = $"{IsNewStr}{item.Date_Time} Пользователь: {vAnswerer.UserName} \n{item.MessageStr}";
                                    else
                                        vMessage = $"{IsNewStr}{item.Date_Time} Оператор: {vUser?.UserName} \n{item.MessageStr}";
                                    await AbotClient.SendMessage(Aupdate.Message.Chat.Id, vMessage);
                                }
                            }
                            else
                                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Нет непрочитанных сообщений");
                            break;
                        }
                    case cstrGetEstimation:
                        break;
                    case cstrGetCloseDialog:
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
                if (vuser?.Roles_id != RolesEnum.reOperator)
                    return;
                //UserQuery?.AddOperatorMessage(vuser?.Id, Aupdate?.Message?.Text, vuser?.Topic_id);
                DoConShowMessage($"Оператор");
            }

        }

        /// <summary>
        /// User Buttons
        /// </summary>
        /// <param name="AbotClient"></param>
        /// <param name="Aupdate"></param>
        /// <param name="Atoken"></param>
        /// <returns></returns>
        private async Task UpdateCallBackUserKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
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
                            UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, (RolesEnum)vRoleId);
                            await HideInlineKeyboard(AbotClient, Aupdate);
                            
                            await DoGetMenuRole(AbotClient, Aupdate, (RolesEnum)vRoleId);
                            break;

                        case _userquerysbord:
                            await HideInlineKeyboard(AbotClient, Aupdate);
                            var vLevel = int.Parse(strs[1]);
                            if (UserQuerysExists(vLevel))
                            {
                                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваш раздел {GetTopicName(vLevel)}");
                                await ShowUserButtons(AbotClient, Aupdate, vLevel);
                            }
                            else
                            {
                                UserQuery?.SetUserQueryId(Aupdate?.CallbackQuery?.From?.Id, vLevel);
                                DoConShowMessage($"раздел {GetTopicName(vLevel)}");
                                if (Aupdate is not null)
                                    await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваш раздел {GetTopicName(vLevel)}, задайте свой вопрос");
                            }
                            break;

                        default:
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// Admin Buttons
        /// </summary>
        /// <param name="AbotClient"></param>
        /// <param name="Aupdate"></param>
        /// <param name="Atoken"></param>
        /// <returns></returns>
        private async Task UpdateCallBackAdminKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null))
                {
                    switch (strs[0])
                    {
                        case _adminkeybord:
                            await HideInlineKeyboard(AbotClient, Aupdate);
                            switch (int.Parse(strs[1]))
                            { 
                                case (int)AdminButtons._ShowNewUsers:
                                    await AbotClient.SendMessage(
                                        chatId: Aupdate.CallbackQuery.Message.Chat.Id,
                                        //text: $"Тут будет таблица с новыми пользователями!\n{GenerateHtmlTable()}",
                                        text: $"Таблица с новыми пользователями!",
                                        parseMode: ParseMode.Html);

                                    var vTextUsers = "";
                                    var vNewUsers = UserQuery?.SelectNewUsers();
                                    if (vNewUsers != null)
                                        foreach (var item in vNewUsers)
                                            vTextUsers = $"{vTextUsers}\n{item.User_Ident}-{item.UserName}-{item.FirstName}";
                                    await AbotClient.SendMessage(
                                        chatId: Aupdate.CallbackQuery.Message.Chat.Id,
                                        text: vTextUsers,
                                        parseMode: ParseMode.Html);
                                    break;

                                case (int)AdminButtons._ShowInfo:
                                    await AbotClient.SendMessage(
                                        chatId: Aupdate.CallbackQuery.Message.Chat.Id,
                                        text: $"Привет!",
                                        parseMode: ParseMode.Html);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }

            }
        }

        private async Task UpdateCallBackOperatorKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null))
                {
                    switch (strs[0])
                    {
                        case _adminkeybord:
                            await HideInlineKeyboard(AbotClient, Aupdate);
                            switch (int.Parse(strs[1]))
                            {
                                case (int)OperatorButtons._QueryOldestMessage:
                                    await AbotClient.SendMessage(
                                        chatId: Aupdate.CallbackQuery.Message.Chat.Id,
                                        //text: $"Тут будет таблица с новыми пользователями!\n{GenerateHtmlTable()}",
                                        text: $"Таблица с новыми пользователями!",
                                        parseMode: ParseMode.Html);

                                    
                                    break;

                                case (int)OperatorButtons._Queryestimation:
                                    await AbotClient.SendMessage(
                                        chatId: Aupdate.CallbackQuery.Message.Chat.Id,
                                        text: $"Привет!",
                                        parseMode: ParseMode.Html);
                                    break;
                                default:
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }

            }
        }

    }
}
