using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MyTelegramBot.DapperClasses;
using MyTelegramBot.secure;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MyTelegramBot.Classes;
using MyTelegramBot.HandleUpdates;
using Microsoft.Extensions.Logging;
//pg_dump -U "postgres" -d "TelegramHelper" -s -f "d:\test\TelegramHelper.sql"

namespace HomeWork24
{
    internal class Program
    {
        private static async Task RegHandleUpdates(CustomHandleUpdates HandleUpdates, CustomQuery Query, Logger logger, TelegramSession telegramSession)
        {
            await Task.Run(() =>
            {
                HandleUpdates.Query = Query;
                HandleUpdates.procShowMessage += Console.WriteLine;
                HandleUpdates.procShowMessage += logger.Log;

                HandleUpdates.RegisterHandlesUpdates(ref telegramSession.UpdRecDelegate);
                HandleUpdates.RegisterCallBackUpdates(ref telegramSession.UpdCallBackDelegate);
            });
        }
        public static async Task Main()
        {
            Logger logger = new Logger("application.log");

            MyAppConfig appConfig = new();
            if (!appConfig.ReadConfig())
              return;
            var telegramSession = new TelegramSession(appConfig.TelegramApiKey);
            var pgUseer = new pgQueryUser(appConfig.ConnectionString);

            await RegHandleUpdates(new HandleUpdatesUser(), pgUseer, logger, telegramSession);
            await RegHandleUpdates(new HandleUpdatesAll(), pgUseer, logger, telegramSession);

            await telegramSession.StartReceiving();
            Console.ReadLine();
            await telegramSession.StopReceiving();


            //MyBot.ConnectionString = appConfig.ConnectionString;
            //MyBot.Query = new pgQuery();
            //await MyBot.RunBot(appConfig.TelegramApiKey);

        }

    }
}
