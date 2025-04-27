using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace MyTelegramBot.secure
{
    internal class MyAppConfig
    {
        public string telegramApiKey;

        protected void WriteDefaultConfig(string AFileName)
        {
            Dictionary<string, Dictionary<string, string>> vConfigData = new Dictionary<string, Dictionary<string, string>>();
            vConfigData["Telegram"] = new Dictionary<string, string>();
            vConfigData["Telegram"]["ApiKey"] = "ApiKey";

            string jsonString = JsonConvert.SerializeObject(vConfigData, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(AFileName, jsonString);
        }

        public bool ReadConfig()
        {
            var vConfigFilePath = AppContext.BaseDirectory + "..\\..\\..\\";
            var vConfigFileName = "appsettings.json";
            if (!File.Exists(vConfigFilePath + vConfigFileName))
                WriteDefaultConfig(vConfigFilePath + vConfigFileName);

            var config = new ConfigurationBuilder()
                .SetBasePath(vConfigFilePath)
                .AddJsonFile(vConfigFileName)
                .Build();

            telegramApiKey = config["Telegram:ApiKey"];

            if (string.IsNullOrEmpty(telegramApiKey))
                return false;

            return true;
        }
    }
}
