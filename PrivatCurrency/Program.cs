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
using System.Linq;
using System.CodeDom.Compiler;

namespace PrivatCurrency
{
    class Program
    {
        static TelegramBotClient client = new TelegramBotClient("848578183:AAEyE9rbGZtyq4eunSdruS91Jj-gHn2F9Oc");
        static string URL1 = $"https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=5";
        static string URL2 = $"https://api.privatbank.ua/p24api/pubinfo?exchange&coursid=4";
        static List<Currency> currencies;
        static List<StateUser> stateUsers = new List<StateUser>();
        static TgContext cont = new TgContext();
        static string version = "1.1.0";
        static void Main(string[] args)
        {
            client.OnMessage += getMsgAsync;
            client.OnCallbackQuery += operateCallback;

            Console.Write(cont.Users.ToList()[0].Interval.IntervalId);
            foreach (var user in cont.Users)
            {
                stateUsers.Add(new StateUser(user));
                if(user.BotVersion!=version)
                {
                    client.SendTextMessageAsync(user.TelegramId, "У бота вышло обновление. Используйте /help, чтобы ознакомиться с ботом.");
                    user.BotVersion = version;
                }
            }
            foreach (var user in cont.Users)
            {
                if (user.BotVersion != version)
                {
                    client.SendTextMessageAsync(user.TelegramId, "У бота вышло обновление. Используйте /help, чтобы ознакомиться с ботом.");
                    user.BotVersion = version;
                }
            }
            cont.SaveChanges();
            #region Thread
            Thread myThread = new Thread(new ThreadStart(async () =>
            {
                while (true)
                {
                    SetCurrencies();
                    int NowMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
                    int i = 0;
                    List<User> users = await cont.Users.ToListAsync();
                    Console.Write(users[0].Interval.IntervalId);
                    foreach (var item in users)
                    {
                        if (stateUsers[i].State == 0)
                        {
                            if ((NowMinutes - item.Interval.SendFromMinutes) * (NowMinutes - item.Interval.SendToMinutes) <= 0)
                            {
                                if (item.Interval.WhenToSendMinutes == NowMinutes)
                                {
                                    List<string> temp = GetOptionalCurrencies(item.ChosenCurrencies);
                                    foreach (var msg in temp)
                                    {
                                        await client.SendTextMessageAsync(item.TelegramId, msg);
                                    }
                                }
                                while (item.Interval.WhenToSendMinutes <= NowMinutes)
                                {
                                    item.Interval.WhenToSendMinutes = item.Interval.WhenToSendMinutes + item.Interval.IntervalMinutes;
                                }
                                item.Interval.WhenToSendMinutes = (item.Interval.WhenToSendMinutes - item.Interval.SendFromMinutes) * (item.Interval.WhenToSendMinutes - item.Interval.SendToMinutes) > 0 ? item.Interval.SendFromMinutes : item.Interval.WhenToSendMinutes;
                            }
                        }
                    }
                    i++;
                    await cont.SaveChangesAsync();
                    Thread.Sleep(60000 - DateTime.Now.Second * 1000);
                }
            }));
            myThread.Start();
            #endregion
            client.StartReceiving();
            Console.Read();
        }
        private static void SetCurrencies()
        {
            currencies = new List<Currency>();
            XmlTextReader reader = new XmlTextReader(URL1);
            while (reader.Read())
            {
                if (reader.Name == "exchangerate")
                {
                    currencies.Add(new Currency() { MainCurrency = reader[1], TranslatingCurrency = reader[0], Sell = CurrencyFormater(reader[2]), Buy = CurrencyFormater(reader[3]) });
                }
            }
            reader = new XmlTextReader(URL2);
            while (reader.Read())
            {
                if (reader.Name == "exchangerate")
                {
                    if(reader[0]!="BTC")
                    {
                        currencies.Add(new Currency() { MainCurrency = reader[1], TranslatingCurrency = reader[0], Sell = CurrencyFormater(reader[2]), Buy = CurrencyFormater(reader[3]) });
                    }
                }
            }
        }
        private static async void operateCallback(object sender, CallbackQueryEventArgs e)
        {
            long TgId = e.CallbackQuery.From.Id;
            StateUser stUser = FindStateUser(TgId);
            string cq = e.CallbackQuery.Data;
            if (stUser == null)
            {
                stUser = new StateUser(FindUserInDb(TgId));
                stateUsers.Add(stUser);
            }
            if (stUser.State == 0)
            {
                if (cq == "change currency")
                {
                    stUser.ChosenCurrs = BitsFromShort(stUser.User.ChosenCurrencies);
                    stUser.State = 1;
                    List<List<InlineKeyboardButton>> inlines = new List<List<InlineKeyboardButton>>();
                    for (int i = 0; i < currencies.Count; i++)
                    {
                        await client.SendTextMessageAsync(TgId, currencies[i].MainCurrency + "-" + currencies[i].TranslatingCurrency, replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = stUser.ChosenCurrs[i] ? "✔️" : "✖️", CallbackData = $"chg {i}" }));
                    }
                    await client.SendTextMessageAsync(TgId, "Потвердить выбор?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Готово", CallbackData = "completeCurr" }));
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                }
                else if (cq == "change interval")
                {
                    stUser.State = 2;
                    stUser.User = FindUserInDb(stUser.User.TelegramId);
                    stUser.Update();
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, "Всё время указано в формате UTC +0").Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"Как часто обновлять данные:\n{MinsInTime(stUser.Interval)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg Interval" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"С какого времени дня отправлять данные:\n{MinsInTime(stUser.SendFrom)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg SendFrom" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"До какого времени дня отправлять данные:\n{MinsInTime(stUser.SendTo)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg SendTo" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"Во сколько начать отправлять:\n{MinsInTime(stUser.WhenToSend)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg WhenToSend" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, "Потвердить выбор?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Готово", CallbackData = "completeInter" })).Result.MessageId);
                }
            }
            else if (stUser.State == 1)
            {
                if (cq.Split(' ')[0] == "chg")
                {
                    int i = int.Parse(cq.Split(' ')[1]);
                    stUser.ChosenCurrs[i] = !stUser.ChosenCurrs[i];
                    await client.EditMessageTextAsync(TgId, e.CallbackQuery.Message.MessageId, e.CallbackQuery.Message.Text, replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = stUser.ChosenCurrs[i] ? "✔️" : "✖️", CallbackData = $"chg {i}" }));
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                }
                else if (cq == "completeCurr")
                {
                    stUser.State = 0;
                    FindUserInDb(TgId).ChosenCurrencies = FromBitToShort(stUser.ChosenCurrs);
                    await cont.SaveChangesAsync();
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Ответ успешно сохранён");
                }
            }
            else if (stUser.State == 2)
            {
                if (cq.Split(' ')[0] == "chg")
                {

                    string msg = cq.Split(' ')[1];
                    string text = "Пришлите сообщение с";
                    if (msg == "Interval")
                    {
                        stUser.State = 3;
                        text += " интервалом";
                    }
                    else if (msg == "SendFrom")
                    {
                        stUser.State = 4;
                        text += " временем от которого отправлять курс";

                    }
                    else if (msg == "SendTo")
                    {
                        stUser.State = 5;
                        text += " временем от которого перестать отправлять курс";

                    }
                    else if (msg == "WhenToSend")
                    {
                        stUser.State = 6;
                        text += " временем от которого начать отправку, если введённое время меньше, то сообщение отправится, а следующее время отсылки будет с прибавлением интервала";
                    }
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, text + " (в минутах или в формате времени)\nНапример: 1439 или 23:59, 0 или 00:00").Result.MessageId);
                }
                else if(cq=="completeInter")
                {
                    stUser.ChangeUser();
                    await cont.SaveChangesAsync();
                }
                await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
            }
        }
        public static string MinsInTime(int mins)
        {
            int hours = mins / 60;
            mins = mins % 60;
            if (hours == 24)
            {
                return "00:00";
            }
            return $"{hours / 10}{hours % 10}:{mins / 10}{mins % 10}";
        }
        public static int TimeInMins(string Time)
        {
            Time = Time.Trim();
            if (Time.Length == 5)
            {
                int hours = int.Parse(Time[0].ToString()) * 10;
                hours += int.Parse(Time[1].ToString());
                int minutes = int.Parse(Time[3].ToString()) * 10;
                minutes += int.Parse(Time[4].ToString());
                minutes += hours * 60;
                return minutes;
            }
            return -1;
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
            StateUser stUser = FindStateUser(TgId);
            string EntryMessage = "/getcurrency - получить курс всех валют бота.\n/settings - показывает меню для смены показываемых валют и интервал их показа.\n/help - показать эти команды";
            if (user == null)
            {
                user = new User() { Nickname = e.Message.Chat.FirstName + " " + e.Message.Chat.LastName, TelegramId = TgId };
                cont.Users.Add(user);
                await cont.SaveChangesAsync();
                await client.SendTextMessageAsync(TgId, "Теперь Вам каждые 15 минут будет присылаться курс валют ПриватБанка)");
                await client.SendTextMessageAsync(TgId, EntryMessage);
                foreach (var curr in currencies)
                {
                    await client.SendTextMessageAsync(user.TelegramId, curr.ToString());
                }
                return;
            }
            else if (e.Message.Text == "/start")
            {
                await client.SendTextMessageAsync(TgId, "Добро пожаловать снова к нам");
                await client.SendTextMessageAsync(TgId, EntryMessage);
            }
            if (stUser == null)
            {
                stUser = new StateUser(user);
                stateUsers.Add(stUser);
            }
            if (stUser.State == 0)
            {
                if (e.Message.Text == "/getcurrency")
                {
                    foreach (var curr in currencies)
                    {
                        await client.SendTextMessageAsync(user.TelegramId, curr.ToString());
                    }
                }
                else if (e.Message.Text == "/settings")
                {
                    InlineKeyboardMarkup settings = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                    {
                    new InlineKeyboardButton() { Text = "Изменить вид", CallbackData = "change currency" },
                    new InlineKeyboardButton() { Text = "Установить интервал", CallbackData = "change interval" } })
                    ;
                    await client.SendTextMessageAsync(user.TelegramId, "Изменить вид - позволяет выбрать какие валюты показывать\nУстановить интервал - позволяет установить как часто отправлять сведения о курсе, а также с какого по какое время", replyMarkup: settings);
                }
                else if (e.Message.Text == "/help")
                {
                    await client.SendTextMessageAsync(TgId, EntryMessage);
                }
            }
            else if (stUser.State == 1)
            {
                await client.SendTextMessageAsync(TgId, "Для использования, завершите настройку валют (нажмите 'Готово').");
            }
            else if (stUser.State == 2)
            {
                await client.SendTextMessageAsync(TgId, "Для использования, завершите настройку интервалов (нажмите 'Готово').");
            }
            else if (stUser.State >= 3 && stUser.State <= 6)
            {
                int res;
                if (int.TryParse(e.Message.Text, out res)) { }
                else
                {
                    res = TimeInMins(e.Message.Text);
                }
                if (res != -1)
                {
                    if (stUser.State == 3)
                    {
                        stUser.Interval = res;
                    }
                    else if (stUser.State == 4)
                    {
                        stUser.SendFrom = res;
                    }
                    else if (stUser.State == 5)
                    {
                        stUser.SendTo = res; 
                    }
                    else if (stUser.State == 6)
                    {
                        stUser.WhenToSend = res;
                    }
                    stUser.State = 2;
                    foreach(var item in stUser.IdOfMsgsOnDelete)
                    {
                        await client.DeleteMessageAsync(TgId, item);
                    }
                    stUser.IdOfMsgsOnDelete = new List<int>();
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, "Всё время указано в формате UTC +0").Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(e.Message.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"Как часто обновлять данные (в минутах):\n{MinsInTime(stUser.Interval)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg Interval" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"С какого времени дня отправлять данные (в минутах):\n{MinsInTime(stUser.SendFrom)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg SendFrom" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"До какого времени дня отправлять данные (в минутах):\n{MinsInTime(stUser.SendTo)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg SendTo" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, $"Во сколько начать отправлять (в минутах):\n{MinsInTime(stUser.WhenToSend)}", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Изменить", CallbackData = "chg WhenToSend" })).Result.MessageId);
                    stUser.IdOfMsgsOnDelete.Add(client.SendTextMessageAsync(TgId, "Потвердить выбор?", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton() { Text = "Готово", CallbackData = "completeInter" })).Result.MessageId);
                }
                else
                {
                    await client.SendTextMessageAsync(TgId, "Неправильный формат времени");
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
        private static StateUser FindStateUser(long TgId)
        {
            foreach (var item in stateUsers)
            {
                if (item.User.TelegramId == TgId)
                {
                    return item;
                }
            }
            return null;
        }
        private static bool[] BitsFromShort(short num)
        {
            bool[] temp = new bool[16];
            BitArray bits = new BitArray(BitConverter.GetBytes(num));
            for (int i = 0; i < bits.Length; i++)
            {
                temp[i] = bits[i];
                Console.Write(Convert.ToInt32(temp[i]));
            }
            return temp;
        }
        private static List<string> GetOptionalCurrencies(short optionNumber)
        {
            List<string> res = new List<string>();
            BitArray bits = new BitArray(BitConverter.GetBytes(optionNumber));
            Console.WriteLine(FromBitToShort(bits));
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    res.Add(currencies[i].ToString());
                }
            }
            return res;
        }
        private static List<string> GetOptionalCurrencies(bool[] bits)
        {
            List<string> res = new List<string>();
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    res.Add(currencies[i].ToString());
                }
            }
            return res;
        }
        private static short FromBitToShort(BitArray bits)
        {
            short res = 0;
            short temp;
            for (int i = 0; i < bits.Length - 1; i++)
            {
                temp = Convert.ToInt16(bits[i]);
                temp *= Convert.ToInt16(Math.Pow(2, i));
                res += temp;
            }
            temp = Convert.ToInt16(bits[bits.Length - 1]);
            temp *= -1;
            res *= temp;
            return res;
        }
        private static short FromBitToShort(bool[] bits)
        {
            short res = 0;
            short temp;
            for (int i = 0; i < bits.Length - 1; i++)
            {
                temp = Convert.ToInt16(bits[i]);
                temp *= Convert.ToInt16(Math.Pow(2, i));
                res += temp;
            }
            res *= bits[bits.Length - 1] ? (short)-1 : (short)1;
            return res;
        }
    }
}
