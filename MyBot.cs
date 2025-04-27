using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyTelegramBot.secure;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MyTelegramBot.DapperClasses;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Reflection;

namespace MyTelegramBot
{
    internal class MyBot
    {
        static string ConnectionString = "Host=localhost;Database=TelegramHelper;Username=postgres;Password=postgres";
        static List<Role> RoleList;
        public static ITelegramBotClient bot;

        public static MyAppConfig appConfig = new MyAppConfig();
        public delegate void ProcShowMessage(string message);

        public static ProcShowMessage procShowMessage {  get; set; }

        private static void DoConShowMessage(string message)
        {
            if (procShowMessage is not null)
                procShowMessage(message);
        }

        enum UserCommand
        {
            None = 0,
            Start = 1

        }

        private static UserCommand UserMessageType(string userMessage)
        {
            if (userMessage == "/start")
            {
                return UserCommand.Start;
            }
            else
            {
                return UserCommand.None;
            }
        }

        public static void SelectRole(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var buttons = new ReplyKeyboardMarkup();
            buttons.ResizeKeyboard = true;

            var info = "Выберите доступную роль";
            foreach (var item in RoleList)
            {
                buttons.AddButton(new KeyboardButton($"{item.Id}.{item.Name}"));
            }
            botClient.SendMessage(update.Message.Chat.Id, info, replyMarkup: buttons);
        }

        public async Task DisableButtons(long chatId, int messageId)
        {
            // Создаем пустую ReplyMarkup, чтобы убрать кнопки
            var buttons = new ReplyKeyboardRemove();
            //bot.SendMessage(chatId: chatId, "messageId: messageId", replyMarkup: buttons);

            // Редактируем сообщение, чтобы убрать кнопки. Параметр replyMarkup: replyMarkup
            //await bot.EditMessageReplyMarkupAsync(
            //    chatId: chatId,
            //    messageId: messageId,
            //    replyMarkup: buttons); // Используем replyMarkup
        }

        public static void UpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var buttons = new ReplyKeyboardMarkup(
                new[] {
                    new[]{
                        new KeyboardButton("50") ,
                        new KeyboardButton("125"),
                        new KeyboardButton("250")
                    },
                    new[] {
                        new KeyboardButton("500"),
                        new KeyboardButton("625"),
                        new KeyboardButton("999")
                    }
                });
            buttons.ResizeKeyboard = true;

            if (update.Type != UpdateType.Message)
            {
                return; //311068358
            }

            if (UserMessageType(update.Message.Text) == UserCommand.Start)
            {
                var info = $"{update.Message.From.Username}({update.Message.From.Id})({update.Message.From.FirstName} {update.Message.From.LastName}) : {update.Message.Text}";
                DoConShowMessage(info);
                //botClient.SendMessage(update.Message.Chat.Id, info, cancellationToken: token);
                //botClient.SendMessage(update.Message.Chat.Id, info, replyMarkup: buttons);

                DapperClasses.User vuser = QueryUsers.SelectUser(ConnectionString, update.Message.From.Id);
                var vUserId = vuser.Id;
                if (vuser == null) 
                {
                    vuser = new DapperClasses.User();
                    vuser.UserName = update.Message.From.Username;
                    vuser.User_Ident = update.Message.From.Id;
                    vuser.FirstName = update.Message.From.FirstName;
                    vuser.LastName = update.Message.From.LastName;
                    vuser.Roles_id = 0;
                    vUserId = QueryUsers.InsertUser(ConnectionString, vuser);
                }
                if (RoleList is null)
                    RoleList = QueryRoles.SelectRoles(ConnectionString, vUserId);

                SelectRole(botClient, update, token);
            }
            else
            {
                var rbuttons = new ReplyKeyboardRemove();
                botClient.SendMessage(chatId: update.Message.Chat.Id, "messageId: messageId", replyMarkup: rbuttons);


                //await DisableButtons(update.Message.Chat.Id, "sentMessage.MessageId");
                DoConShowMessage(update.Message.Text);
                botClient.SendMessage(update.Message.Chat.Id, update.Message.Text, cancellationToken: token);
            }

        }

        public static void ExceptionHandler(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            DoConShowMessage(exception.Message);
        }


        //public static async Task RunBot()
        //{
        //    if (!appConfig.ReadConfig())
        //    {
        //        return;
        //    }

        //    var cts = new CancellationTokenSource();
        //    var bot = new TelegramBotClient(appConfig.telegramApiKey);

        //    var me = await bot.GetMe(); //Информация о боте
        //    Console.WriteLine($"Hello, {me.FirstName} {me.LastName} with {me.Username} an id {me.Id}!");
        //    bot.StartReceiving(UpdateReceived, ExceptionHandler, new Telegram.Bot.Polling.ReceiverOptions()
        //    {
        //        AllowedUpdates = [Telegram.Bot.Types.Enums.UpdateType.Message]
        //    }, cts.Token);
        //    Console.ReadLine();
        //    await cts.CancelAsync();
        //}
    }
}
