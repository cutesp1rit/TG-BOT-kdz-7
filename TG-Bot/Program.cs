﻿using Bot_Library.Bots;

internal class Program
{
    // заменить ли _bot????
    static async Task Main(string[] args)
    {
        var botClient = new BotClient("6839200648:AAEXaVIrzhpfvYKzXIDd8f2oPAkLQfdt_Bs");
        await botClient.StartBot();
    }
}