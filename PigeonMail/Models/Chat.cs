namespace PigeonMail.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public List<User> Members { get; set; }
        public List<Message> Messages { get; set; }
    }
}
