using System;

namespace NotificationBot.Models.Database
{
    public class User
    {
        public int UserID { get; set; }
        
        public long ChatID { get; set; }
        
        public int TimeZoneID { get; set; } // Default time zone
        
        public TimeSpan DefaultNotification { get; set; } // Default time for notifications with no time
        
        public bool IsSetting { get; set; } // мб переделаю в полноценный режим Actions
        
        public bool IsDefaultNotificationsOn { get; set; } // включено ли получение уведомлений в дефолтное время
        
        public bool IsEmptyNotificationsOn { get; set; } // получать ли пустое дефолтное уведомление
    }
}