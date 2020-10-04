namespace PrivatCurrency
{
    public class User
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public long TelegramId { get; set; }
        public short ChosenCurrencies { get; set; }
        public int IntervalMinutes { get; set; }
        public int SendFromMinutes { get; set; }
        public int SendToMinutes { get; set; }
        public int WhenToSendMinutes { get; set; }
    }
}
