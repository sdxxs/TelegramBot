using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramBot.TGClient
{
    internal class TelegramClient
    {
        private ITelegramBotClient botClient { get; set; }
        public TelegramClient(ITelegramBotClient _botClient)
        {
            botClient = _botClient;
        }

       
        public async Task GreetingMessage(Message message)
        {
            await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Вітаємо вас в боті для спостереженням за птахами\\! \n\nЗа допомогою *команд* ти зможеш як отримувати всю потрібну інформацію про спостереження\\, так і створити свій власний список\\.",
            parseMode: ParseMode.MarkdownV2,
            disableNotification: true,
            replyToMessageId: message.MessageId,
            replyMarkup: new InlineKeyboardMarkup(
            InlineKeyboardButton.WithUrl(
            text: "Більше цікавої інформації на сайті",
            url: "https://ebird.org/home")));

        }

        public async Task GetОbservForRegion(Message message)
        {

            string RegionCode = message.Text.Substring(message.Text.IndexOf("v") + 1);

            string path = $"http://localhost:5122/api/MyBird/GetObservByRegionCode?regionCode={RegionCode}";//
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(path),//address            
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string ListOfObserv = await response.Content.ReadAsStringAsync();
                if (ListOfObserv == "error")
                {
                    ListOfObserv= "Щось пішло не так...Ви допустили помилку в записі команди, або такого регіону не занесено до доступної нам бази даних." +
                 "\n Спробуйте ще раз (p.s приклад команди для коректного пошуку спостережень в Києвській обсласті:/observUA-30)";

                }

                await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: ListOfObserv,
                disableNotification: true,
                InlineKeyboardButton.
                replyToMessageId: message.MessageId); ;
            }
        }


        public async Task GetListOfRegionCodeUkraine(Message message)
        {
            string path = $"http://localhost:5122/api/MyBird/GetRegionList";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(path),//address            
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string ListOfRegionCode = await response.Content.ReadAsStringAsync();
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: ListOfRegionCode,
                disableNotification: true,
                replyToMessageId: message.MessageId);
            }

        }

        public async Task CheckMyObservathionList(Message message)
        {
            long ChatId = message.Chat.Id;
            string path = $"http://localhost:5122/api/MyBird/GetMyOwnObservList?PersonChatId={ChatId}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(path),//address            
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string ListOfMyObserv = await response.Content.ReadAsStringAsync();
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: ListOfMyObserv,
                disableNotification: true,
                replyToMessageId: message.MessageId);
            }          
        }


        public async Task RemoveObservFromMyList(Message message)
        {

            string id = message.Text.Substring(message.Text.IndexOf("t") + 1);

            string path = $"http://localhost:5122/api/MyBird/DeleteMyObserv?PersonChatId={message.Chat.Id}&id={id}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(path),//address            
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();

                if (result == "error")
                {
                    result="`Помилка при запиті, перевірте корректність запису команди (Введіть іd існуючого елемента, який бажаєте видалити на місці ID в команді)`";
                }

                await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                text: result,
                parseMode: ParseMode.MarkdownV2,
                disableNotification: true,
                replyToMessageId: message.MessageId
                );
            }
        }

        public async Task AddMyObserv(Message message)
        {
            string[] info = message.Text.Split(";");
            string result = " ";

            if (info.Length != 6)
            {
                result = "Щось пішло не так\\.\\.\\.Ви допустили помилку в записі команди\\, ocь приклад щодо використання \\p\\.s обов\\'язково після кожної властивості *пишіть \\;* \\:" +
             "\n /addmyobserv; _\n Назва птаха\\; \n Назва місця спостереження\\; \n Дата і час\\;  \n Кількість особин цього видy\\, які спостерігались\\;_";
            }

            else
            {
                long ChatId = message.Chat.Id;
                string path = $"http://localhost:5122/api/MyBird/PostMyObserv?PersonChatId={ChatId}&sciName={info[1]}&locName={info[2]}&obsDt={info[3]}&howMany={info[4]}";
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(path),            
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();            
                }
                if (result == "error")
                {
                    result = "`Помилка при запиті, перевірте корректність запису команди (ви ввели цифру на місці кількості?)`";
                    result = result+ "Приклад щодо використання \\p\\.s обов\\'язково після кожної властивості *пишіть \\;* \\:" +
                    "\n /addmyobserv; _\n Назва птаха\\; \n Назва місця спостереження\\; \n Дата і час\\;  \n Кількість особин цього видy\\, які спостерігались\\;_";
                }
            }
           await botClient.SendTextMessageAsync(
                      chatId: message.Chat.Id,
                      text: result,
                      parseMode: ParseMode.MarkdownV2,
                      disableNotification: true,
                      replyToMessageId: message.MessageId);
        }
        public async Task GetBirdPhoto(Message message)
        {
            string path = $"http://localhost:5122/api/MyBird/GetRandomPhotoOfBird";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(path),//address            
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string url = await response.Content.ReadAsStringAsync();

                await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: InputFile.FromString(url),
                caption: "`Тримайте фотографію`",
                parseMode: ParseMode.MarkdownV2,
                disableNotification: true,
                replyToMessageId: message.MessageId,
                replyMarkup: new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl(
                text: "Більше на сайті",
                url: "https://unsplash.com/s/photos/birds"))
                 );
            }

        }

        public async Task GetPhoto(Message message)
        {
            string PhotoQuery = message.Text.Substring(message.Text.IndexOf("f") + 1);

            string path = $"http://localhost:5122/api/MyBird/GetPhoto?query={PhotoQuery}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(path),//address            
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                string url = await response.Content.ReadAsStringAsync();

                if (url == "error")
                {
                    url = "`Щось пішло не так\\.\\.\\.Ви допустили помилку в записі команди\\, або фотографію не було знайдено\\.`" +
               "\n `Правильний запис команди \\/photoofНАЗВАПТАХААНГЛІЙСЬКОЮ \\(p\\.s приклад команди для пошуку фотографії мартину\\: \\/photoofgull\\)`";
                    await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: url,
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: true,
                    replyToMessageId: message.MessageId
                    );
                }
                else
                {
                    await botClient.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    photo: InputFile.FromString(url),
                    caption: "`Тримайте фотографію`",
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: true,
                    replyToMessageId: message.MessageId,
                    replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                    text: "Більше на сайті",
                    url: "https://unsplash.com/s/photos/birds"))
                     );
                }
            }

        }
    }
}
