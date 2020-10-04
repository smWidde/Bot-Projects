namespace SendPics
{
    public class User
    {
        public int UserId { get; set; }
        public string Nickname { get; set; }
        public long TelegramId { get; set; }
        public virtual System.Collections.Generic.List<Image> Images { get; set; }
    }
}
