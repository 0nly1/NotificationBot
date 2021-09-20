using System.Collections.Generic;
using System.Linq;
using NotificationBot.Models.Database;

namespace NotificationBot.Logic
{
    public class Buttons
    {
        public string[][] Default =
        {
            new[] {"Список дел на сегодня"},
            new[] {"Список дел"},
            new[] {"Выполненные дела"}
            //new[] {"Настройки"}
        };
        
        public string[][] Settings =
        {
            new[] {"Часовой пояс"},
            new[] {"Время для уведомлений"}
        };
    }
}