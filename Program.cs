using System;
using MyTelegramBot;
using MyTelegramBot.secure;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeWork24
{
    internal class Program
    {

        public static async Task Main()
        {
            if (!MyBot.appConfig.ReadConfig())
            {
                return;
            }

            MyBot.procShowMessage = Console.WriteLine;

            var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(MyBot.appConfig.telegramApiKey);

            var me = await bot.GetMe(); //Информация о боте
            Console.WriteLine($"Hello, {me.FirstName} {me.LastName} with {me.Username} an id {me.Id}!");
            bot.StartReceiving(MyBot.UpdateReceived, MyBot.ExceptionHandler, new Telegram.Bot.Polling.ReceiverOptions()
            {
                AllowedUpdates = [Telegram.Bot.Types.Enums.UpdateType.Message]
            }, cts.Token);
            Console.ReadLine();
            await cts.CancelAsync();
        }

    }
}
