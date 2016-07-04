using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class NotificationTelegram is a specific implementation for 
    /// sending messages via a Telegram messenger bot; see website 
    /// of provider for more information: https://core.telegram.org/bots/api
    /// </summary>
    public class NotificationTelegram : Notification
    {
        // The bot I use here is https://telegram.me/PushMonBot; feel free to create your 
        // own telegram bot and change the bot key here
        private const string botKey = "269024855:AAHSpgytxGAtnz3s_gJbqHjnXxO0tYGArv8";

        public NotificationTelegram(string chatId, string message, string title)
            : base(chatId, message, title)
        {
            Initialize();
        }

        public NotificationTelegram(string chatId, string message, string title, bool isImportant, bool isSilent)
            : base(chatId, message, title, isImportant, isSilent)
        {
            Initialize();
        }

        private void Initialize()
        {
            apiUrl = String.Format("https://api.telegram.org/bot{0}/sendMessage", botKey);

            if(Send())
            {
                NotificationSuccessfulSend = true;
            }
        }

        protected override bool Send()
        {
            try
            {
                using (var client = new WebClient())
                {
                    NameValueCollection payload = new NameValueCollection();
                    payload["chat_id"] = rcpt;
                    payload["text"] = String.Format("*{0}*\n{1}", 
                        title.Length > 250 ? title.Substring(0, 250) : title, 
                        message.Length > 1024 ? message.Substring(0, 1024) : message);
                    payload["parse_mode"] = "Markdown";
                    if (isSilent && !isImportant)
                    {
                        payload["disable_notification"] = isSilent.ToString().ToUpper();
                    }

                    byte[] response = client.UploadValues(apiUrl, "POST", payload);
                    using (StreamReader reader = new StreamReader(new MemoryStream(response)))
                    {
                        NotificationWebResponse = reader.ReadToEnd();
                    }
                }
                return true;
            }
            catch (WebException exWeb)
            {
                using (StreamReader reader = new StreamReader(exWeb.Response.GetResponseStream()))
                {
                    NotificationWebResponse = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                // TODO: Implement error handling
            }
            return false;
        }
    }
}
