using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NotificationBot.Models;
using NotificationBot.Models.Database;
using Telegram.Bot.Types;

namespace NotificationBot.Logic
{
    public class NotificationMode
    {
        public string ResultParse(Reminder reminder)
        {
            string result = "";
            DateTime remDate = reminder.RemindDate;
            string done = "";
            
            if (reminder.IsDone)
                done = "✅ ";
            
            result += done + reminder.Name;
            
            if (remDate != DateTime.MinValue)
            {
                string day = remDate.Day.ToString();
                string month = remDate.Month.ToString();
                string hour = remDate.Hour.ToString();
                string minute = remDate.Minute.ToString();

                if (day.Length == 1) // красиво парсим день
                    day = "0" + day;

                if (month.Length == 1) // красиво парсим месяц
                    month = "0" + month;
                
                if (hour.Length == 1) // красиво парсим час
                    hour = "0" + hour;
                
                if (minute.Length == 1) // красиво парсим минуту
                    minute = "0" + minute;
                    
                string date = day + "." + month;
                
                if (remDate.Year == DateTime.Now.Year)
                {
                    date += "." + remDate.Year;
                }
                
                date += " " + hour + ":" + minute;
                
                result += " " + date;
            }

            return result;
        }

        public async Task SendNotifications(List<Reminder> remindersList, long chatId, string additional)
        {
            var botClient = await Bot.GetBotClientAsync();
            TelegramKeyboard tk = new TelegramKeyboard();
            
            if (remindersList == null || remindersList.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, 
                    additional + "\nДел нет!",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                return;
            }
            

            string result = "";
            Queue<Reminder> reminders = new Queue<Reminder>();

            int count = 1;

            foreach (Reminder rem in remindersList) // Создает 
            {
                result += count + ") " + ResultParse(rem) + "\n";
                reminders.Enqueue(rem);
                count++;
            }

            int rows = reminders.Count / 6 + 1; // получаем кол-во строк по 6 (максимум) цифр

            string[][] variantButtons = new string[rows][];
            string[][] callBack = new string[rows][];

            count = 1;

            try
            {
                for (int i = 0; i < rows; i++)
                {
                    int c; // счетчик 
                    if (i == rows - 1)
                        c = reminders.Count % 6;
                    else
                        c = 6;

                    variantButtons[i] = new string[c];
                    callBack[i] = new string[c];

                    for (int j = 0; j < variantButtons[i].Length; j++)
                    {
                        Reminder newRemider = reminders.Dequeue();
                        variantButtons[i][j] = count.ToString();
                        callBack[i][j] = newRemider.ReminderID.ToString();
                        count++;
                    }
                }

                await botClient.SendTextMessageAsync(chatId, 
                    additional + "\n" + result,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                    replyMarkup: tk.GetInlineKeyboard(variantButtons, callBack));
            }
            catch (Exception e)
            {
                await botClient.SendTextMessageAsync(chatId, 
                    "Ошибка!\n" + e.Message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
            }
        }
    }
}