using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NotificationBot.Logic;
using NotificationBot.Models.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotificationBot.Models
{
    public class TodayButton : Button
    {
        public override string Name => "Список дел на сегодня";
        public override bool Contains(Message message)
        {
            return message.Text.Equals(Name);
        }

        public override async Task Execute(Message message, TelegramBotClient client)
        {
            using (MyContext db = new MyContext())
            {
                NotificationMode nm = new NotificationMode();
                long chatId = message.Chat.Id;
                int userId = db.Users.First(x => x.ChatID == chatId).UserID;
                var remindersList = db.Reminders.Where(x => x.UserID == userId &&
                                                                       !x.IsDone && 
                                                                       (x.RemindDate == DateTime.MinValue || 
                                                                        x.RemindDate <= DateTime.Today.AddDays(1)))// && x.RemindDate >= DateTime.Today))
                    .OrderBy(x => x.RemindDate).ToList();
                            
                await nm.SendNotifications(remindersList, chatId, "<b>Список дел на сегодня:</b>");
            }
        }
    }
}