using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NotificationBot.Models
{
    public abstract class Button
    {
        public abstract string Name { get; }
        
        public abstract bool Contains(Message message);

        public abstract Task Execute(Message message, TelegramBotClient client);
    }
}