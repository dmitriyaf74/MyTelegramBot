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

namespace MyTelegramBot
{
    internal class MyBot
    {
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
                return;
            }

            if (UserMessageType(update.Message.Text) == UserCommand.Start)
            {
                var info = $"{update.Message.From.Username} : {update.Message.Text}";
                DoConShowMessage(info);
                //botClient.SendMessage(update.Message.Chat.Id, info, cancellationToken: token);
                botClient.SendMessage(update.Message.Chat.Id, info, replyMarkup: buttons);
            }
            else
            {
                DoConShowMessage(update.Message.Text);
                botClient.SendMessage(update.Message.Chat.Id, update.Message.Text, cancellationToken: token);
            }

            ////var msg:string = update.Message?.ToString() ?? "Error";
            ////var msg:string? = update.Message != null ? update.Message.ToString() ?? "Error";
            //if (update?.Message?.From is null)
            //{
            //    Console.WriteLine("Message is null");
            //    return;
            //}

            //var info = $"{update.Message.From.Username} : {update.Message.Text}";
            //Console.WriteLine(info);
            //botClient.SendMessage(update.Message.Chat.Id, info, cancellationToken: token);
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
