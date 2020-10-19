namespace PrivatCurrency
{
    public class User
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public long TelegramId { get; set; }
        public short ChosenCurrencies { get; set; }
        public virtual Interval Interval { get; set; }
        public string BotVersion { get; set; }
    }
}
