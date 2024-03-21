using Bot_Library.Bots;

internal class Program
{
    // ссылка на бота в тг: https://t.me/wifi_file_handler_bot
    // он красиво оформлен, используйте мой токен!! 
    static async Task Main(string[] args)
    {
        try
        {
            var botClient = new BotClient("6839200648:AAEXaVIrzhpfvYKzXIDd8f2oPAkLQfdt_Bs");
            await botClient.StartBot();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Произошла неизвестная ошибка.");
        }
    }
}