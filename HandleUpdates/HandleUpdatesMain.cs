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
    internal class HandleUpdatesMain : CustomHandleUpdates
    {
        public TelegramSession BotSession;
        public HandleUpdatesMain(TelegramSession ABotSession, pgQueryUser AUserQuery)
        {
            BotSession = ABotSession;
            UserQuery = AUserQuery;
        }
        private pgQueryUser? UserQuery;

        private const string _rolekeybord = "rolekeybord";

        public InlineKeyboardMarkup GetKeyBoard(Dictionary<int, string> AkeyboardList, string prefix)
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

        public async Task HideInlineKeyboard(ITelegramBotClient AbotClient, Update? Aupdate)
        {
            #pragma warning disable CS8602
            await AbotClient.EditMessageReplyMarkup(Aupdate.CallbackQuery.Message.Chat.Id, Aupdate.CallbackQuery.Message.Id);
            #pragma warning restore CS8602
        }

        public async Task HideKeyboard(ITelegramBotClient AbotClient, Update? Aupdate)
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

        public CustomUser? GetUser(Update? Aupdate)
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
        protected override async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
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
                        await HideKeyboard(AbotClient, Aupdate);


                        await AbotClient.SendMessage(Aupdate.Message.Chat.Id, $"Здравствуйте {Aupdate?.Message?.From?.FirstName}");

                        var info = $"{Aupdate?.Message?.From?.Username}({Aupdate?.Message?.From?.Id})" +
                                $"({Aupdate?.Message?.From?.FirstName} {Aupdate?.Message?.From?.LastName}) : {Aupdate?.Message?.Text}";
                        DoConShowMessage(info);
                                                            
                        var keyboardList = GetRoleKeyboardList(vuser?.Id);
                        if ((keyboardList?.Count > 1) && (Aupdate?.Message?.Chat is not null))
                        {
                            await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Выберите роль:", replyMarkup:
                                GetKeyBoard(keyboardList, _rolekeybord));
                        }
                        else
                        {
                            UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, vuser?.Roles_id);
                            if (BotSession?.DoGetMenuRole != null)
                                await BotSession.DoGetMenuRole(AbotClient, Aupdate, vuser?.Roles_id);
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


        /// <summary>
        /// User Buttons
        /// </summary>
        /// <param name="AbotClient"></param>
        /// <param name="Aupdate"></param>
        /// <param name="Atoken"></param>
        /// <returns></returns>
        protected override async Task UpdateCallBackKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null))
                {
                    switch (strs[0])
                    {
                        case _rolekeybord:
                            var vRoleId = int.Parse(strs[1]);
                            var vRoleName = BotSession?.Roles?[vRoleId] ?? "";
                            if (vRoleName != string.Empty)
                                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваша роль {vRoleName}");
                            UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, (RolesEnum)vRoleId);
                            await HideInlineKeyboard(AbotClient, Aupdate);

                            if (BotSession != null)
                                await BotSession.DoGetMenuRolesDelegates(AbotClient, Aupdate, (RolesEnum)vRoleId);
                            break;                        

                        default:
                            break;
                    }
                }

            }
        }
    }
}
