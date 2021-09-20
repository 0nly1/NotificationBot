using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotificationBot.Logic;
using NotificationBot.Models;
using NotificationBot.Models.Database;

namespace NotificationBot.Controllers
{
    public class TimerController : ControllerBase
    {
        // Метод для cron'а
        public async Task<OkResult> CheckNotifications()
        {
            using (MyContext db = new MyContext())
            {
                // long chatId = 254744873; // для теста
                var botClient = await Bot.GetBotClientAsync();
                TimeSpan minute = new TimeSpan(0, 1, 0);
                NotificationMode nm = new NotificationMode();
                TelegramKeyboard tk = new TelegramKeyboard();
                
                // Два вида уведомлений: по времени и по "дефолтному" времени
                
                // список пользователей, у которых напоминание по дефолту в это время
                List<User> defaultUsers = db.Users.Where(x => x.DefaultNotification <= DateTime.Now.TimeOfDay && 
                                                              x.DefaultNotification.Add(minute) >= DateTime.Now.TimeOfDay &&
                                                              x.IsDefaultNotificationsOn)
                    .ToList();

                // все пользователи
                List<User> users = db.Users.ToList();
                
                // список прошедших напоминаний + напоминания на день (= напоминание на это время) на это время 
                //List<Reminder> oldReminderList = db.Reminders.Where(x => x.RemindDate.Date <= DateTime.Today.AddDays(1) && 
                //                                                      !x.IsDone).ToList();
                
                // список напоминаний без дат + напоминания на сегодня
                List<Reminder> reminderList = db.Reminders.Where(x => (x.RemindDate == DateTime.MinValue ||
                                                                      x.RemindDate >= DateTime.Today &&
                                                                      x.RemindDate <= DateTime.Today.AddDays(1)) && 
                                                                      !x.IsDone).ToList();

                // списпок напоминаний на это время
                List<Reminder> actualReminders = db.Reminders.Where(x => x.RemindDate <= DateTime.Now &&
                                                                         x.RemindDate.Add(minute) >= DateTime.Now &&
                                                                         !x.IsDone).ToList();
                
                int c; // counter
                string notification;

                #region "Дефолтные" напоминания
                
                foreach (var user in defaultUsers) // для дефолтных напоминалок
                {
                    // Напоминания конкретного пользователя
                    List<Reminder> userReminders = reminderList.Where(x => x.UserID == user.UserID).ToList();

                    c = userReminders.Count;

                    if (c >= 1)
                    {
                        // TODO присылать разные сообщения с подбадриванием
                        string morningMessage = "Доброе утро! :)\n<b>Вот твои дела на сегодня:</b>\n";
                        await nm.SendNotifications(userReminders, user.ChatID, morningMessage);
                    }
                    else if (c == 0 && user.IsEmptyNotificationsOn)
                    {
                        await botClient.SendTextMessageAsync(user.ChatID,
                            "Доброе утро! :)\n<b>Дел на сегодня нет!</b>",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                }
                #endregion

                #region Напоминание по времени
                
                foreach (var user in users) // для временных напоминалок
                {
                    foreach (var reminder in actualReminders.Where(reminder => reminder.UserID == user.UserID))
                    {
                        notification = nm.ResultParse(reminder) + ";\n";
                            
                        string[][] variantButtons =
                        {
                            new[] {"Удалить"}, // "Изменить"},
                            new[] {"Выполнить"}
                        };

                        string[][] callBack =
                        {
                            new[] {"1-" + reminder.ReminderID}, // "2-"+rem.ReminderID},
                            new[] {"3-" + reminder.ReminderID},
                        };
                            
                        await botClient.SendTextMessageAsync(user.ChatID,
                            "<b>Напоминаю!</b>\n" + notification,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                            replyMarkup: tk.GetInlineKeyboard(variantButtons, callBack));
                    }
                }
                #endregion
                
                return Ok();
            }
        }
    }
}