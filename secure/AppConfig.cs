using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace MyTelegramBot.secure
{
    internal class MyAppConfig
    {
        public string TelegramApiKey;
        public string ConnectionString;

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
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                char correctSeparator = isWindows ? '\\' : '/';
                vConfigFilePath = vConfigFilePath.Replace('\\', correctSeparator).Replace('/', correctSeparator);
                if (!File.Exists(vConfigFilePath + vConfigFileName))
                {
                    WriteDefaultConfig(vConfigFilePath + vConfigFileName);
                }
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(vConfigFilePath)
                .AddJsonFile(vConfigFileName)
                .Build();

            TelegramApiKey = config["Telegram:ApiKey"];
            ConnectionString = config["Telegram:ConnectionString"];

            if (string.IsNullOrEmpty(TelegramApiKey))
                return false;

            return true;
        }
    }
}
