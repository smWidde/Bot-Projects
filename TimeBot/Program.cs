using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Drawing;
using System.Net;
namespace TimeBot
{
    class Program
    {
        static TelegramBotClient client;
        static void Main(string[] args)
        {
            client = new TelegramBotClient("848578183:AAEyE9rbGZtyq4eunSdruS91Jj-gHn2F9Oc");
            client.OnMessage += getMsgAsync;
            client.StartReceiving();
            Console.Read();
        }
        private static async void getMsgAsync(object sender, MessageEventArgs e)
        {
            if(e.Message.Text.ToString()=="/start")
            {
                await client.SendTextMessageAsync(e.Message.Chat.Id, "Бот только показывает дату по сообщению ");
            }
            else
            {
                DateTime date = DateTime.Now;
                await client.SendTextMessageAsync(e.Message.Chat.Id, $"Сейчас (UTC +0): {date}");
            }
        }
    }
}
