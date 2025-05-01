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

namespace HomeWork24
{
    internal class Program
    {

        public static async Task Main()
        {
            MyBot.procShowMessage = Console.WriteLine;

          MyAppConfig appConfig = new();
          if (!appConfig.ReadConfig())
            return;

            MyBot.ConnectionString = appConfig.ConnectionString;
            MyBot.Query = new pgQuery();
            await MyBot.RunBot(appConfig.TelegramApiKey);
            
        }

    }
}
