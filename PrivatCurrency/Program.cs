using System;
using System.Collections.Generic;
using System.Data.Entity;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Xml;
using System.Net;
using System.Threading;
using Telegram.Bot.Types;

namespace PrivatCurrency
{
    public class User
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public long TelegramId { get; set; }
    }

    public class TgContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
    public class Currency
    {
        public string MainCurrency { get; set; }
        public string TranslatingCurrency { get; set; }
        public string Sell { get; set; }
        public string Buy { get; set; }
        public override string ToString()
        {
            return $"{MainCurrency}-{TranslatingCurrency}:\nПокупка {Buy}\nПродажа {Sell}";
        }
    }
    class Program
    {
        static TelegramBotClient client;
        static string URL;
        static List<Currency> currencies;
        static TgContext cont;
        static void Main(string[] args)
        {
            cont = new TgContext();
            client = new TelegramBotClient("848578183:AAEyE9rbGZtyq4eunSdruS91Jj-gHn2F9Oc");
            URL = $"https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";
            client.OnMessage += getMsgAsync;
            client.StartReceiving();
            List<BotCommand> commands = new List<BotCommand>();
            BotCommand command = new BotCommand();
            command.Command = "/getCurrency";
            command.Description = "Получить валюты прямо сейчас";
            commands.Add(command);
            client.SetMyCommandsAsync(commands);
            InlineKeyboardMarkup myInlineKeyboard = new InlineKeyboardMarkup(
            new InlineKeyboardButton[][]
            {
                    new InlineKeyboardButton[] // First row
                    {
                        new InlineKeyboardButton(){Text="Установить интервал", CallbackData="/setInterval"}, // First column
                        new InlineKeyboardButton(){Text="Изменить показываемые валюты", CallbackData="/changeCurrency"}  //Second column
                    }
            })
            { };
            Thread myThread = new Thread(new ThreadStart(() =>
            {
                currencies = new List<Currency>();
                XmlTextReader reader = new XmlTextReader(URL);
                int count = reader.AttributeCount;
                while (reader.Read())
                {
                    if (reader.Name == "exchangerate")
                    {
                        currencies.Add(new Currency() { MainCurrency = reader[1], TranslatingCurrency = reader[0], Sell = CurrencyFormater(reader[2]), Buy = CurrencyFormater(reader[3]) });
                    }
                }
                foreach (var item in cont.Users)
                {
                    //foreach (var curr in currencies)
                    //{
                    //    client.SendTextMessageAsync(item.TelegramId, curr.ToString());
                    //}
                    if(item.TelegramId==425334144)
                        client.SendTextMessageAsync(item.TelegramId, "Test", replyMarkup: myInlineKeyboard);
                }
                Thread.Sleep(15 * 60000);
            }));
            myThread.Start();
            Console.Read();
        }
        public static string CurrencyFormater(string currency)
        {
            for (int i = 0; i < currency.Length; i++)
            {
                if (currency[i] == '.')
                {
                    return currency.Substring(0, i + 3);
                }
            }
            return currency;
        }
        private static async void getMsgAsync(object sender, MessageEventArgs e)
        {
            long TgId = e.Message.Chat.Id;
            User user = FindUserInDb(TgId);
            if (user == null)
            {
                user = new User() { Nickname = e.Message.Chat.FirstName + " " + e.Message.Chat.LastName, TelegramId = TgId };
                cont.Users.Add(user);
                cont.SaveChanges();
                await client.SendTextMessageAsync(TgId, "Теперь Вам каждые 15 минут будет присылаться курс валют ПриватБанка)");
                foreach (var curr in currencies)
                {
                    await client.SendTextMessageAsync(user.TelegramId, curr.ToString());
                }
                return;
            }
            if (e.Message.Text == "/getCurrency")
            {
                foreach (var curr in currencies)
                {
                    await client.SendTextMessageAsync(user.TelegramId, curr.ToString());
                }
            }
        }
        private static User FindUserInDb(long TgId)
        {
            foreach (var item in cont.Users)
            {
                if (item.TelegramId == TgId)
                {
                    return item;
                }
            }
            return null;
        }

    }
}
