namespace NotificationBot.Models
{
    public class AppSettings
    {
        public static string Url { get; set; } = "https://<domain>:<port>/{0}";
        public static string Name { get; set; } = "your_notification_bot";
        public static string Key { get; set; } = "<bot_token>";

        public static string ConnectionString { get; set; } =
            "Server=localhost;Database=<database_name>;User Id=<username>; Password = <password>;";
    }
}