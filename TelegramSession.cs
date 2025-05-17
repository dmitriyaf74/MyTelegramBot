using Microsoft.VisualBasic;
using MyTelegramBot.HandleUpdates;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;
using MyTelegramBot.Classes;
using System;




public class TelegramSession
{
    private TelegramBotClient botClient;
    private CancellationTokenSource cts;

    public UpdateReceivedDelegate? UpdRecDelegate = null;
    public UpdateCallBackDelegate? UpdCallBackDelegate = null;

    //public async Task OnMessageHandler1(Message message, UpdateType type)
    public TelegramSession(string botToken)
    {
        // Создаем экземпляр клиента Telegram Bot API с использованием предоставленного токена
        botClient = new TelegramBotClient(botToken);
        botClient.DropPendingUpdates();

        //Telegram.Bot.Polling.DefaultUpdateReceiver.ReceiveAsync(IUpdateHandler updateHandler, CancellationToken cancellationToken)
        cts = new CancellationTokenSource();
        //botClient.OnMessage = OnMessageHandler1;
    }


    public async Task StartReceiving()
    {
        // Настраиваем параметры получения обновлений
        var receiverOptions = new ReceiverOptions()
        {
            //AllowedUpdates = { }, // Получать все типы обновлений
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
            //ThrowPendingUpdates = true // Сбрасывать необработанные обновления при запуске
        };
        //botClient.OnMessage += this.HandleMessageAsync1;

        // Запускаем получение обновлений в асинхронном режиме

        await Task.Run(() =>
        {
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );
        });

        Console.WriteLine($"Бот запущен и готов к работе.");
    }

    //public static async Task HideInlineKeyboard(long chatId, int messageId)
    //{
    //    // Создаем объект EditMessageReplyMarkupArgs для редактирования сообщения.
    //    var editMessageReplyMarkup = new EditMessageReplyMarkupArgs(chatId, messageId)
    //    {
    //        // Указываем, что reply_markup должен быть null, чтобы убрать клавиатуру.
    //        ReplyMarkup = null
    //    };

    //    // Используем метод EditMessageReplyMarkupAsync для редактирования сообщения.
    //    await botClient.EditMessageReplyMarkupAsync(editMessageReplyMarkup);
    //}

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обработка полученного обновления (например, сообщения)
        if ((update.Type == UpdateType.CallbackQuery) && (update.CallbackQuery is { } callbackQuery))
        {
            //var data = callbackQuery.Data;

            Console.WriteLine($"Получено сообщение: {callbackQuery.Data}-{callbackQuery?.Message?.Text} от {callbackQuery?.Message?.Chat.Id}");
            if (UpdCallBackDelegate != null)
            {
                foreach (UpdateCallBackDelegate del in UpdCallBackDelegate.GetInvocationList())
                {
                    if (!del(botClient, update, cancellationToken))
                        break; 
                }
            }            
        }
        else
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
                await UpdRecDelegate(botClient, update, cancellationToken);
                //if (!HandleUpdatesUser.BeginUpdate)
                {
                    //ReplyKeyboardRemove removeKeyboard = new ReplyKeyboardRemove();
                    //await botClient.SendTextMessageAsync(
                    //        chatId: message.Chat.Id,
                    //        text: $"Неизвестная команда {update.Message.Text}",
                    //        cancellationToken: cancellationToken,
                    //    replyMarkup: removeKeyboard);
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

    public async Task StopReceiving()
    {
        // Останавливаем получение обновлений
        await cts.CancelAsync();
        Console.WriteLine("Бот остановлен.");
    }
}
