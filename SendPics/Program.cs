using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Drawing;
using System.Net;
using Telegram.Bot.Types.InlineQueryResults;
using System.Text;
using Telegram.Bot.Types;

namespace SendPics
{


    class Program
    {
        static TelegramBotClient client;
        static TgContext cont;
        static List<StateUser> uploads;
        static Method meth;
        static InlineKeyboardMarkup myInlineKeyboard;
        static List<string> categories;
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string Token = "1041731270:AAGrn5z0D32IboCzzienKH3YWbR34jBybD0";
            meth = new Method(Token);
            uploads = new List<StateUser>();
            categories = new List<string>();
            cont = new TgContext();
            foreach (var item in cont.Categories)
            {
                categories.Add(item.Name);
            }
            categories.Sort();
            foreach (var item in categories)
            {
            }
            client = new TelegramBotClient(Token);
            client.OnMessage += getMsgAsync;
            client.OnCallbackQuery += operateButtonsAsync;
            client.StartReceiving();
            DateTime date2 = DateTime.Now;
            Console.Read();
        }

        private static async void operateButtonsAsync(object sender, CallbackQueryEventArgs e)
        {
            long TgId = e.CallbackQuery.Message.Chat.Id;
            User user = FindUserInDb(TgId);
            StateUser upload = FindUpload(user);
            if(upload==null)
            {
                upload = new StateUser(user);
                uploads.Add(upload);
            }
            if (upload.State == 3)
            {
                if (e.CallbackQuery.Data == "Y")
                {
                    Category cat = FindCategory(upload.Category);
                    if (cat == null)
                    {
                        cat = new Category() { Name = upload.Category };
                        cont.Categories.Add(cat);
                        categories.Add(cat.Name);
                        categories.Sort();
                        await cont.SaveChangesAsync();
                    }
                    cont.Images.Add(new Image() { Category = cat, User = user, Path = upload.PhotoPath });
                    await cont.SaveChangesAsync();
                    upload.Success();
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Фото загружено успешно!");
                }
                else if (e.CallbackQuery.Data == "N")
                {
                    upload.SetPath(upload.PhotoPath);
                    await client.SendTextMessageAsync(TgId, "Введите категорию");
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Категория удалена!");
                }
                else
                {
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Не сработает");
                }
            }
            else if(upload.State==0)
            {
                if (e.CallbackQuery.Data == "forward"||e.CallbackQuery.Data == "back")
                {
                    int temp = upload.CurrentPage;
                    upload.CurrentPage += e.CallbackQuery.Data=="forward"?(upload.CurrentPage * 5 > categories.Count ? 0 : 1) : (upload.CurrentPage <= 1 ? 0 : -1);
                    List<List<InlineKeyboardButton>> inlines = new List<List<InlineKeyboardButton>>();
                    foreach(var item in GetCategories(upload.CurrentPage*5-4, upload.CurrentPage*5))
                    {
                        inlines.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton() { Text = item, CallbackData = $"#{item}" } });
                    }
                    inlines.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData("<","back"),
                        InlineKeyboardButton.WithCallbackData(">","forward")
                    });
                    var keys = new InlineKeyboardMarkup(inlines);
                    
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                    if(temp!=upload.CurrentPage)
                        await client.EditMessageTextAsync(TgId, e.CallbackQuery.Message.MessageId, "Выберите категорию:", replyMarkup: keys);
                }
                else if(e.CallbackQuery.Data[0]=='#')
                {
                    Category cat = FindCategory(e.CallbackQuery.Data.Substring(1));
                    if (cat != null)
                    {
                        foreach (var item in cat.Images)
                            await meth.SendPhotoIputFile(e.CallbackQuery.From.Id, item.Path, $"Published by {item.User.Nickname}");
                    }
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
                }
                else
                {
                    await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Не сработает");
                }
            }
            else
            {
                await client.AnswerCallbackQueryAsync(e.CallbackQuery.Id, text: "Не сработает");
            }
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
                await client.SendTextMessageAsync(TgId, "Загрузить фото на сервер - /upload");
                await client.SendTextMessageAsync(TgId, "Для получения фоток, введите категорию");
                await client.SendTextMessageAsync(TgId, "Для получения категорий, введите /categories");
                return;
            }
            StateUser upload = FindUpload(user);
            if (upload == null)
            {
                upload = new StateUser(user);
                uploads.Add(upload);
            }
            if (e.Message.Text == "/upload" && upload.State == 0)
            {
                await client.SendTextMessageAsync(TgId, "Скиньте фото для загрузки");
                upload.BeginProcess();
            }
            else if (upload.State != 0)
            {
                if (e.Message.Text != "/exit")
                {
                    if (upload.State == 1)
                    {
                        if (e.Message.Photo != null)
                        {
                            Telegram.Bot.Types.File test = await client.GetFileAsync(e.Message.Photo[e.Message.Photo.Length - 1].FileId);
                            string url = "https://api.telegram.org/file/bot1041731270:AAGrn5z0D32IboCzzienKH3YWbR34jBybD0/" + test.FilePath;
                            DateTime date = DateTime.Now;
                            string fileName = $@"photos\{e.Message.Chat.Username}_{date.ToShortDateString().Replace(".", "-")}_{date.ToLongTimeString().Replace(":", "-")}.png";
                            using (WebClient client = new WebClient())
                            {
                                client.DownloadFileAsync(new Uri(url), fileName);
                            }
                            upload.SetPath(fileName);
                            await client.SendTextMessageAsync(TgId, "Введите категорию");
                        }
                        else
                        {
                            await client.SendTextMessageAsync(TgId, "Скиньте картинку (не файлом)");
                        }
                    }
                    else if (upload.State == 2)
                    {
                        if (e.Message.Text != null)
                        {
                            string msg = e.Message.Text;
                            upload.SetCategory(msg);
                            var keys = new InlineKeyboardMarkup(
                            new InlineKeyboardButton[][]
                            {
                                new InlineKeyboardButton[] // First row
                                {
                                    InlineKeyboardButton.WithCallbackData("Да","Y"),
                                    InlineKeyboardButton.WithCallbackData("Нет","N")
                                }
                            })
                            { };
                            await client.SendTextMessageAsync(TgId, "Вы согласны с выбранной категорией?", replyMarkup: keys);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(TgId, "Пришлите текстовое сообщение с категорией");
                        }
                    }
                    else if (upload.State == 3)
                    {
                        await client.SendTextMessageAsync(e.Message.Chat.Id, "Нажмите на одну из кнопок");
                    }
                }
                else
                {
                    upload.Kill();
                    await client.SendTextMessageAsync(TgId, "Процесс отправки остановлен, фото не сохранено");
                }
            }
            else if (e.Message.Text == "/categories")
            {
                upload.CurrentPage = 1;
                List<List<InlineKeyboardButton>> inlines = new List<List<InlineKeyboardButton>>();
                foreach (var item in GetCategories(upload.CurrentPage * 5 - 4, upload.CurrentPage * 5))
                {
                    inlines.Add(new List<InlineKeyboardButton>() { new InlineKeyboardButton() { Text = item, CallbackData = $"#{item}" } });
                }
                inlines.Add(new List<InlineKeyboardButton>()
                    {
                        InlineKeyboardButton.WithCallbackData("<","back"),
                        InlineKeyboardButton.WithCallbackData(">","forward")
                    });
                var keys = new InlineKeyboardMarkup(inlines);
                await client.SendTextMessageAsync(TgId, "Выберите категорию:", replyMarkup: keys);
            }
            else if(e.Message.Text =="/exit")
            {
                await client.SendTextMessageAsync(TgId, "Использовать для остановки /upload");
            }
            else
            {
                Category cat = FindCategory(e.Message.Text);
                if (cat != null)
                {
                    foreach (var item in cat.Images)
                        await meth.SendPhotoIputFile(e.Message.Chat.Id, item.Path, $"Published by {item.User.Nickname}");
                }
                else
                {
                    await client.SendTextMessageAsync(TgId, $"Категория {e.Message.Text} не найдена. Может добавите фото с ней?\n/upload");
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
        private static StateUser FindUpload(User user)
        {
            foreach (var item in uploads)
            {
                if (item.User == user)
                {
                    return item;
                }
            }
            return null;
        }
        private static Category FindCategory(string Category)
        {
            foreach (var item in cont.Categories)
            {
                if (item.Name == Category)
                {
                    return item;
                }
            }
            return null;
        }
        private static List<string> GetCategories(int from, int to)
        {
            List<string> res = new List<string>();
            to = to > categories.Count ? categories.Count : to;
            for (int i = from - 1; i < to - 1; i++)
            {
                res.Add(categories[i]);
            }
            if(to>0)
                res.Add(categories[to-1]);
            else
                res.Add("Категорий нету");
            return res;
        }
    }
}