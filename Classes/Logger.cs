using System;
using System.IO;

namespace MyTelegramBot.Classes
{
    public class Logger
    {
        private string logFilePath;

        public Logger(string filePath)
        {
            logFilePath = filePath;
        }

        public void Log(string message)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}";
                    writer.WriteLine(logEntry);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка записи в лог-файл: {e.Message}");
            }
        }
    }
}
