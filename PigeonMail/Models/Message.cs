namespace PigeonMail.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public Chat Chat { get; set; }
        public User User { get; set; }
        public DateTime Time { get; set; }
        public bool Read { get; set; }
    }
}
