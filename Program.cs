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
        private static async Task RegHandleUpdates(CustomHandleUpdates HandleUpdates, Logger logger, TelegramSession telegramSession)
        {
            await Task.Run(() =>
            {
                HandleUpdates.procShowMessage += Console.WriteLine;
                HandleUpdates.procShowError += Console.WriteLine;
                HandleUpdates.procShowMessage += logger.Log;

                HandleUpdates.RegisterHandlesAll(ref telegramSession.UpdRecDelegate,
                    ref telegramSession.UpdCallBackDelegate, ref telegramSession.DoGetMenuRole);
            });
        }

        public static HandleUpdatesMain? gHandleUpdatesAll;
        public static HandleUpdatesAdmin? gHandleUpdatesAdmin;
        public static HandleUpdatesOperator? gHandleUpdatesOperator;
        public static HandleUpdatesUser? gHandleUpdatesUser;
        public static async Task Main()
        {
            Logger logger = new Logger("application.log");

            MyAppConfig appConfig = new();
            if (!appConfig.ReadConfig())
              return;
            var pgUseer = new pgQueryUser(appConfig.ConnectionString);
            var telegramSession = new TelegramSession(appConfig.TelegramApiKey, pgUseer);
            telegramSession.procShowMessage += Console.WriteLine;
            telegramSession.procShowError += Console.WriteLine;
            telegramSession.procShowMessage += logger.Log;


            gHandleUpdatesAll = new(ABotSession: telegramSession, AUserQuery: pgUseer);
            await RegHandleUpdates(gHandleUpdatesAll, logger, telegramSession);

            gHandleUpdatesAdmin = new(telegramSession, gHandleUpdatesAll, pgUseer);
            await RegHandleUpdates(gHandleUpdatesAdmin, logger, telegramSession);

            gHandleUpdatesOperator = new(telegramSession, gHandleUpdatesAll, pgUseer);
            await RegHandleUpdates(gHandleUpdatesOperator, logger, telegramSession);

            gHandleUpdatesUser = new(telegramSession, gHandleUpdatesAll, pgUseer);
            await RegHandleUpdates(gHandleUpdatesUser, logger, telegramSession);


            await telegramSession.StartReceiving();
            Console.ReadLine();
            await telegramSession.StopReceiving();
        }

    }
}
