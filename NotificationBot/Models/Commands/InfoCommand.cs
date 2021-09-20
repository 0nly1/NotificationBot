using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NotificationBot.Models.Database;
using NotificationBot.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotificationBot.Models.Commands
{
    public class InfoCommand : Command
    {
        public override string Name => @"/info";
        
        public override bool Contains(Message message)
        {
            if (message.Type != Telegram.Bot.Types.Enums.MessageType.Text)
                return false;
            return message.Text.Contains(this.Name);
        }
        public override async Task Execute(Message message, TelegramBotClient client)
        {
            long chatId = message.Chat.Id;
            TelegramKeyboard tk = new TelegramKeyboard();
            Buttons buttons = new Buttons();
            
            try
            {
                await client.SendTextMessageAsync(chatId, "Приветствую тебя в версии Аркаши v1.1!\n\n" +
                                                          "<b>Какие функции сейчас доступны:</b>\n" +
                                                          "- Создание напоминания;\n" +
                                                          "- Уведомление о планах в определенное время;\n" +
                                                          "- Вывод списка напоминаний на сегодня, все невыполненные и только выполненные;\n" +
                                                          "- Удаление созданного напоминания;\n" +
                                                          "- Отметить напоминание как выполненное;\n" +
                                                          "- Отметить напоминание как невыполненное.\n\n" +
                                                          "<b>Какие функции ожидать в ближаших обновлениях:</b>\n" +
                                                          "- Изменение созданных напоминаний;\n" +
                                                          "- Настройки часового пояса и времени \"по умолчанию\";\n\n" +
                                                          "Если у тебя есть какие-то предложения/замечания, напиши моему " +
                                                          "создателю @namord_nick", 
                    replyMarkup: tk.GetKeyboard(buttons.Default, false),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                
                
                
                    
            }
            catch (Exception e)
            {
                string error = "";
                if (e.InnerException.Message != null)
                {
                    error = e.InnerException.Message;
                }

                await client.SendTextMessageAsync(chatId, "Если ты видишь это сообщение, перешли" +
                                                             "его моему создателю @namord_nick.\n" +
                                                             e.Message + " " + error,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Default);
            }
        }
    }
}