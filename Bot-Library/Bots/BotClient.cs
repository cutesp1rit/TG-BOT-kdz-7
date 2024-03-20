namespace Bot_Library.Bots;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class BotClient
{
    private TelegramBotClient _bot;
    private List<User> _users = new List<User>();

    public BotClient(string token)
    {
        _bot = new TelegramBotClient(token);
    }
    
    public async Task StartBot()
    {
        using CancellationTokenSource cts = new ();
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };
        _bot.StartReceiving(HandleUpdateAsync, 
            HandlePollingErrorAsync, receiverOptions, cts.Token);
        Console.WriteLine("Вы запустили бота.");
        Console.ReadKey();
    }
    
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null)
            return;
        
        var message = update.Message;
        var chatId = message.Chat.Id;

        // если наш пользователь еще не зарегистрирован - регистрируем
        if (!_users.ConvertAll(x => x.ChatId).Contains(chatId.ToString()))
        {
            _users.Add(new User(chatId.ToString()));
        }

        // для работы с текущем пользователем, помещаем его в отдельную переменную
        User currentUser = _users.Find(x => x.ChatId.Contains(x.ChatId.ToString()));

        if (message.Text == "/start" && currentUser.UserState==UserEnum.Starting)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Привет! Я рад видеть тебя здесь!\n\n" +
                "Чтобы получить возможность пользоваться функциями для работы с файлом, пожалуйста," +
                " загрузите файл формата (CSV/JSON)");
        }
        else if (message.Document != null && currentUser.UserState==UserEnum.Starting)
        {
            var fileId = message.Document.FileId;
            var fileInfo = await botClient.GetFileAsync(fileId, cancellationToken);
            var filePath = fileInfo.FilePath;
            var f = new FileInfo(filePath!);
            if (f.Extension == ".csv" || f.Extension == ".json")
            {
                HandleGetFile(message, currentUser, _bot, cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Вы отправили файл с некорретным разрешением.\n" +
                    "Пожалуйста, отправьте файл с разрешением CSV или JSON");
            }
        }
        else if (currentUser.UserState == UserEnum.Starting)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Чтобы получить возможность пользоваться функциями для работы с файлом, пожалуйста," +
                " загрузите файл формата (CSV/JSON)");
        }
        else if (message.Text == "Загрузить новый файл на обработку" && currentUser.UserState==UserEnum.Choosing)
        {
            
        }
        else if ((message.Text == "Произвести выборку по одному из полей файла" && currentUser.UserState==UserEnum.Choosing))
        {
            
        }
        else if ((message.Text == "Отсортировать по одному из полей" && currentUser.UserState==UserEnum.Choosing))
        {
            
        }
        else if ((message.Text == "Скачать обработанный файл" && currentUser.UserState==UserEnum.Choosing))
        {
            
        }
        else if (currentUser.UserState == UserEnum.Choosing)
        {
            await Menu(message, currentUser, _bot, cancellationToken);
        }
    }
    
    private async Task Menu(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(currentUser.ChatId,
            "Файл успешно загружен. Какую работу над ней вы бы хотели проделать?\n" +
            "Пожалуйста, воспользуйтесь кнопками или напишите текстом то, что написано на одной из них.",
            replyMarkup: new ReplyKeyboardMarkup(new[]
            {
                // кнопки с вариантами
                new [] { new KeyboardButton("Загрузить новый файл на обработку") },
                new [] { new KeyboardButton("Произвести выборку по одному из полей файла") }, 
                new [] { new KeyboardButton("Отсортировать по одному из полей") },
                new [] { new KeyboardButton("Скачать обработанный файл") }
            }), cancellationToken: cancellationToken);
    }

    private void ChangeState(User currentUser, UserEnum state)
    {
        for (int i = 0; i < _users.Count; i++)
        {
            if (_users[i].ChatId.Contains(currentUser.ChatId))
            {
                _users[i].UserState = state;
            }
        }
    }
    
    private async Task HandleGetFile(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        try
        {
            var fileId = message.Document.FileId;
            var file = await botClient.GetFileAsync(fileId, cancellationToken);
            var filePath = file.FilePath;
            List<WifiCC> allInf;

            using (var stream = new MemoryStream())
            {
                await botClient.DownloadFileAsync(filePath, stream, cancellationToken);
                stream.Position = 0;
                allInf = CSVProcessing.Read(stream);
            }

            // файл успешно считался, поэтому переводим его в следующее состояние
            currentUser.UserState = UserEnum.Choosing;
            await Menu(message, currentUser, _bot, cancellationToken);
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "\U0001F480 я чуть не упал");
            // отправка стикера через его публичное id
            await botClient.SendStickerAsync(
                chatId: message.Chat.Id,
                sticker: InputFile.FromFileId("CAACAgIAAxkBAAELwHNl-sq5dRfmVUIbI8yqGTG3VNiXEAACCBgAAo5ioUqoLIXkKvvgijQE"),
                cancellationToken: cancellationToken);
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Вы отправили файл с некорректными данными...\n" +
                "Пожалуйста, пришлите другой");
            Console.WriteLine($"Ошибка при обработке файла: {ex.Message}");
        }
    }
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}