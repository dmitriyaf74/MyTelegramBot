using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


public delegate bool UpdateReceivedDelegate(ITelegramBotClient botClient, Update update, CancellationToken token);

public class TelegramSession
{
    private TelegramBotClient botClient;
    private CancellationTokenSource cts;

    public UpdateReceivedDelegate UpdRecDelegate = null;

    public TelegramSession(string botToken)
    {
        // Создаем экземпляр клиента Telegram Bot API с использованием предоставленного токена
        botClient = new TelegramBotClient(botToken);
        cts = new CancellationTokenSource();
    }

    public async Task StartReceiving()
    {
        // Настраиваем параметры получения обновлений
        var receiverOptions = new ReceiverOptions()
        {
            //AllowedUpdates = { }, // Получать все типы обновлений
            AllowedUpdates = [UpdateType.Message]
            //ThrowPendingUpdates = true // Сбрасывать необработанные обновления при запуске
        };

        // Запускаем получение обновлений в асинхронном режиме

        botClient.StartReceiving(
        updateHandler: HandleUpdateAsync,
        errorHandler: HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token
        );

        Console.WriteLine($"Бот запущен и готов к работе.");
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обработка полученного обновления (например, сообщения)
        if (update.Message is { } message)
        {
            Console.WriteLine($"Получено сообщение: {message.Text} от {message.Chat.Id}");
            //Здесь можно добавить логику для ответа на сообщения
            //await botClient.SendTextMessageAsync(
            //chatId: message.Chat.Id,
            //text: "Вы прислали сообщение",
            //cancellationToken: cancellationToken);

            if (UpdRecDelegate != null) // Важно проверить, что делегат не равен null, иначе будет исключение.
            {
                var res = UpdRecDelegate(botClient, update, cancellationToken);
                if (!res)
                {
                    ReplyKeyboardRemove removeKeyboard = new ReplyKeyboardRemove();

                    await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"Неизвестная команда {update.Message.Text}",
                            cancellationToken: cancellationToken,
                        replyMarkup: removeKeyboard);
                }
            }
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Обработка ошибок при получении обновлений
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
            => $"Ошибка Telegram API: [{apiRequestException.ErrorCode}] {apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.Error.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    public void StopReceiving()
    {
        // Останавливаем получение обновлений
        cts.CancelAsync();
        Console.WriteLine("Бот остановлен.");
    }
}
