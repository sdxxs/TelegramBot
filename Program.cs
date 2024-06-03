using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.TGClient;

namespace TelegramBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("6981602280:AAHke33WLZeHd7wjzcDR3WdglqhfgFhV_2g");
            botClient.StartReceiving(Update, Error);
            Console.ReadLine();

        }

        async static Task Update(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken token)
        {

            if (update.Type != UpdateType.Message)
            {
                return;
            }
            if (update.Message!.Type != MessageType.Text)
            {
                return;
            }
            var message = update.Message;
            if (message.Text != null)
            {
                await Handlermessage(botClient, message, token);
            }
        }
        async static Task Handlermessage(ITelegramBotClient botClient, Message message, CancellationToken token)
        {
            TelegramClient telegramBotClient = new TelegramClient(botClient);

            if (message.Text.Contains("/start"))
            {
                await telegramBotClient.GreetingMessage(message);

            }

            else if (message.Text.Contains("/observ"))
            {
                await telegramBotClient.GetОbservForRegion(message);

            }

            else if (message.Text.Contains("/getlistofregioncodeukraine"))
            {
                await telegramBotClient.GetListOfRegionCodeUkraine(message);
            }

            else if (message.Text.Contains("/addmyobserv"))
            {
                await telegramBotClient.AddMyObserv(message);
            }

            else if (message.Text.Contains("/checkmyobservlist"))
            {
                await telegramBotClient.CheckMyObservathionList(message);
            }

            else if (message.Text.Contains("/removeobservfrommylist"))
            {
                await telegramBotClient.RemoveObservFromMyList(message);
            }
            else if (message.Text.Contains("/photoof"))
            {
                await telegramBotClient.GetPhoto(message);
            }

            else if (message.Text.Contains("/photo"))
            {
                await telegramBotClient.GetBirdPhoto(message);
            }

        }
        private static Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            var ErroeMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n{apiRequestException.ErrorCode}" +
                $"\n {apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErroeMessage);
            return Task.CompletedTask;
        }
    }
}