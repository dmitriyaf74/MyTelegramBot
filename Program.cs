using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using MyTelegramBot.Classes;
using MyTelegramBot.DapperClasses;
using MyTelegramBot.HandleUpdates;
using MyTelegramBot.Interfaces;
using MyTelegramBot.secure;
using MyTelegramBot.VirtDBClasses;
using Npgsql;
using System;
using System.Data.Common;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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

        public static bool DatabaseExists(string connectionString, string databaseName)
        {
            try
            {
                // Создаем строку подключения к postgres, но без указания конкретной базы данных.
                // Используем "postgres" в качестве базы данных по умолчанию для подключения.
                NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connectionString);
                string originalDatabase = builder.Database; // Сохраняем оригинальное имя базы данных
                builder.Database = "postgres"; // Подключаемся к базе данных postgres для проверки
                string connectionStringWithoutDatabase = builder.ConnectionString;


                using (var connection = new NpgsqlConnection(connectionStringWithoutDatabase))
                {
                    connection.Open();

                    // Выполняем запрос для проверки существования базы данных.
                    using (var cmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname='{databaseName}'", connection))
                    {
                        object result = cmd.ExecuteScalar();

                        // Если запрос вернул 1, значит, база данных существует.
                        return result != null && result.ToString() == "1";
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка исключений, например, если не удалось подключиться к серверу.
                Console.WriteLine($"Ошибка при проверке базы данных: {ex.Message}");
                return false;
            }
        }
        public static async Task Main()
        {
            Logger logger = new Logger("application.log");

            MyAppConfig appConfig = new();
            if (!appConfig.ReadConfig())
              return;

            //string connectionString = appConfig.ConnectionString;
            string connectionString = "Host=localhost;Database=TelegramHelper;Username=postgres;Password=postgres";
            string dbName = "";
            string[] con = connectionString.Split(';');
            foreach (string s in con) 
            {
                string[] param = s.Split("=");
                if ((param.Length > 0) && (param[0] == "Database"))
                {
                    dbName = param[1];
                    break;
                }
            }

            ICustomQuery iCustomQuery;
            if (DatabaseExists(connectionString, dbName))
                iCustomQuery = new pgQuery(connectionString);
            else
            {
                VirtDB.FillDB();
                iCustomQuery = new lstQuery("");
            }

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
