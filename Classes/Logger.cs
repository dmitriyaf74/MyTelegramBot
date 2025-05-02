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
                // Открываем файл для добавления записи.  Если файла нет, он будет создан.
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    // Формируем строку для записи в лог, добавляя текущую дату и время.
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}";

                    // Записываем строку в файл.
                    writer.WriteLine(logEntry);

                    // writer.Flush();  // Можно добавить для немедленной записи на диск (не обязательно).
                }
            }
            catch (Exception ex)
            {
                // Обрабатываем возможные исключения, например, отсутствие доступа к файлу.
                Console.WriteLine($"Ошибка записи в лог-файл: {ex.Message}");
            }
        }
    }
}
