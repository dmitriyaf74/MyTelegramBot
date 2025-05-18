using MyTelegramBot.Classes;
using MyTelegramBot.DapperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesAdmin : CustomHandleUpdates
    {

        private  TelegramSession BotSession;
        public HandleUpdatesAdmin(TelegramSession ABotSession, HandleUpdatesMain AHandleUpdatesUtils, pgQueryUser AUserQuery)
        {
            BotSession = ABotSession;
            _HandleUpdatesUtils = AHandleUpdatesUtils;
            UserQuery = AUserQuery;
        }
        private pgQueryUser? UserQuery;

        private HandleUpdatesMain _HandleUpdatesUtils;
        public HandleUpdatesMain HandleUpdatesUtils { get => _HandleUpdatesUtils; }

        private enum AdminButtons
        {
            _ShowNewUsers,
            _ShowInfo,
        }
        private const string _adminkeybord = "adminkeybord";

        protected override async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
        {
            if (ARole_Id == RolesEnum.reAdmin)
                await ShowAdminButtons(AbotClient, Aupdate, 0);
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
                                  HandleUpdatesUtils.GetKeyBoard(keyboardList, _adminkeybord));
        }



        /// <summary>
        /// Admin Buttons
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
                        case _adminkeybord:
                            await HandleUpdatesUtils.HideInlineKeyboard(AbotClient, Aupdate);
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
    }
}
