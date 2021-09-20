using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotificationBot.Models.Commands;
using NotificationBot.Models.Database;
using Telegram.Bot;

namespace NotificationBot.Models
{
    public class Bot
    {
        private static TelegramBotClient client;
        private static List<Command> commandsList;
        private static List<Button> menuButtons;

        public static IReadOnlyList<Command> Commands => commandsList.AsReadOnly();
        public static IReadOnlyList<Button> MenuButtons => menuButtons.AsReadOnly();

        public static async Task<TelegramBotClient> GetBotClientAsync()
        {
            if (client != null)
            {
                return client;
            }

            commandsList = new List<Command>();
            commandsList.Add(new StartCommand());
            commandsList.Add(new InfoCommand());
            commandsList.Add(new HelpCommand());

            menuButtons = new List<Button>();
            menuButtons.Add(new AllButton());
            menuButtons.Add(new FinishedButton());
            menuButtons.Add(new TodayButton());
            
            client = new TelegramBotClient(AppSettings.Key);
            var hook = string.Format(AppSettings.Url, "api/message/update");
            await client.SetWebhookAsync(hook);

            return client;
        }
    }
}