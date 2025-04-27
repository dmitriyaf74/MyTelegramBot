using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
            var vConfigFileName = "appsettings.json";
            var vConfigFilePath = AppContext.BaseDirectory;
            if (!File.Exists(vConfigFilePath + vConfigFileName))
            {
                vConfigFilePath = vConfigFilePath + "..\\..\\..\\";
                if (!File.Exists(vConfigFilePath + vConfigFileName))
                {
                    WriteDefaultConfig(vConfigFilePath + vConfigFileName);
                }
            }

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
