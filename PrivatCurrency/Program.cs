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
using System.Collections;
using System.Threading.Tasks;

namespace PrivatCurrency
{
    class Program
    {
        static TelegramBotClient client;
        static string URL;
        static List<Currency> currencies;
        static TgContext cont;

        static void Main(string[] args)
        {
            GetOptionalCurrencies(15);
            //cont = new TgContext

            //client = new TelegramBotClient("848578183:AAEyE9rbGZtyq4eunSdruS91Jj-gHn2F9Oc");
            //URL = $"https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";
            //client.OnMessage += getMsgAsync;
            //client.OnCallbackQuery += operateCallback;
            //#region Thread
            //Thread myThread = new Thread(new ThreadStart(async () =>
            //{
            //    while (true)
            //    {
            //        currencies = new List<Currency>();
            //        XmlTextReader reader = new XmlTextReader(URL);
            //        int count = reader.AttributeCount;
            //        while (reader.Read())
            //        {
            //            if (reader.Name == "exchangerate")
            //            {
            //                currencies.Add(new Currency() { MainCurrency = reader[1], TranslatingCurrency = reader[0], Sell = CurrencyFormater(reader[2]), Buy = CurrencyFormater(reader[3]) });
            //            }
            //        }
            //        int NowMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            //        using(TgContext cont2 = new TgContext())
            //        {
            //            foreach (var item in cont2.Users)
            //            {
            //                if (item.WhenToSendMinutes <= NowMinutes)
            //                {
            //                    List<string> temp = GetOptionalCurrencies(item.ChosenCurrencies);
            //                    foreach (var msg in temp)
            //                    {
            //                        await client.SendTextMessageAsync(item.TelegramId, msg);
            //                    }
            //                    item.WhenToSendMinutes = NowMinutes + item.IntervalMinutes;
            //                    item.WhenToSendMinutes = item.WhenToSendMinutes > item.SendToMinutes ? item.SendFromMinutes : item.WhenToSendMinutes;
            //                }
            //            }
            //            await cont2.SaveChangesAsync();
            //        }
            //        Thread.Sleep(60000 - DateTime.Now.Second * 1000);
            //    }
            //}));
            //myThread.Start();
            //#endregion
            //client.StartReceiving();
            Console.Read();
        }

        private static void operateCallback(object sender, CallbackQueryEventArgs e)
        {
            if(e.CallbackQuery.Data=="change currency")
            {

            }
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
                await cont.SaveChangesAsync();
                await client.SendTextMessageAsync(TgId, "Теперь Вам каждые 15 минут будет присылаться курс валют ПриватБанка)");
                foreach (var curr in currencies)
                {
                    await client.SendTextMessageAsync(user.TelegramId, curr.ToString());
                }
                return;
            }
            if (e.Message.Text == "/getcurrency")
            {
                foreach (var curr in currencies)
                {
                    await client.SendTextMessageAsync(user.TelegramId, curr.ToString());
                }
            }
            else if (e.Message.Text == "/settings")
            {
                //InlineKeyboardMarkup settings = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                //{
                //    new InlineKeyboardButton() { Text = "Изменить вид", CallbackData = "change currency" },
                //    new InlineKeyboardButton() { Text = "Изменить вид", CallbackData = "change currency" } })
                //;
                //await client.SendTextMessageAsync(user.TelegramId, "Изменить вид - позволяет выбрать какие валюты показывать\nУстановить интервал - позволяет установить как часто отправлять сведения о курсе, а также с какого по какое время", replyMarkup: settings);
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
        private static List<string> GetOptionalCurrencies(short optionNumber)
        {
            List<string> res = new List<string>();
            BitArray bits = new BitArray(BitConverter.GetBytes(optionNumber));
            for (int i = 0; i < bits.Length; i++)
            {
                if (bool.Parse(bits[i].ToString()))
                {
                    //res.Add(currencies[i].ToString());
                }
                Console.Write(Convert.ToInt16(bool.Parse(bits[i].ToString())));
            }
            return res;
        }
    }
}
