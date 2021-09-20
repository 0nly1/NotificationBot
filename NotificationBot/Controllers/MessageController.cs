using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationBot.Logic;
using NotificationBot.Models;
using NotificationBot.Models.Database;
using Telegram.Bot.Types.Enums;
using TimeZone = NotificationBot.Models.Database.TimeZone;

namespace NotificationBot.Controllers
{
    [Route (@"api/message/update")]
    public class MessageController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "Ok";
        }

        [HttpPost]
        public async Task<OkResult> Post([FromBody] Update update)
        {
            using (MyContext db = new MyContext())
            {
                var botClient = await Bot.GetBotClientAsync();
                NotificationMode nm = new NotificationMode();
                TelegramKeyboard tk = new TelegramKeyboard();
                Buttons buttons = new Buttons();

                //await db.Logs.AddAsync(new Log() {Date = DateTime.Now, Text = "Получил сообщение"});
                
                if (update == null)
                {
                    return Ok();
                }

                if (update.Type == UpdateType.Message) // Message Zone
                {
                    var commands = Bot.Commands;
                    var message = update.Message;
                    long chatId = message.Chat.Id;
                    
                    //await db.Logs.AddAsync(new Log {Date = DateTime.Now, Text = $"Сообщение {message.Text}"});
                    //await db.Logs.AddAsync(new Log {Date = DateTime.Now, Text = "Проверяю на команды"});
                    
                    foreach (var command in commands)
                    {
                        if (command.Contains(message))
                        {
                            await command.Execute(message, botClient);
                            return Ok();
                        }
                    }

                    #region Cекретное сообщение

                    string codeAnswer = "Ответь на это сообщение то, что хотел бы отправить."; // для меня
                    
                    if (message.Text == "Send message to Kate") // Секретное сообщение
                    {
                        if (chatId == 254744873)
                        {
                            await botClient.SendTextMessageAsync(chatId, codeAnswer,
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return Ok();
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, "У тебя нет таких прав.",
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return Ok();
                        }
                    }

                    if (message.ReplyToMessage != null)
                    {
                        if (message.ReplyToMessage.Text == codeAnswer && chatId == 254744873)
                        {
                            await botClient.SendTextMessageAsync(310775301, message.Text,
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return Ok();
                        }
                    }

                    #endregion
                    
                    Models.Database.User user = db.Users.FirstOrDefault(x => x.ChatID == chatId);
                    
                    if (user == null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Я не понял тебя, введи сообщение " +
                            "по шаблону (если возникли проблемы, введи " +
                            "команду /help).",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    
                    List<Models.Database.TimeZone> timeZoneList = db.TimeZones.ToList();
                    
                    // TODO: доделать
                    #region Настройки времени
                    /*
                    foreach (var timeZone in timeZoneList)
                    {
                        if (timeZone.TimeZoneName == message.Text)
                        {
                            db.Users.FirstOrDefault(x => x.UserID == user.UserID).TimeZoneID =
                                timeZone.TimeZoneID;
                            db.SaveChanges();
                            
                            // TODO: изменять все время в бд (включая дефолтное время) в зависимости от этого часового пояса
                            
                            await botClient.SendTextMessageAsync(chatId, 
                                "Ваш регион сменен на " + timeZone.TimeZoneName,
                                replyMarkup: tk.GetKeyboard(buttons.Default, false),
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
                            
                            return Ok();
                        }
                    }
                    */
                    #endregion
                    
                    var menuButtons = Bot.MenuButtons;
                    
                    foreach (var button in menuButtons)
                    {
                        if (button.Contains(message))
                        {
                            await button.Execute(message, botClient);
                            return Ok();
                        }
                    }

                    #region Парсер
                    
                    // TODO: Парсить без шаблона
                    // Шаблон: Название; ДД.ММ.ГГ ЧЧ:ММ

                    // TODO: Если введена дата прошедшая, то дата ставится следующий год

                    Reminder reminder = new Reminder();

                    // переменные для даты
                    int day = DateTime.Now.Day;
                    int month = DateTime.Now.Month;
                    int year = DateTime.Now.Year;
                    int hours = 0;
                    int minutes = 0;

                    DateTime dateTime = DateTime.MinValue;

                    string pattern =
                        @"^(.{1,});\s{0,1}(\d+[.]\d+[.]\d+|\d+[.]\d+){0,1}\s{0,1}(\d+[:]\d+){0,1}$";
                    string datePattern = @"(\d+)[.](\d+)[.]{0,1}(\d{0,4})";
                    string timePattern = @"(\d+)[:](\d+)";

                    Regex regex = new Regex(pattern);
                    Match match = regex.Match(message.Text);

                    if (match.Groups[1].Value == "")
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            "Я не понял тебя, введи сообщение " +
                            "по шаблону (если возникли проблемы, введи " +
                            "команду /help).",
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        return Ok();
                    }

                    if (match.Groups[2].Value != "") // Добавление даты, если есть
                    {
                        Regex dateParse = new Regex(datePattern);
                        Match date = dateParse.Match(match.Groups[2].Value);

                        day = Convert.ToInt32(date.Groups[1].Value);
                        month = Convert.ToInt32(date.Groups[2].Value);

                        string stringYear = date.Groups[3].Value;

                        if (!string.IsNullOrEmpty(stringYear))
                        {
                            if (stringYear.Length == 2)
                                stringYear = "20" + stringYear;
                            
                            year = Convert.ToInt32(stringYear);
                        }

                        // добавление времени, если есть
                        if (match.Groups[3].Value != "") 
                        {
                            Regex timeParse = new Regex(timePattern);
                            Match time = timeParse.Match(match.Groups[3].Value);

                            hours = Convert.ToInt32(time.Groups[1].Value);
                            minutes = Convert.ToInt32(time.Groups[2].Value);

                            try
                            {
                                dateTime = new DateTime(year, month, day, hours, minutes, 0);
                            }
                            catch
                            {
                                Console.WriteLine("Неправильно введена дата");
                            }
                        }
                        else
                        {
                            hours = user.DefaultNotification.Hours;
                            minutes = user.DefaultNotification.Minutes;
                            
                            try
                            {
                                dateTime = new DateTime(year, month, day, hours, minutes, 0);
                            }
                            catch
                            {
                                Console.WriteLine("Неправильно введена дата");
                                return Ok();
                            }
                        }
                    }
                    else if (match.Groups[3].Value != "") // добавление времени БЕЗ даты
                    {
                        Regex timeParse = new Regex(timePattern);
                        Match time = timeParse.Match(match.Groups[3].Value);

                        hours = Convert.ToInt32(time.Groups[1].Value);
                        minutes = Convert.ToInt32(time.Groups[2].Value);

                        try
                        {
                            dateTime = new DateTime(year, month, day, hours, minutes, 0);
                        }
                        catch
                        {
                            Console.WriteLine("Неправильно введена дата");
                        }
                    }

                    reminder.Name = match.Groups[1].Value;
                    reminder.RemindDate = dateTime; // TODO поменять на адаптирующееся
                    reminder.UserID = db.Users.FirstOrDefault(x => x.ChatID == chatId).UserID;
                    reminder.IsDone = false;

                    db.Reminders.Add(reminder);

                    try
                    {
                        db.SaveChanges();

                        reminder = db.Reminders.OrderByDescending(x => x.ReminderID).First();

                        await botClient.SendTextMessageAsync(chatId, 
                            "<b>Создано новое напоминание:</b>\n" +
                            nm.ResultParse(reminder),
                            parseMode: ParseMode.Html);
                    }
                    catch (Exception e)
                    {
                        string error = "";
                        if (e.InnerException.Message != null)
                        {
                            error = e.InnerException.Message;
                        }

                        await botClient.SendTextMessageAsync(chatId, 
                            "Если ты видишь это сообщение, перешли" +
                            "его моему создателю @namord_nick.\n" +
                            e.Message + " " + error,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
                        return Ok();
                    }
                    return Ok();
                    
                    #endregion
                }
                
                if (update.Type == UpdateType.CallbackQuery) // Callback Query Zone
                {
                    long chatId = update.CallbackQuery.Message.Chat.Id;
                    
                    try
                    {
                        int messageId = update.CallbackQuery.Message.MessageId;
                        string message = update.CallbackQuery.Data;

                        Models.Database.User user = db.Users.FirstOrDefault(x => x.ChatID == chatId);
                        List<Reminder> remindersList = db.Reminders.Where(x => x.UserID == user.UserID).ToList();
                        Reminder reminder = new Reminder();
                        
                        // Первая цифра - действие, которое хочет совершить пользователь (1 - удалить,
                        // 2 - удалить, 3 - выполнить, 4 - отмена всего и возврат в прежнее состояние),
                        // Второе число - ID напоминания
                        
                        #region Вывод выбранного напоминания

                        int code; // проверка, является ли кодом (callBack). Выводит сообщение с напоминанием
                        if (int.TryParse(message, out code))
                        {
                            foreach (Reminder rem in remindersList)
                            {
                                if (code == rem.ReminderID)
                                {
                                    string[][] variantButtons =
                                    {
                                        new[] {"Удалить"}, // "Изменить"},
                                        new[] {"Выполнить"}
                                    };

                                    string[][] callBack =
                                    {
                                        new[] {"1-" + rem.ReminderID}, // "2-"+rem.ReminderID},
                                        new[] {"3-" + rem.ReminderID},
                                    };

                                    if (rem.IsDone)
                                    {
                                        variantButtons[variantButtons.Length-1][0] = "Невыполнено";
                                        callBack[callBack.Length-1][0] = "4-" + rem.ReminderID;
                                    }

                                    await botClient.SendTextMessageAsync(chatId,
                                        nm.ResultParse(rem),
                                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                                        replyMarkup: tk.GetInlineKeyboard(variantButtons, callBack));
                                    await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                                    
                                    return Ok();
                                }
                            }

                            return Ok();
                        }

                        #endregion

                        #region Удаление/Изменение/Выполнено/Невыполнено

                        string pattern = @"^([1-4])-([0-9]*)$";

                        Regex r = new Regex(pattern);
                        Match m = r.Match(message);

                        foreach (Reminder rem in remindersList)
                        {
                            if (rem.ReminderID.ToString() == m.Groups[2].Value)
                            {
                                reminder = rem;
                                break;
                            }
                        }

                        if (m.Groups[1].Value == "1") // Удаление
                        {
                            // TODO: Сделать подтверждение удаления

                            db.Remove(reminder);
                            db.SaveChanges();

                            await botClient.EditMessageTextAsync(chatId, messageId, 
                                "Напоминание удалено!",
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                            return Ok();
                        }

                        if (m.Groups[1].Value == "2") // Изменение
                        {
                            // TODO: Реализовать изменение
                            // (?) Спрашиваем, что пользователь хочет выбрать: Имя, Дату или Время (?)
                            await botClient.SendTextMessageAsync(chatId, "Тут должно быть изменение...",
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                            return Ok();
                        }

                        if (m.Groups[1].Value == "3") // Выполнение
                        {
                            db.Reminders.FirstOrDefault(x => x.ReminderID == reminder.ReminderID).IsDone = true;
                            db.SaveChanges();
                            
                            await botClient.EditMessageTextAsync(chatId, messageId, 
                                nm.ResultParse(reminder), 
                                parseMode: ParseMode.Markdown);
                            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                            return Ok();
                        }
                        
                        if (m.Groups[1].Value == "4") // Невыполнено
                        {
                            db.Reminders.FirstOrDefault(x => x.ReminderID == reminder.ReminderID).IsDone = false;
                            db.SaveChanges();
                            
                            await botClient.EditMessageTextAsync(chatId, messageId, 
                                nm.ResultParse(reminder), 
                                parseMode: ParseMode.Markdown);
                            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                            return Ok();
                        }

                        #endregion
                    }
                    catch (Exception e)
                    {
                        
                        string error = "";
                        if (e.InnerException.Message != null)
                        {
                            error = e.InnerException.Message;
                        }
                        
                        await botClient.SendTextMessageAsync(chatId, 
                            "Если вы видите это сообщение, пожалуйста," +
                            "перешлите его @namord_nick\n\n" + 
                            e.Message + " " + error,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
                    }
                }

                return Ok();
            }
        }
    }
}
