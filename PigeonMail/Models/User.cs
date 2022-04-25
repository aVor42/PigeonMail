using Microsoft.AspNetCore.Identity;

namespace PigeonMail.Models
{
    public class User: IdentityUser
    {
        public User()
        {
            Chats = new List<Chat>();
        }

        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public byte[] Photo { get; set; }

        public List<Chat> Chats { get; set; }

    }
}
