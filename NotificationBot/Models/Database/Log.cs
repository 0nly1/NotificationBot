using System;

namespace NotificationBot.Models.Database
{
    public class Log
    {
        public int LogID { get; set; }
        
        public string Text { get; set; }
        
        public DateTime Date { get; set; }
    }
}