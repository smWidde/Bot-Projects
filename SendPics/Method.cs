using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SendPics
{
    class Method : IDisposable
    {
        string _token;
        string LINK = "https://api.telegram.org/bot";
        public Method(string Token)
        {
            _token = Token;
        }
        public string Getme()
        {
            using (WebClient webClient = new WebClient())
            {
                string response = webClient.DownloadString(LINK + _token + "/getMe");
                return response;
            }
        }
        async public Task SendPhotoIputFile(long ChatID, string pathToPhoto, string catprion = "")
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string url = LINK + _token + "/sendPhoto";
                string fileName = pathToPhoto.Split('\\').Last();

                form.Add(new StringContent(ChatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(catprion.ToString(), Encoding.UTF8), "caption");
                using (FileStream fileStream = new FileStream(pathToPhoto, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "photo", fileName);
                    using (HttpClient client = new HttpClient())
                        await client.PostAsync(url, form);
                }
            }

        }
        async public Task SendAudioIputFile(long ChatID, string pathToAudio, string catprion = "", int duration = 0, string performer = "", string title = "")
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string url = LINK + _token + "/sendAudio";
                string fileName = pathToAudio.Split('\\').Last();

                form.Add(new StringContent(ChatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(catprion.ToString(), Encoding.UTF8), "caption");
                form.Add(new StringContent(duration.ToString(), Encoding.UTF8), "duration");
                form.Add(new StringContent(performer.ToString(), Encoding.UTF8), "performer");
                form.Add(new StringContent(title.ToString(), Encoding.UTF8), "title");
                using (FileStream fileStream = new FileStream(pathToAudio, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "audio", fileName);
                    using (HttpClient client = new HttpClient())
                        await client.PostAsync(url, form);
                }
            }

        }
        async public Task SendDocumentIputFile(long ChatID, string pathToDocument, string catprion = "")
        {
            using (MultipartFormDataContent form = new MultipartFormDataContent())
            {
                string url = LINK + _token + "/sendDocument";
                string fileName = pathToDocument.Split('\\').Last();

                form.Add(new StringContent(ChatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(catprion.ToString(), Encoding.UTF8), "caption");
                using (FileStream fileStream = new FileStream(pathToDocument, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "document", fileName);
                    using (HttpClient client = new HttpClient())
                        await client.PostAsync(url, form);
                }
            }

        }
        async public Task SendVideoInputFile(long chatID, string pathToVideo, string caption = "")
        {
            using (var form = new MultipartFormDataContent())
            {
                string url = string.Format("https://api.telegram.org/bot{0}/sendVideo", _token);
                string fileName = pathToVideo.Split('\\').Last();

                form.Add(new StringContent(chatID.ToString(), Encoding.UTF8), "chat_id");
                form.Add(new StringContent(caption, Encoding.UTF8), "caption");

                using (FileStream fileStream = new FileStream(pathToVideo, FileMode.Open, FileAccess.Read))
                {
                    form.Add(new StreamContent(fileStream), "video", fileName);
                    using (var client = new HttpClient())
                    {
                        await client.PostAsync(url, form);
                    }
                }
            }
        }
        public void Dispose()
        {
            this._token = "";

        }
    }
}
