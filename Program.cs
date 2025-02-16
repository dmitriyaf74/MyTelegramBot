using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeWork24
{
    internal class Program
    {
        private static void UpdateReceived(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            //var msg:string = update.Message?.ToString() ?? "Error";
            //var msg:string? = update.Message != null ? update.Message.ToString() ?? "Error";
            if (update?.Message?.From is null)
            {
                Console.WriteLine("Message is null");
                return;
            }

            var info = $"{update.Message.From.Username} : {update.Message.Text}";
            Console.WriteLine(info);
            botClient.SendMessage(update.Message.Chat.Id, info, cancellationToken: token);
        }

        static async Task Main()
        {
            var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient("");

            var me = await bot.GetMe(); //Информация о боте
            Console.WriteLine($"Hello, {me.FirstName} {me.LastName} with {me.Username} an id {me.Id}!");
            bot.StartReceiving(UpdateReceived, (_, exception, _) => Console.WriteLine(exception.Message), new Telegram.Bot.Polling.ReceiverOptions()
            {
                AllowedUpdates = [Telegram.Bot.Types.Enums.UpdateType.Message]
            }, cts.Token);
            Console.ReadLine();
            await cts.CancelAsync();
        }

    }
}
