using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotificationBot.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotificationBot.Models.Commands
{
    public class HelpCommand : Command
    {
        public override string Name => @"/help";
        
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
            
            await client.SendTextMessageAsync(chatId, "Вводить напоминание ты можешь в любой момент текстом, но " +
                                                      "обязательно вот в таком формате:\n" +
                                                      "Название; ДД.ММ.ГГ ЧЧ:ММ\n" +
                                                      "Ты можешь не писать год и время, если хочешь, чтобы я напоминал тебе" +
                                                      "об этих делах каждое утро.\nЕсли ты не напишешь дату," +
                                                      " напоминание создастся на сегодня.\nЕсли ты не напишешь время, но " +
                                                      "напишешь дату, я запишу в это уведомление дату, которая у тебя стоит" +
                                                      "в настройке \"Время для уведомлений\". Ее ты можешь изменить в настройках.",
                replyMarkup: tk.GetKeyboard(buttons.Default, false),
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }
    }
}