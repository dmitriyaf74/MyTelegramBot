using MyTelegramBot.Classes;
using MyTelegramBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesAdmin : CustomHandleUpdates
    {

        private  TelegramSession BotSession;
        public HandleUpdatesAdmin(TelegramSession ABotSession, HandleUpdatesMain AHandleUpdatesUtils)
        {
            BotSession = ABotSession;
            _HandleUpdatesUtils = AHandleUpdatesUtils;
        }
        private ICustomQuery? UserQuery { get => BotSession?.QueryUser; }

        private HandleUpdatesMain _HandleUpdatesUtils;
        public HandleUpdatesMain HandleUpdatesUtils { get => _HandleUpdatesUtils; }

        private enum AdminButtons
        {
            _ShowNewUsers,
            _ShowInfo,
        }
        private const string _newuserskeybord = "newuserskeybord";
        private const string _roleuserskeybord = "roleuserskeybord";
        private const string cstrReportNewUsers = "/Новые пользователи";

        protected override async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
        {
            if (ARole_Id == RolesEnum.reAdmin)
            {
                await ShowAdminButtons(AbotClient, Aupdate, 0);
            }
        }
        public async Task ShowAdminButtons(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            var keyboard = new ReplyKeyboardMarkup();
            keyboard.ResizeKeyboard = true;

            var info = "Добавлены кнопки";

            keyboard.AddButton(new KeyboardButton(cstrReportNewUsers));
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

        protected override async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;
            var vuser = HandleUpdatesUtils.GetUser();
            if (vuser?.Roles_id != RolesEnum.reAdmin)
                return;

            if (Aupdate?.Message?.Text?[0] == '/')
            {
                switch (Aupdate.Message.Text)
                {
                    case cstrReportNewUsers:
                        var vNewUsers = UserQuery?.SelectNewUsers();
                        if (vNewUsers != null)
                        {
                            Dictionary<long, string> keyboardList = new();
                            foreach (var item in vNewUsers.OrderBy(x => x.Id))
                                keyboardList.Add(item.Id ?? 0, $"{item.UserName}/{item.User_Ident}");
                            await AbotClient.SendMessage(Aupdate?.Message?.Chat?.Id ?? 0, "Выберите пользователя для смены прав", replyMarkup:
                                                              HandleUpdatesUtils.GetKeyBoard(keyboardList, _newuserskeybord));
                        }
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
                    var vuser = UserQuery?.SelectUserByIdent(Aupdate?.CallbackQuery?.From?.Id);
                    switch (strs[0])
                    {
                        case _newuserskeybord:
                            var vSender = UserQuery?.SelectUserById(int.Parse(strs[1]));
                            if (vSender != null && vuser != null)
                            {
                                _HandleUpdatesUtils?.HideInlineKeyboard(AbotClient, Aupdate);
                                UserQuery?.SetActiveSenderId(vuser.Id, vSender.Id);
                                vuser.Sender_Id = vSender.Id;
                                Dictionary<long, string> keyboardList = new();
                                var vRoles = UserQuery?.GetAllRoles();
                                if (vRoles != null)
                                {
                                    foreach (var item in vRoles.OrderBy(x => x.Id))
                                        keyboardList.Add((long)item.Id, $"Добавить {item.Name}");
                                    foreach (var item in vRoles.OrderBy(x => x.Id))
                                        keyboardList.Add(-(long)item.Id, $"Убрать {item.Name}");
                                    keyboardList.Add(0, $"Скрыть");
                                }

                                await AbotClient.SendMessage(Aupdate?.CallbackQuery?.Message?.Chat?.Id ?? 0,
                                    $"Добавьте роль пользователю: {vSender.UserName}/{vSender.User_Ident}", replyMarkup:
                                    HandleUpdatesUtils.GetKeyBoard(keyboardList, _roleuserskeybord));
                            }
                            break;
                        case _roleuserskeybord:
                            var vSenderId = vuser?.Sender_Id;
                            var vAction = int.Parse(strs[1]);
                            if (vSenderId == null)
                                await AbotClient.SendMessage(Aupdate?.CallbackQuery?.Message?.Chat?.Id ?? 0,
                                    $"Не выбран пользователь");
                            else
                            {
                                if (vAction < 0)
                                    UserQuery?.DropRole(vSenderId, (RolesEnum)(-vAction));
                                else
                                if (vAction > 0)
                                    UserQuery?.AddRole(vSenderId, (RolesEnum)(vAction));
                                else
                                {
                                    UserQuery?.HideFromNewUser(vSenderId);
                                    _HandleUpdatesUtils?.HideInlineKeyboard(AbotClient, Aupdate);
                                }
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
