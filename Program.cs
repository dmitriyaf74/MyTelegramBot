using System;
using MyTelegramBot;
using MyTelegramBot.secure;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeWork24
{
    internal class Program
    {
        private static MyAppConfig appConfig = new MyAppConfig();


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

        private static void UpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken token)
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
                Console.WriteLine(info);
                //botClient.SendMessage(update.Message.Chat.Id, info, cancellationToken: token);
                botClient.SendMessage(update.Message.Chat.Id, info, replyMarkup: buttons);
            }
            else
            {
                Console.WriteLine(update.Message.Text);
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

        private static void ExceptionHandler(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception.Message);
        }

        static async Task Main()
        {
            if (!appConfig.ReadConfig())
            {
                return;
            }

            var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(appConfig.telegramApiKey);

            var me = await bot.GetMe(); //Информация о боте
            Console.WriteLine($"Hello, {me.FirstName} {me.LastName} with {me.Username} an id {me.Id}!");
            bot.StartReceiving(UpdateReceived, ExceptionHandler, new Telegram.Bot.Polling.ReceiverOptions()
            {
                AllowedUpdates = [Telegram.Bot.Types.Enums.UpdateType.Message]
            }, cts.Token);
            Console.ReadLine();
            await cts.CancelAsync();
        }

    }
}
