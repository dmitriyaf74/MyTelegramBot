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
using Telegram.Bot.Types.ReplyMarkups;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesOperator : CustomHandleUpdates
    {
        private TelegramSession BotSession;
        public HandleUpdatesOperator(TelegramSession ABotSession, HandleUpdatesMain AHandleUpdatesUtils, pgQueryUser AUserQuery)
        {
            BotSession = ABotSession;
            _HandleUpdatesUtils = AHandleUpdatesUtils;
            UserQuery = AUserQuery;
        }
        private pgQueryUser? UserQuery;

        private HandleUpdatesMain _HandleUpdatesUtils;
        public HandleUpdatesMain HandleUpdatesUtils { get => _HandleUpdatesUtils; }


        private const string cstrGetOldestMessage = "/Получить обращение";
        private const string cstrGetEstimation = "/Оценить оператора";
        private const string cstrGetCloseDialog = "/Завершить диалог";
        private const string _operatorquerysbord = "operatorquerysbord";
        private enum OperatorButtons
        {
            _QueryOldestMessage,
            _Queryestimation,
        }

        protected override async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
        {
            if (ARole_Id == RolesEnum.reOperator)
                await ShowOperatorButtons(AbotClient, Aupdate, 0);
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

        protected override async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;
            var vuser = HandleUpdatesUtils.GetUser(Aupdate);

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
                                        vMessage = $"{IsNewStr}{item.Date_Time} Оператор: {vAnswerer.UserName} \n{item.MessageStr}";
                                    else
                                        vMessage = $"{IsNewStr}{item.Date_Time} Пользователь: {vUser?.UserName} \n{item.MessageStr}";
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

        protected override async Task UpdateCallBackKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null))
                {
                    switch (strs[0])
                    {
                        case _operatorquerysbord:
                            await HandleUpdatesUtils.HideInlineKeyboard(AbotClient, Aupdate);
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
