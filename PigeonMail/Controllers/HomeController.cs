using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PigeonMail.Data;
using PigeonMail.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace PigeonMail.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("/Identity/Account/Login");

            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var response = new List<Dictionary<string, object>>();

            var user =  _context.Users.
                Include(u => u.Chats).
                FirstOrDefault(u => u.Id == userId);
            var chats = _context.Chats.
                Include(c => c.Members).
                Include(c => c.Messages).
                Where(c => c.Members.
                Contains(user));

            foreach(var chat in chats)
            {
                var member = chat.Members.FirstOrDefault(u => u.Id != userId);
                var unreadMessagesCount = chat.Messages.Where(m => !m.Read && m.User.Id != userId).Count();

                response.Add(new Dictionary<string, object>
                {
                    ["Id"] = chat.Id.ToString(),
                    ["ChatName"] = member.FirstName + " " + member.LastName,
                    ["IdMember"] = member.Id,
                    ["UnreadMessagesCount"] = unreadMessagesCount
                });
            }
            return View(response);
                
        }


        public IActionResult Chat(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("/Identity/Account/Login");

            var currentUserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var chat = _context.Chats.
                Include(c => c.Members).
                Include(c => c.Messages).
                FirstOrDefault(c => 
                c.Id == id &&
                c.Members.FirstOrDefault(m => m.Id == currentUserId) != null);

            if (chat == null)
                return NotFound();
            
            if(chat.Members.Count == 2)
            {
                var member = chat.Members.
                    FirstOrDefault(m => m.Id != currentUserId);
                ViewData["ChatName"] = member.FirstName + " " + member.LastName;
            }

            return View(chat);
        }


        public IActionResult Search()
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("/Identity/Account/Login");
            return View();
        }

        public IActionResult SearchUsers(string searchValue)
        {
            if (string.IsNullOrEmpty(searchValue))
                return BadRequest();

            var currentUserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUserChatMembers = _context.Chats.
                Include(u => u.Members).
                Where(c => c.Members.FirstOrDefault(m => m.Id == currentUserId) != null).
                Select(c => c.Members.FirstOrDefault(m => m.Id != currentUserId)).ToList();

            searchValue = searchValue.Trim().ToLower();

            var getResponseEntry = (string id, string fullName, bool hasChat) => new Dictionary<string, object>
            {
                ["Id"] = id,
                ["FullName"] = fullName,
                ["HasChat"] = hasChat
            };

            var result = _context.Users.
                Include(u => u.Chats).
                Where(u => u.Id != currentUserId &&
                (u.FirstName.ToLower().Contains(searchValue) ||
                u.LastName.ToLower().Contains(searchValue))).
                Select(u => getResponseEntry(u.Id, u.FirstName + " " + u.LastName, currentUserChatMembers.Contains(u))).
                ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddChatWithUser(string id)
        {
            var firstUserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var firstUser = _context.Users.FirstOrDefault(u => u.Id == firstUserId);
            var secondUser = _context.Users.FirstOrDefault(u => u.Id == id);
            
            if(firstUser == null || secondUser == null)
                return BadRequest();

            var currentChats = _context.Chats.
                Include(c => c.Members).
                Where(c =>
                c.Members.Contains(firstUser) &&
                c.Members.Contains(secondUser));

            if (currentChats.Count() > 0)
                return BadRequest();

            _context.Chats.Add(new Chat
            {
                Members = new List<User>
                {
                    firstUser,
                    secondUser
                }
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SandMessage(string message, string connectionId, int chatId)
        {
            var userId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (currentUser == null)
                return BadRequest();

            var chat = _context.Chats.
                Include(c => c.Members).
                Include(c => c.Messages).
                FirstOrDefault(c => c.Id == chatId);

            if (chat == null || !chat.Members.Contains(currentUser))
                return BadRequest();

            var messageData = new Dictionary<string, object>
            {
                ["UserId"] = userId,
                ["ChatId"] = chat.Id,
                ["UserFullName"] = currentUser.FirstName + " " + currentUser.LastName,
                ["Message"] = message
            };

            // ТУТ РАССЫЛКА ПО СОБЫТИЮ
            foreach (var user in chat.Members)
                await _hubContext.Clients.User(user.Id).SendAsync("Notify", messageData);

            _context.Messages.Add(new Message
            {
                Time = DateTime.Now,
                User = currentUser,
                Chat = chat,
                Text = message,
                Read = false
            });

            try
            {
                await _context.SaveChangesAsync();
            } 
            catch (Exception)
            {
                return BadRequest();
            }
            
            return Ok();
        }

        // Установить все сообщения собеседника в чате как прочитанные.
        // Использовать при входе в чат
        [HttpPut]
        public async Task<IActionResult> SetReadChatMessages(int chatId)
        {
            var currentUserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == currentUserId);

            if (currentUser == null)
                return BadRequest();

            var unreadMessages = _context.Messages.
                Where(m => m.User.Id != currentUserId && m.Chat.Id == chatId);
            foreach (var message in unreadMessages)
                message.Read = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return BadRequest();
            }
            return Ok();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}