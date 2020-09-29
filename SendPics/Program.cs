using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using System.Drawing;
using System.Net;

namespace SendPics
{
    
    
    class TgContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }
    }
    class StateOfUpload
    {
        public User User { get; set; }
        public int State { get; private set; }
        public string PhotoPath { get; set; }
        public string Category { get; set; }
        public StateOfUpload(User user)
        {
            this.User = user;
            State = 0;
            PhotoPath = "";
            Category = "";
        }
        public void Kill()
        {
            if (State > 1)
            {
                File.Delete(PhotoPath);
                PhotoPath = "";
            }
            Category = "";
            State = 0;
        }
        public void BeginProcess()
        {
            State = 1;
        }
        public void SetPath(string PhotoPath)
        {
            this.PhotoPath = PhotoPath;
            State = 2;
        }
        public void SetCategory(string Category)
        {
            this.Category = Category;
            State = 3;
        }
        public void Success()
        {
            if (State == 3)
            {
                State = 0;
                PhotoPath = "";
                Category = "";
            }
        }
    }
    class Program
    {
        static TelegramBotClient client;
        static TgContext cont;
        static List<StateOfUpload> uploads;
        static Method meth;
        static void Main(string[] args)
        {
            string Token = "1041731270:AAGrn5z0D32IboCzzienKH3YWbR34jBybD0";
            meth = new Method(Token);
            uploads = new List<StateOfUpload>();
            cont = new TgContext();
            cont.SaveChanges();
            client = new TelegramBotClient(Token);
            client.OnMessage += getMsgAsync;
            client.StartReceiving();
            DateTime date2 = DateTime.Now;
            Console.WriteLine(date2.ToLongTimeString());
            Console.Read();
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
                StateOfUpload tempUpload = new StateOfUpload(user);
                uploads.Add(tempUpload);
                await client.SendTextMessageAsync(TgId, "Загрузить фото на сервер - /upload");
                await client.SendTextMessageAsync(TgId, "Для получения фоток, введите категорию (вне процесса загрузки машины)");
                return;
            }
            StateOfUpload upload = FindUpload(user);
            if (upload == null)
            {
                upload = new StateOfUpload(user);
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
                    if (e.Message.Photo != null && upload.State == 1)
                    {
                        Telegram.Bot.Types.File test = await client.GetFileAsync(e.Message.Photo[e.Message.Photo.Length - 1].FileId);
                        string url = "https://api.telegram.org/file/bot1041731270:AAGrn5z0D32IboCzzienKH3YWbR34jBybD0/" + test.FilePath;
                        DateTime date = DateTime.Now;
                        string fileName = $@"photos\{e.Message.Chat.Username}_{date.ToShortDateString().Replace(".", "-")}_{date.ToLongTimeString().Replace(":","-")}.png";
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFileAsync(new Uri(url), fileName);
                        }
                        upload.SetPath(fileName);
                        await client.SendTextMessageAsync(TgId, "Выберите категорию");
                    }
                    else if (e.Message.Text != null && upload.State == 2)
                    {
                        string msg = e.Message.Text;
                        upload.SetCategory(msg);
                        await client.SendTextMessageAsync(TgId, "Вы согласны с выбранной категорией?");
                    }
                    else if (upload.State == 3)
                    {
                        if (e.Message.Text=="Y")
                        {
                            Category cat = FindCategory(upload.Category);
                            if (cat == null)
                            {
                                cat = new Category() { Name = upload.Category };
                                cont.Categories.Add(cat);
                                await cont.SaveChangesAsync();
                            }
                            cont.Images.Add(new Image() { Category = cat, User = user, Path = upload.PhotoPath });
                            await cont.SaveChangesAsync();
                            upload.Success();
                            await client.SendTextMessageAsync(TgId, "Фото загружено успешно!");
                        }
                        //else if(e.Message.Text=="N")
                        //{

                        //}
                        //else
                        //{
                        //    await client.SendTextMessageAsync(e.Message.Chat.Id, "Вы согласны с выбранной категорией, Y/N?");
                        //}
                    }
                }
                else
                {
                    upload.Kill();
                    await client.SendTextMessageAsync(TgId, "Процесс отправки остановлен, фото не сохранено");
                }
            }
            else
            {
                Category cat = FindCategory(e.Message.Text);
                if (cat != null)
                {
                    foreach (var item in cat.Images)
                        await meth.SendPhotoIputFile(e.Message.Chat.Id, item.Path);
                }
            }
            Console.WriteLine(e.Message.Type.ToString());
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
        private static StateOfUpload FindUpload(User user)
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
    }
}
