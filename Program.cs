using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MyTelegramBot;
using MyTelegramBot.DapperClasses;
using MyTelegramBot.secure;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MyTelegramBot.Classes;

namespace HomeWork24
{
    internal class Program
    {

        /*public static void Method1(string message)
        {
            Console.WriteLine("Method1: " + message);
        }

        public static void Method2(string message)
        {
            Console.WriteLine("Method2: " + message);
        }*/

        public static async Task Main()
        {
            Logger logger = new Logger("application.log");

            MyBot.procShowMessage = Console.WriteLine;

            MyAppConfig appConfig = new();
            if (!appConfig.ReadConfig())
              return;
            var telegramSession = new TelegramSession(appConfig.TelegramApiKey);

            //HandleUpdates handleUpdates = new HandleUpdates();
            HandleUpdates.Query = new pgQuery(appConfig.ConnectionString);
            HandleUpdates.procShowMessage += Console.WriteLine;
            HandleUpdates.procShowMessage += logger.Log;

            HandleUpdates.RegisterHandlesUpdates(ref telegramSession.UpdRecDelegate);

            telegramSession.StartReceiving();
            Console.ReadLine();
            telegramSession.StopReceiving();


            //MyBot.ConnectionString = appConfig.ConnectionString;
            //MyBot.Query = new pgQuery();
            //await MyBot.RunBot(appConfig.TelegramApiKey);

        }

    }
}
