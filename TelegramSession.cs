using Microsoft.VisualBasic;
using MyTelegramBot.Classes;
using MyTelegramBot.DapperClasses;
using MyTelegramBot.HandleUpdates;
using System;
using System.Data;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;



internal class TelegramSession
{
    private TelegramBotClient botClient;
    private CancellationTokenSource cts;

    public UpdateReceivedDelegate? UpdRecDelegate = null;
    public UpdateCallBackDelegate? UpdCallBackDelegate = null;


    static pgQueryUser? QueryUser { get; set; }

    public Dictionary<int, string?>? Roles = new();

    public ProcShowMessage? procShowMessage { get; set; }
    protected void DoConShowMessage(string message)
    {
        if (procShowMessage is not null)
            procShowMessage(message);
    }
    public ProcShowError? procShowError { get; set; }
    protected void DoConShowError(string message)
    {
        if (procShowError is not null)
            procShowError(message);
    }

    private void InitRoleList()
    {
        if (Roles == null)
            return; 
        Roles.Clear();
        var rList = QueryUser?.GetAllRoles();
        if (rList != null)
            foreach (var r in rList)
                Roles.Add((int)r.Id, r.Name);
    }

    
    public TelegramSession(string botToken, pgQueryUser? AQueryUser)
    {
        // Создаем экземпляр клиента Telegram Bot API с использованием предоставленного токена
        botClient = new TelegramBotClient(botToken);
        botClient.DropPendingUpdates();

        //Telegram.Bot.Polling.DefaultUpdateReceiver.ReceiveAsync(IUpdateHandler updateHandler, CancellationToken cancellationToken)
        cts = new CancellationTokenSource();

        QueryUser = AQueryUser;
        InitRoleList();
    }


    public async Task StartReceiving()
    {
        // Настраиваем параметры получения обновлений
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
        };

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

        DoConShowMessage($"Бот запущен и готов к работе.");
    }
        
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обработка полученного обновления (например, сообщения)
        if ((update.Type == UpdateType.CallbackQuery) && (update.CallbackQuery is { } callbackQuery))
        {
            DoConShowMessage($"Получено сообщение: {callbackQuery.Data}-{callbackQuery?.Message?.Text} от {callbackQuery?.Message?.Chat.Id}");
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
            DoConShowMessage($"Получено сообщение: {message.Text} от {message.Chat.Id}");
            if (UpdRecDelegate != null) 
                await UpdRecDelegate(botClient, update, cancellationToken);
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

        DoConShowError(errorMessage);
        return Task.CompletedTask;
    }

    public async Task StopReceiving()
    {
        // Останавливаем получение обновлений
        await cts.CancelAsync();
        DoConShowMessage("Бот остановлен.");
    }
}
