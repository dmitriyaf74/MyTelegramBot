using MyTelegramBot.Classes;
using MyTelegramBot.Interfaces;
using Newtonsoft.Json.Linq;
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
        public HandleUpdatesOperator(TelegramSession ABotSession, HandleUpdatesMain AHandleUpdatesUtils)
        {
            BotSession = ABotSession;
            _HandleUpdatesUtils = AHandleUpdatesUtils;
        }
        private ICustomQuery? UserQuery { get => BotSession?.QueryUser; }

        private HandleUpdatesMain _HandleUpdatesUtils;
        public HandleUpdatesMain HandleUpdatesUtils { get => _HandleUpdatesUtils; }


        private const string cstrGetOldestMessage = "/Получить обращение";
        private const string cstrGetCloseDialog = "/Завершить диалог";
        private const string cstrGetDelayDialog = "/Отложить диалог";
        private const string cstrGetDelayedDialogs = "/Отложенные диалоги";
        private const string cstrStart = "/start";
        private const string _delayedchatsbord = "delayedchatsbord";

        protected override async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
        {
            if (ARole_Id == RolesEnum.reOperator)
                await ShowOperatorButtons(AbotClient, Aupdate, 0);
        }
        private async Task ShowOperatorButtons(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            var info = "Выберите действие: ";

            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton(cstrGetOldestMessage), new KeyboardButton(cstrGetCloseDialog) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(cstrGetDelayDialog), new KeyboardButton(cstrGetDelayedDialogs) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(cstrStart) });
            var keyboard = new ReplyKeyboardMarkup(buttons);

            long? vChatId = null;
            if (Aupdate?.Message is not null)
                vChatId = Aupdate.Message?.Chat?.Id;
            else
            if (Aupdate?.CallbackQuery?.Message?.Chat is not null)
                vChatId = Aupdate.CallbackQuery.Message.Chat.Id;

            if (vChatId != null)
                await AbotClient.SendMessage(vChatId, info, replyMarkup: keyboard);
        }

        private async Task ShowDelayedChats(ITelegramBotClient AbotClient, Update Aupdate, List<DelayedChats> ADelayedChats)
        {
            var keyboardRows = new List<List<InlineKeyboardButton>>();
            foreach (var item in ADelayedChats)
            {
                var vChat = UserQuery?.SelectUserById(item.Chat_Id);
                var vUser = UserQuery?.SelectUserById(item.User_Id);
                if (vChat == null || vUser == null)
                    continue;
                var button = new InlineKeyboardButton($"{vChat.UserName}/{vChat.FirstName}/{vChat.User_Ident}" +
                    $"---{vUser.UserName}/{vUser.FirstName}/{vUser.User_Ident}", 
                    $"{_delayedchatsbord}.{item.Chat_Id}");
                var row = new List<InlineKeyboardButton> { button };
                keyboardRows.Add(row);
            }

            var vText = keyboardRows.Count > 0 ? "Выберите чат для активации:" : "Нет отложенных диалогов";
            var vChatId = Aupdate?.Message is not null ? Aupdate.Message.Chat.Id : Aupdate?.CallbackQuery?.Message?.Chat?.Id;
            if (vChatId is not null)
                await AbotClient.SendMessage(vChatId, vText, replyMarkup: new InlineKeyboardMarkup(keyboardRows));
        }

        protected override async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;
            var vuser = UserQuery?.SelectUserByIdent(Aupdate?.Message?.From?.Id);
            if (vuser?.Roles_id != RolesEnum.reOperator)
                return;

            if (Aupdate?.Message?.Text?[0] == '/')
            {
                switch (Aupdate.Message.Text)
                {
                    //Получить все сообщения самого давнего обратившегося
                    case cstrGetOldestMessage:
                        {
                            if (Aupdate.Message.From is null)
                                return;


                            long? vMessageUserId = null;
                            long? vTopycId = null;
                            if (UserQuery != null)
                                (vMessageUserId, vTopycId) = UserQuery.GetOldestMessageUserId();
                            var vUserMessages = UserQuery?.GetUserMessages(vMessageUserId);
                            if (vUserMessages?.Count > 0)
                            {
                                var vMessage = "";
                                var IsNewStr = "";
                                var vSender = UserQuery?.SelectUserById(vUserMessages[0].User_Id);
                                var vTopics = UserQuery?.GetTopics();
                                foreach (var item in vUserMessages)
                                {
                                    var vAnswerer = UserQuery?.SelectUserById(vUserMessages[0].Answerer_Id);
                                    if (item.IsNew)
                                        IsNewStr = "Новое\n";
                                    else
                                        IsNewStr = "";

                                    var vTopicName = vTopics != null 
                                        ? vTopics?.Where(x => x.Id == item.Topic_Id).Select(x => x.Name)?.FirstOrDefault()?.ToString() + "\n"
                                        : "";

                                    if (vAnswerer != null)
                                        vMessage = $"{vTopicName}{IsNewStr}{item.Date_Time} Оператор: {vAnswerer.UserName} \n{item.MessageStr}";
                                    else
                                        vMessage = $"{vTopicName}{IsNewStr}{item.Date_Time} Пользователь: {vSender?.UserName} \n{item.MessageStr}";

                                    await AbotClient.SendMessage(Aupdate.Message.Chat.Id, vMessage);
                                }
                                UserQuery?.SetActiveSenderId(vuser?.Id, vUserMessages[0].User_Id);
                            }
                            else
                            {
                                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Нет непрочитанных сообщений");
                                if (vuser is not null)
                                    UserQuery?.SetActiveSenderId(vuser.Id, null);
                            }
                            break;
                        }
                    case cstrGetCloseDialog:
                        if ((vuser is not null) && (Aupdate?.Message?.Text is not null))
                        {
                            var vMessage = "Спасибо за обращение. В любой момент вы можете обратиться снова";
                            var vsender = UserQuery?.SelectUserById(vuser.Sender_Id);
                            //Отправляем сообщение пользователю
                            if (vsender?.User_Ident != null)
                                await AbotClient.SendMessage(vsender.User_Ident, vMessage);
                            //Отправляем сообщение оператору
                            if (vsender?.User_Ident != Aupdate.Message.Chat.Id)
                                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, vMessage);
                            //Закрываем диалог
                            UserQuery?.CloseActiveMessages(vuser.Sender_Id);
                        }
                        break;
                    case cstrGetDelayDialog:
                        if ((vuser is not null) && (Aupdate?.Message is not null))
                        {
                            if (vuser.Sender_Id != null)
                            {
                                UserQuery?.DelayChat(vuser.Id, vuser.Sender_Id);
                                UserQuery?.SetActiveSenderId(vuser.Id, null);
                                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Диалог отложен");
                            }
                        }
                        break;
                    case cstrGetDelayedDialogs:
                        if (vuser is not null)
                        {
                            var vDelayedChats = UserQuery?.GetDelayedChats(vuser.Id);
                            if (vDelayedChats != null)
                                await ShowDelayedChats(AbotClient, Aupdate, vDelayedChats);
                        }
                        break;
                    case cstrStart:
                        break;
                    default:
                        break;

                }
            }
            else
            {
                if (vuser?.Sender_Id is null)
                    throw new ArgumentNullException("Отправитель", "Значение не указано.");

                UserQuery?.AddMessage(vuser.Sender_Id, Aupdate?.Message?.Text, vuser?.Topic_id, vuser?.Id);
                if ((vuser is not null) && (Aupdate?.Message?.Text is not null))
                {
                    var vsender = UserQuery?.SelectUserById(vuser.Sender_Id);
                    //Отправляем сообщение пользователю
                    if (vsender?.User_Ident != null)
                        await AbotClient.SendMessage(vsender.User_Ident, Aupdate.Message.Text);
                    //Отправляем сообщение оператору
                    if (vsender?.User_Ident != Aupdate.Message.Chat.Id)
                        await AbotClient.SendMessage(Aupdate.Message.Chat.Id, Aupdate.Message.Text);
                }
                DoConShowMessage($"Оператор");
            }

        }

        protected override async Task UpdateCallBackKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var vuser = UserQuery?.SelectUserByIdent(Aupdate?.CallbackQuery?.From?.Id) ?? new();
                var strs = Aupdate?.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null) && (Aupdate?.CallbackQuery.Message.Chat.Id != null))
                {
                    switch (strs[0])
                    {                        
                        case _delayedchatsbord:
                            UserQuery?.ResumeChat(int.Parse(strs[1]));
                            UserQuery?.SetActiveSenderId(vuser.Id, int.Parse(strs[1]));
                            _HandleUpdatesUtils?.HideInlineKeyboard(AbotClient, Aupdate);
                            await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, "Диалог восстановлен");
                            break;
                        default:
                            break;
                    }
                }

            }
        }
    }
}
