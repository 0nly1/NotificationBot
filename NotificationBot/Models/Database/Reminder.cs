using System;

namespace NotificationBot.Models.Database
{
    public class Reminder
    {
        public int ReminderID { get; set; }
        public string Name { get; set; }
        //public string Extra { get; set; }
        public DateTime RemindDate { get; set; }
        public int UserID { get; set; }
        public bool IsDone { get; set; }
    }
}