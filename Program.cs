using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using MyTelegramBot.Classes;
using MyTelegramBot.DapperClasses;
using MyTelegramBot.HandleUpdates;
using MyTelegramBot.Interfaces;
using MyTelegramBot.secure;
using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MyTelegramBot.VirtDBClasses;
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

            //ICustomQuery iCustomQuery = new pgQuery(appConfig.ConnectionString);
            VirtDB.FillDB();
            ICustomQuery iCustomQuery = new lstQuery("");

            var telegramSession = new TelegramSession(appConfig.TelegramApiKey, iCustomQuery);
            telegramSession.procShowMessage += Console.WriteLine;
            telegramSession.procShowError += Console.WriteLine;
            telegramSession.procShowMessage += logger.Log;


            gHandleUpdatesAll = new(ABotSession: telegramSession);
            await RegHandleUpdates(gHandleUpdatesAll, logger, telegramSession);

            gHandleUpdatesAdmin = new(telegramSession, gHandleUpdatesAll);
            await RegHandleUpdates(gHandleUpdatesAdmin, logger, telegramSession);

            gHandleUpdatesOperator = new(telegramSession, gHandleUpdatesAll);
            await RegHandleUpdates(gHandleUpdatesOperator, logger, telegramSession);

            gHandleUpdatesUser = new(telegramSession, gHandleUpdatesAll);
            await RegHandleUpdates(gHandleUpdatesUser, logger, telegramSession);


            await telegramSession.StartReceiving();
            Console.ReadLine();
            await telegramSession.StopReceiving();
        }

    }
}
