using Microsoft.VisualBasic;
using MyTelegramBot.Classes;
using MyTelegramBot.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesMain : CustomHandleUpdates
    {
        public TelegramSession BotSession;
        public HandleUpdatesMain(TelegramSession ABotSession)
        {
            BotSession = ABotSession;
        }
        private ICustomQuery? UserQuery { get => BotSession?.QueryUser; }

        private const string _rolekeybord = "rolekeybord";

        public InlineKeyboardMarkup GetKeyBoard(Dictionary<int, string> AkeyboardList, string prefix)
        {
            var keyboardRows = new List<List<InlineKeyboardButton>>();
            foreach (var item in AkeyboardList)
            {
                var button = new InlineKeyboardButton($"{item.Key}.{item.Value}", $"{prefix}.{item.Key}");
                var row = new List<InlineKeyboardButton> { button };
                keyboardRows.Add(row);
            }
            return new InlineKeyboardMarkup(keyboardRows);
        }

        public async Task ShowDefaultButtons(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            var keyboard = new ReplyKeyboardMarkup();
            keyboard.ResizeKeyboard = true;

            var info = "Добавлены кнопки";

            keyboard.AddButton(new KeyboardButton("/start"));


            long? vChatId = null;
            if (Aupdate?.Message is not null)
                vChatId = Aupdate.Message?.Chat?.Id;
            else
            if (Aupdate?.CallbackQuery?.Message?.Chat is not null)
                vChatId = Aupdate.CallbackQuery.Message.Chat.Id;

            if (vChatId != null)
                await AbotClient.SendMessage(vChatId, info, replyMarkup: keyboard);
        }


        public async Task HideInlineKeyboard(ITelegramBotClient AbotClient, Update? Aupdate)
        {
            #pragma warning disable CS8602
            await AbotClient.EditMessageReplyMarkup(Aupdate.CallbackQuery.Message.Chat.Id, Aupdate.CallbackQuery.Message.Id);
            #pragma warning restore CS8602
        }

        public async Task HideKeyboard(ITelegramBotClient AbotClient, Update? Aupdate, string Info)
        {
            ReplyKeyboardRemove replyKeyboardRemove = new();
            #pragma warning disable CS8602
            await AbotClient.SendMessage(
                chatId: Aupdate.Message.Chat.Id,
                text: Info,
                replyMarkup: replyKeyboardRemove);
            #pragma warning restore CS8602            
        }

        private List<CustomRole>? FRoleList;
        private void InitRoleList(long? Auser_ident)
        {
            if (UserQuery != null) 
                FRoleList = UserQuery.SelectRoles(Auser_ident);
        }
        private bool HasRole(RolesEnum? Arole_id)
        {
            if ((FRoleList != null) && (FRoleList.Any()))
            {
                var roles = FRoleList.Where(x => x.Id == Arole_id).ToList();
                return ((roles != null) && (roles.Any()));
            }
            return false;
        }

        private Dictionary<int, string> GetRoleKeyboardList(long? Auser_ident)
        {
            Dictionary<int, string> keyboardList = new();
            if (FRoleList != null)
            {
                if (FRoleList.Count > 0)
                    foreach (var r in FRoleList)
                        keyboardList.Add((int)r.Id, r?.Name ?? "");
            }
            return keyboardList;
        }

        private CustomUser? CurUser;
        private CustomUser? InitUser(Update? Aupdate)
        {
            var vuser = UserQuery?.SelectUserByIdent(Aupdate?.Message?.From?.Id);
            InitRoleList(vuser?.Id);

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
            if (//(Aupdate?.Message?.Text?.ToLower() == "/start") && 
                (vuser != null) &&
                (vuser.Roles_id != RolesEnum.reUser) &&
                (!HasRole(vuser.Roles_id)))
            {
                UserQuery?.SetUserRole(Aupdate?.Message?.From?.Id, RolesEnum.reUser);
                if (vuser != null)
                    vuser.Roles_id = RolesEnum.reUser;
            }
            CurUser = vuser;
            return vuser;
        }
        public CustomUser? GetUser()
        {
            return CurUser;
        }

        protected override async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            var vuser = InitUser(Aupdate);
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;
            if (vuser?.Roles_id == RolesEnum.reUnknown)
                vuser.Roles_id = RolesEnum.reUser;
            //if (vuser?.Roles_id != RolesEnum.reUser)
            //    return;
            
            if (Aupdate?.Message?.Text?[0] == '/')
            {
                switch (Aupdate.Message.Text.ToLower())
                {
                    case "/start":
                    {
                        if (Aupdate.Message.From is null)
                            return;

                        //await AbotClient.SendMessage(Aupdate.Message.Chat.Id, $"Здравствуйте {Aupdate?.Message?.From?.FirstName}");
                        await HideKeyboard(AbotClient, Aupdate, $"Здравствуйте {Aupdate?.Message?.From?.FirstName}");

                        var info = $"{Aupdate?.Message?.From?.Username}({Aupdate?.Message?.From?.Id})" +
                                $"({Aupdate?.Message?.From?.FirstName} {Aupdate?.Message?.From?.LastName}) : {Aupdate?.Message?.Text}";
                        DoConShowMessage(info);
                                                            
                        var keyboardList = GetRoleKeyboardList(vuser?.User_Ident);
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

                        if (vuser is not null) 
                            UserQuery?.SetActiveSenderId(vuser.Id, null);
                        break;
                    }
                    case "/hide":
                        await HideKeyboard(AbotClient, Aupdate, "Клавиатура скрыта");
                        break;
                    default:
                        break;

                }
            }             
        }

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
                            RolesEnum vRoleId = (RolesEnum)int.Parse(strs[1]);
                            var vRoleName = BotSession?.Roles?[vRoleId] ?? "";
                            if (vRoleName != string.Empty)
                                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваша роль {vRoleName}");
                            UserQuery?.SetUserRole(Aupdate?.CallbackQuery?.From?.Id, vRoleId);
                            await HideInlineKeyboard(AbotClient, Aupdate);

                            if (BotSession != null)
                                await BotSession.DoGetMenuRolesDelegates(AbotClient, Aupdate, vRoleId);
                            break;                        

                        default:
                            break;
                    }
                }

            }
        }
    }
}
