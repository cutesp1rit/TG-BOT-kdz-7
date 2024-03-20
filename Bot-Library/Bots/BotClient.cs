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
                " загрузите файл формата (CSV / JSON)");
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
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Файл был успешно загружен!");
                // переводим его в следующее состояние
                currentUser.UserState = UserEnum.Choosing;
                await ChoosingMenu(message, currentUser, _bot, cancellationToken);
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
                " загрузите файл формата (CSV / JSON)");
        }
        else if (message.Text == "Загрузить новый файл на обработку" && currentUser.UserState==UserEnum.Choosing)
        {
            currentUser.UserState = UserEnum.Starting;
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Загрузите файл формата (CSV / JSON)");
        }
        else if ((message.Text == "Произвести выборку по одному из полей файла" && currentUser.UserState==UserEnum.Choosing))
        {
            currentUser.UserState = UserEnum.TakingOverField;
            await TakingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if ((message.Text == "Отсортировать по одному из полей" && currentUser.UserState==UserEnum.Choosing))
        {
            // доделать
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Эта функция в разработке! Пожалуйста, выберите другую!");
        }
        else if ((message.Text == "Скачать обработанный файл" && currentUser.UserState==UserEnum.Choosing))
        {
            currentUser.UserState = UserEnum.WaitingFileName;
            await FileMenu(message, currentUser, _bot, cancellationToken);
        }
        else if (currentUser.UserState == UserEnum.Choosing)
        {
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if (currentUser.UserState == UserEnum.TakingOverField && (message.Text == "CoverageArea" || 
                                                                       message.Text == "WiFiName" || 
                                                                       message.Text == "District"|| 
                                                                       message.Text == "AccessFlag"))
        {
            // меняем состояние для получения значение по выборке
            currentUser.UserState = UserEnum.WaitingForValue;
            // запомянаем наше введенное поле
            currentUser.TmpField = message.Text;
            
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Теперь введите значение, по которому должна пройти выборка:");
        }
        else if (currentUser.UserState == UserEnum.WaitingForValue)
        {
            if (currentUser.TakingOverField(currentUser.TmpField, message.Text).Count == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "К сожалению, по данному значению не нашлось никаких данных. " +
                    "Поэтому мы оставили все те же данные для дальнейшей работы.");
            }
            else
            {
                currentUser.ListFromFile = currentUser.TakingOverField(currentUser.TmpField, message.Text);
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Данные были сокращены под вашу выборку! Можете продолжить с ними работу!");
            }
            // возвращаем его в меню
            currentUser.UserState = UserEnum.Choosing;
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if (currentUser.UserState == UserEnum.TakingOverField)
        {
            await TakingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if (currentUser.UserState == UserEnum.WaitingFileName && (message.Text == "CSV" || 
                                                                       message.Text == "JSON"))
        {
            await GettingNewFile(message, currentUser, _bot, cancellationToken);
            // возвращаем его в меню
            currentUser.UserState = UserEnum.Choosing;
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if (currentUser.UserState == UserEnum.WaitingFileName)
        {
            await FileMenu(message, currentUser, _bot, cancellationToken);
        }
    }
    
    private async Task ChoosingMenu(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(currentUser.ChatId,
            "Чтобы вы хотели сделать дальше?\n" +
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
    
    private async Task TakingMenu(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(currentUser.ChatId,
            "Отлично, тогда выберите поле, по которому собираетесь произвести выбору!\n" +
            "Пожалуйста, воспользуйтесь кнопками или напишите текстом то, что написано на одной из них.",
            replyMarkup: new ReplyKeyboardMarkup(new[]
            {
                // кнопки с вариантами
                new [] { new KeyboardButton("CoverageArea"), new KeyboardButton("WiFiName") },
                new [] { new KeyboardButton("District"), new KeyboardButton("AccessFlag") }
            }), cancellationToken: cancellationToken);
    }
    
    private async Task FileMenu(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(currentUser.ChatId,
            "Выберите формат, в котором хотели бы получить файл с новыми данными. " +
            "Обратите внимание, я работаю исключительно с двумя форматами. " +
            "Пожалуйста, воспользуйтесь кнопками или напишите текстом то, что написано на одной из них.",
            replyMarkup: new ReplyKeyboardMarkup(new[]
            {
                // кнопки с вариантами
                new [] { new KeyboardButton("CSV"), new KeyboardButton("JSON") }
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
            var f = new FileInfo(filePath!);
            List<WifiCC> allInf;

            using (var stream = new MemoryStream())
            {
                await botClient.DownloadFileAsync(filePath, stream, cancellationToken);
                stream.Position = 0;
                if (f.Extension == ".csv")
                {
                    allInf = CSVProcessing.Read(stream);
                }
                else
                {
                    allInf = JSONProcessing.Read(stream);
                }
            }
            currentUser.ListFromFile = allInf;
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
    
    public async Task GettingNewFile(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        try
        {
            MemoryStream stream;
            string fileName;
            if (message.Text == "CSV")
            {
                stream = CSVProcessing.Write(currentUser.ListFromFile);
                fileName = $"{message.Chat.Username}_new.csv";
            }
            else // иначе JSON
            {
                stream = JSONProcessing.Write(currentUser.ListFromFile);
                fileName = $"{message.Chat.Username}_new.json";
            }
            await botClient.SendDocumentAsync(chatId: message.Chat.Id, document: InputFile.FromStream(stream, fileName), caption: "Ваш файл!");
            // отправка стикера через его публичное id
            await botClient.SendStickerAsync(
                chatId: message.Chat.Id,
                sticker: InputFile.FromFileId("CAACAgIAAxkBAAELwPdl-xBU7YXSmHIHaxNmat2c4pUijQACCQsAAvNDWUqhTkkZJ-u7ezQE"),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла ошибка при загрузке файла.");
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