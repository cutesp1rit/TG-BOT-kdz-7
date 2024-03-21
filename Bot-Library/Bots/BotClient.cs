using System.Net;
namespace Bot_Library.Bots;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class BotClient
{
    private TelegramBotClient _bot; // поле для нашего бота
    private List<User> _users = new List<User>(); // список всех пользователей, для работы с их файлами

    /// <summary>
    /// конструктор для инициализации бота
    /// </summary>
    /// <param name="token">токен</param>
    public BotClient(string token)
    {
        _bot = new TelegramBotClient(token);
    }
    
    /// <summary>
    /// Метод для активации работы бота
    /// </summary>
    public async Task StartBot()
    {
        using CancellationTokenSource cts = new ();
        ReceiverOptions receiverOptions = new ()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };
        _bot.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, receiverOptions, cts.Token);
        Console.WriteLine("Вы запустили бота.");
        Console.WriteLine("Нажмите любую кнопку, чтобы закончить работу");
        Console.ReadKey();
    }
    
    /// <summary>
    /// Асинхронный метод для обработки сообщений
    /// </summary>
    /// <param name="botClient">бот</param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // если null --> пропускаем
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
        User currentUser = _users.Find(x => x.ChatId.Contains(chatId.ToString()));

        // стартовое сообщение
        if (message.Text == "/start" && currentUser.UserState==UserEnum.Starting)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "\ud83d\udc4b\ud83c\udffb Привет! Я рад видеть тебя здесь!\n\n" +
                "\u2757\ufe0f Чтобы получить возможность пользоваться функциями для работы с файлом, пожалуйста," +
                " загрузите файл формата (CSV / JSON)");
        }
        // прислали документ --> обрабатываем по форматам
        else if (message.Document != null && currentUser.UserState==UserEnum.Starting)
        {
            var fileId = message.Document.FileId;
            var fileInfo = await botClient.GetFileAsync(fileId, cancellationToken);
            var filePath = fileInfo.FilePath;
            var f = new FileInfo(filePath!);
            if (f.Extension == ".csv" || f.Extension == ".json")
            {
                // вызываем метод по обработке файла
                await HandleGetFile(message, currentUser, _bot, cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Вы отправили файл с некорретным разрешением.\n" +
                    "\u2757\ufe0f Пожалуйста, отправьте файл с разрешением CSV или JSON");
            }
        }
        else if (currentUser.UserState == UserEnum.Starting)
        {
            // если пользователь отправил некорректное сообщение --> переспрашиваем
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "\u2757\ufe0f Чтобы получить возможность пользоваться функциями для работы с файлом, пожалуйста," +
                " загрузите файл формата (CSV / JSON)");
        }
        else if (message.Text == "Загрузить новый файл на обработку" && currentUser.UserState==UserEnum.Choosing)
        {
            // меняем на начальное состояние
            currentUser.UserState = UserEnum.Starting;
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "Загрузите файл формата (CSV / JSON)");
        }
        else if ((message.Text == "Произвести выборку по полям из файла" && currentUser.UserState==UserEnum.Choosing))
        {
            // переводим в состояние данного выбора
            currentUser.UserState = UserEnum.TakingOverField;
            // запускаем кнопки
            await TakingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if ((message.Text == "Отсортировать по одному из полей" && currentUser.UserState==UserEnum.Choosing))
        {
            // запускаем кнопки
            await _bot.SendTextMessageAsync(currentUser.ChatId,
                "По какому полю вы бы хотели отсортировать?",
                replyMarkup: new ReplyKeyboardMarkup(new[]
                {
                    // кнопки с вариантами
                    new [] { new KeyboardButton("CulturalCenterName по алфавиту") },
                    new [] { new KeyboardButton("NumberOfAccessPoints по возрастанию") }
                }), cancellationToken: cancellationToken);
            // переводим в соответствующее состояние
            currentUser.UserState = UserEnum.Sorting;
        }
        else if (currentUser.UserState == UserEnum.Sorting) 
        {
            // после получения сообщения
            if (message.Text == "CulturalCenterName по алфавиту")
            {
                currentUser.Sorting("CulturalCenterName");
            }
            else if (message.Text == "NumberOfAccessPoints по возрастанию")
            {
                currentUser.Sorting("NumberOfAccessPoints");
            }
            else // еще раз просим ответить корректно
            {
                await _bot.SendTextMessageAsync(currentUser.ChatId,
                    "\u2757\ufe0f Пожалуйста, выберите из двух возможных вариантов.",
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        // кнопки с вариантами
                        new [] { new KeyboardButton("CulturalCenterName по алфавиту") },
                        new [] { new KeyboardButton("NumberOfAccessPoints по возрастанию") }
                    }), cancellationToken: cancellationToken);
                return;
            }
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "\u2705 Сортировка прошла успешно!");  
            // при успехе переводим в меню
            currentUser.UserState = UserEnum.Choosing;
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if ((message.Text == "Скачать обработанный файл" && currentUser.UserState==UserEnum.Choosing))
        {
            // переводим в соответствующее состояние
            currentUser.UserState = UserEnum.WaitingFileName;
            // вызываем меню с кнопками
            await FileMenu(message, currentUser, _bot, cancellationToken);
        }
        else if (currentUser.UserState == UserEnum.Choosing)
        {
            // если сообщение некорректное -- отправляем меню еще раз
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        // работа с выборками
        else if (currentUser.UserState == UserEnum.TakingOverField && (message.Text == "CoverageArea" || 
                                                                       message.Text == "WiFiName" || 
                                                                       message.Text == "District и AccessFlag"))
        {
            // запомянаем наше введенное поле
            currentUser.TmpField = message.Text;

            if (message.Text == "CoverageArea" ||
                message.Text == "WiFiName")
            { // если выборка не по двум поля 
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Теперь введите значение, по которому должна пройти выборка.");    
                // меняем состояние для получения значение по выборке
                currentUser.UserState = UserEnum.WaitingForValue;
            }
            else // если выборка по двум полям
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Сначала введите значение для District."); 
                // меняем состояние для получения значение по выборке для дистрикт
                currentUser.UserState = UserEnum.WaitingForTwoValue;
            }
        }
        // получение данных по выборке и запуск фильтрации
        else if (currentUser.UserState == UserEnum.WaitingForTwoValue || currentUser.UserState == UserEnum.WaitingForValue)
        {
            if (currentUser.TmpField == "District и AccessFlag" && currentUser.UserState == UserEnum.WaitingForTwoValue)
            {
                // заносим первый результат в временный лист
                currentUser.TmpList = currentUser.TakingOverField(currentUser.TmpField, message.Text);
                currentUser.UserState = UserEnum.WaitingForValue; 
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Теперь введите значение для AccessFlag.");
                return;
            }

            // когда по двум полям
            if (currentUser.TmpField == "District и AccessFlag")
            {
                // если таких данных не нашлось, то список не перезаписываем
                if (currentUser.TakingOverField(currentUser.TmpField, message.Text, currentUser.TmpList).Count == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "К сожалению, я ничего не нашел по этим данным. :( " +
                        "Поэтому мы оставили все те же данные для дальнейшей работы.");
                }
                else // перезаписываем список и уведомляем
                {
                    currentUser.ListFromFile =
                        currentUser.TakingOverField(currentUser.TmpField, message.Text, currentUser.TmpList);
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "\u2705 Данные были сокращены под вашу выборку! Можете продолжить с ними работу!");
                }
            }
            else // сортировка для одного поля
            {
                // если таких данных не нашлось, то список не перезапсываем
                if (currentUser.TakingOverField(currentUser.TmpField, message.Text).Count == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "К сожалению, я ничего не нашел по этим данным. :( " +
                        "Поэтому мы оставили все те же данные для дальнейшей работы.");
                }
                else // перезаписываем список и уведомляем
                {
                    currentUser.ListFromFile = currentUser.TakingOverField(currentUser.TmpField, message.Text);
                    await botClient.SendTextMessageAsync(message.Chat.Id,
                        "\u2705 Данные были сокращены под вашу выборку! Можете продолжить с ними работу!");
                }
            }

            // возвращаем его в меню
            currentUser.UserState = UserEnum.Choosing;
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        // если сообщение некорректное для состояния выборки -- отправляем меню
        else if (currentUser.UserState == UserEnum.TakingOverField)
        {
            await TakingMenu(message, currentUser, _bot, cancellationToken);
        }
        else if (currentUser.UserState == UserEnum.WaitingFileName && (message.Text == "CSV" || 
                                                                       message.Text == "JSON"))
        { // состояние обработки выгрузки файла
            await GettingNewFile(message, currentUser, _bot, cancellationToken);
            // возвращаем его в меню
            currentUser.UserState = UserEnum.Choosing;
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        // если сообщение некорректно - снова выбрасываем меню
        else if (currentUser.UserState == UserEnum.WaitingFileName)
        {
            await FileMenu(message, currentUser, _bot, cancellationToken);
        }
    }
    
    /// <summary>
    /// Основное меню с кнопками
    /// </summary>
    /// <param name="message"></param>
    /// <param name="currentUser"></param>
    /// <param name="botClient"></param>
    /// <param name="cancellationToken"></param>
    private async Task ChoosingMenu(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(message.Chat.Id,
            "Чтобы вы хотели сделать дальше?\n" +
            "\u2757\ufe0f Пожалуйста, воспользуйтесь кнопками или напишите текстом то, что написано на одной из них.",
            replyMarkup: new ReplyKeyboardMarkup(new[]
            {
                // кнопки с вариантами
                new [] { new KeyboardButton("Загрузить новый файл на обработку") },
                new [] { new KeyboardButton("Произвести выборку по полям из файла") }, 
                new [] { new KeyboardButton("Отсортировать по одному из полей") },
                new [] { new KeyboardButton("Скачать обработанный файл") }
            }), cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Меню для состояния выборки
    /// </summary>
    /// <param name="message"></param>
    /// <param name="currentUser"></param>
    /// <param name="botClient"></param>
    /// <param name="cancellationToken"></param>
    private async Task TakingMenu(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(currentUser.ChatId,
            "Отлично, тогда выберите поле, по которому собираетесь произвести выборку!\n" +
            "\u2757\ufe0f Пожалуйста, воспользуйтесь кнопками или напишите текстом то, что написано на одной из них.",
            replyMarkup: new ReplyKeyboardMarkup(new[]
            {
                // кнопки с вариантами
                new [] { new KeyboardButton("CoverageArea"), new KeyboardButton("WiFiName") },
                new [] { new KeyboardButton("District и AccessFlag") }
            }), cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Меню для выгрузки файла
    /// </summary>
    /// <param name="message"></param>
    /// <param name="currentUser"></param>
    /// <param name="botClient"></param>
    /// <param name="cancellationToken"></param>
    private async Task FileMenu(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        await _bot.SendTextMessageAsync(currentUser.ChatId,
            "Выберите формат, в котором хотели бы получить файл с новыми данными. " +
            "\u2757\ufe0f Обратите внимание, я работаю исключительно с двумя форматами. " +
            "Пожалуйста, воспользуйтесь кнопками или напишите текстом то, что написано на одной из них.",
            replyMarkup: new ReplyKeyboardMarkup(new[]
            {
                // кнопки с вариантами
                new [] { new KeyboardButton("CSV"), new KeyboardButton("JSON") }
            }), cancellationToken: cancellationToken);
    }
    
    /// <summary>
    /// Метод для получения данных из файла
    /// </summary>
    /// <param name="message"></param>
    /// <param name="currentUser"></param>
    /// <param name="botClient"></param>
    /// <param name="cancellationToken"></param>
    private async Task HandleGetFile(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        try
        {
            var fileId = message.Document.FileId;
            var file = await botClient.GetFileAsync(fileId, cancellationToken);
            var filePath = file.FilePath;
            var f = new FileInfo(filePath!);
            List<WifiCC> allInf;

            using (var stream = new MemoryStream()) // запускаем поток, в который передаем присланный файл
            {
                await botClient.DownloadFileAsync(filePath, stream, cancellationToken);
                stream.Position = 0;
                if (f.Extension == ".csv")
                {
                    allInf = CSVProcessing.Read(stream); // запускаем read для csv
                }
                else // тогда json
                {
                    allInf = JSONProcessing.Read(stream); // запускаем read для json
                }
            }
            currentUser.ListFromFile = allInf; // передаем полученные данные в лист, закрепленный за user
            await botClient.SendTextMessageAsync(message.Chat.Id,
                "\u2705 Файл был успешно загружен!");
            // переводим его в следующее состояние
            currentUser.UserState = UserEnum.Choosing;
            await ChoosingMenu(message, currentUser, _bot, cancellationToken);
        }
        catch (Exception ex) // при передаче файла с некорректными данными
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
    
    /// <summary>
    /// Метод для выгрузки файла пользователю
    /// </summary>
    /// <param name="message"></param>
    /// <param name="currentUser"></param>
    /// <param name="botClient"></param>
    /// <param name="cancellationToken"></param>
    public async Task GettingNewFile(Message message, User currentUser, ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        try
        {
            MemoryStream stream; // поток
            string fileName; // имя
            if (message.Text == "CSV")
            {
                stream = CSVProcessing.Write(currentUser.ListFromFile); // передаем данные в поток
                fileName = $"{message.Chat.Username}_new.csv";
            }
            else // иначе JSON
            {
                stream = JSONProcessing.Write(currentUser.ListFromFile); // передаем данные в поток
                fileName = $"{message.Chat.Username}_new.json";
            }
            
            // отправляем получанный документ 
            await botClient.SendDocumentAsync(chatId: message.Chat.Id, document: InputFile.FromStream(stream, fileName), caption: "Ваш файл!");
            // отправка стикера через его публичное id
            await botClient.SendStickerAsync(
                chatId: message.Chat.Id,
                sticker: InputFile.FromFileId("CAACAgIAAxkBAAELwPdl-xBU7YXSmHIHaxNmat2c4pUijQACCQsAAvNDWUqhTkkZJ-u7ezQE"),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex) // при возникновении ошибок
        {
            Console.WriteLine("Произошла ошибка при загрузке файла.");
        }
    }
    
    /// <summary>
    /// Метод обработки ошибок бота
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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

    /// <summary>
    /// конструктор без параметров
    /// </summary>
    public BotClient()
    {
        
    }
}