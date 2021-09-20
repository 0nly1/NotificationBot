using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NotificationBot.Controllers;
using NotificationBot.Models.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotificationBot.Models.Commands
{
    public class StartCommand : Command
    {
        //public Timer timer;

        public override string Name => "/start";
        
        public override bool Contains(Message message)
        {
            if (message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                return false;
            return message.Text.Contains(this.Name);
        }
        
        public override async Task Execute(Message message, TelegramBotClient client)
        {
            long chatId = message.Chat.Id;
            InfoCommand info = new InfoCommand();

            try
            {
                using (MyContext db = new MyContext())
                {
                    db.Database.EnsureCreated();
                    
                    if (db.Users.FirstOrDefault(x => x.ChatID == chatId) == null)
                    {
                        Database.User user = new Database.User();

                        user.ChatID = chatId;
                        user.DefaultNotification = new TimeSpan(9, 0, 0);
                        user.TimeZoneID = 1;
                        
                        db.Add(user);
                        db.SaveChanges();

                        await client.SendTextMessageAsync(chatId, "Привет! Поздравляю с первой регистрацией!",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        
                        await info.Execute(message, client);
                    }
                    else
                    {
                        await client.SendTextMessageAsync(chatId, 
                            "Ты уже зарегистирован!",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                }
            }
            catch (Exception e)
            {
                await client.SendTextMessageAsync(chatId, e.Message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }
    }
}