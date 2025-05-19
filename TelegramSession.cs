using Microsoft.VisualBasic;
using MyTelegramBot.Classes;
using MyTelegramBot.HandleUpdates;
using MyTelegramBot.Interfaces;
using System;
using System.Collections.Concurrent;
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
    public TelegramSession(string botToken, ICustomQuery AQueryUser)
    {
        // Создаем экземпляр клиента Telegram Bot API с использованием предоставленного токена
        botClient = new TelegramBotClient(botToken);
        botClient.DropPendingUpdates();

        //Telegram.Bot.Polling.DefaultUpdateReceiver.ReceiveAsync(IUpdateHandler updateHandler, CancellationToken cancellationToken)
        cts = new CancellationTokenSource();

        _QueryUser = AQueryUser;
        InitRoleList();
    }
    private ICustomQuery _QueryUser { get; set; }
    public ICustomQuery QueryUser { get => _QueryUser; }

    private TelegramBotClient botClient;
    private CancellationTokenSource cts;

    public UpdateReceivedDelegate? UpdRecDelegate = null;
    public UpdateCallBackDelegate? UpdCallBackDelegate = null;
    public DoGetMenuRoleDelegate? DoGetMenuRole = null;



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
    
    public async Task DoGetMenuRolesDelegates(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
    {
        if (DoGetMenuRole != null)
            await DoGetMenuRole(AbotClient, Aupdate, ARole_Id);
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
                    await del(botClient, update, cancellationToken);
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
