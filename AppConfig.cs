using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MyTelegramBot.secure
{
    internal class MyAppConfig
    {
        public string telegramApiKey;

        public bool ReadConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory + "..\\..\\..\\")
                .AddJsonFile("appsettings.json")
                .Build();

            telegramApiKey = config["Telegram:ApiKey"];

            if (string.IsNullOrEmpty(telegramApiKey))
            {
                return false;
            }

            return true;
        }
    }
}
