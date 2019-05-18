using Facebook.Models;
using FacebookDAW.Models;
using FacebookIA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Web;



namespace Facebook.Controllers
{
    public class ChatController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();
        // GET: Chat
        public ActionResult Index()
        {
            string userId = UserManager;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (User.IsInRole("Administrator"))
            {
                Profile admin = db.Profiles.SingleOrDefault(p => p.UserId == userId);
                foreach(Profile p in db.Profiles)
                {
                    
                    if(p.Friends == null)
                    {
                        p.Friends = new List<Profile>();
                    }
                    if (!p.Friends.Contains(admin))
                    {
                        p.Friends.Add(admin);
                    }
                    if(admin.Friends == null)
                    {
                        admin.Friends = new List<Profile>();
                    }
                    if (!admin.Friends.Contains(p))
                    {
                        admin.Friends.Add(p);
                        Chat chat = new Chat();
                        chat.Profiles = new List<Profile>();
                        chat.Profiles.Add(admin);
                        chat.Profiles.Add(p);
                        db.Chats.Add(chat);
                    }
                }
                
                db.SaveChanges();
            }
            //caut profilul utilizatorului curent si preiau lista sa de prieteni
            //pentru fiecare prieten, adaug in baza de date un chat cu el 
            var userProfile = db.Profiles.SingleOrDefault(p => p.UserId == userId);
            ViewBag.myFriends = userProfile.Friends;
            var userProfileGroups = db.Profiles.SingleOrDefault(p => p.UserId == userId).Groups;
            ViewBag.myGroups = userProfileGroups;
            return View();
        }
        
        public ActionResult StartNormalChat(int id) //primeste ca param id-ul profilului prietenului 
        {
            
            //ViewBag.friendId = id;
            string myId = User.Identity.GetUserId();
            //ViewBag.myId = myId;
          
            Profile friendProfile = db.Profiles.Find(id);
            Profile myProfile = db.Profiles.SingleOrDefault(p => p.UserId == myId);
            int myProfileId = myProfile.Id;
            ViewBag.myId = myProfileId;
            int chatId = new int();
            var oldMessages = new List<Message>();

            foreach(var chat in db.Chats)
            {
                //caut in baza de date chatul format doar din profilurile: myProfile si friendProfile
                if(chat.Profiles.Contains(myProfile) && chat.Profiles.Contains(friendProfile) && chat.Profiles.Count() == 2)
                {
                    chatId = chat.Id;
                    // preiau mesajele vechi din conversatia cu profilul friendProfile
                    oldMessages = chat.Messages.OrderBy(x => x.SendDate).ToList(); //conversie din ICollection in List<Message>
                    break;
                }               
            }
            TempData["groupAllowDelete"] = false;
            TempData["allowDelete"] = false;
            if (User.IsInRole("Administrator"))
            {
                TempData["allowDelete"] = true;
            }
          
            TempData["isAdmin"] = false;
            if (User.IsInRole("Admin"))
            {
                TempData["isAdmin"] = true;
            }
            TempData["listOldMessages"] = oldMessages;
            ViewBag.firstnameFriend = friendProfile.FirstName;
            TempData["firstnameFriend"] = friendProfile.FirstName;
            TempData["lastnameFriend"] = friendProfile.LastName;
            //ViewBag.firstnameFriend = friendProfile.FirstName;
           // ViewBag.lastnameFriend = friendProfile.LastName;
            return RedirectToAction("NewMessage", new { chatId = chatId, senderId = myProfileId});
            //return RedirectToAction("MessageBox", "Chat");
        }
        public ActionResult StartGroupChat(int id) //primeste ca parametru id-ul grupului
        {
            string myId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            Group group = db.Groups.Find(id);
            
            Profile myProfile = db.Profiles.SingleOrDefault(p => p.UserId == myId);
            int myProfileId = myProfile.Id;
            ViewBag.myId = myProfileId;
            //iau chat-ul corespunzator grupului
            Chat groupChat = db.Chats.SingleOrDefault(c => c.GroupId == group.Id);

            int chatId = new int();
            var oldMessages = new List<Message>();
            chatId = groupChat.Id;
            oldMessages = groupChat.Messages.OrderBy(x => x.SendDate).ToList();
            TempData["allowDelete"] = false;
            if(groupChat.Profiles.Count > 2)
            {
                TempData["groupAllowDelete"] = true;
            }
            else
            {
                TempData["groupAllowDelete"] = false;
            }
           
            if (User.IsInRole("Administrator"))
            {
                TempData["allowDelete"] = true;
            }
            TempData["listOldMessages"] = oldMessages;
             return RedirectToAction("NewMessage", new { chatId = chatId, senderId = myProfileId });
        }
        public ActionResult NewMessage(int chatId, int senderId)
        {
            //preiau lista de mesaje vechi adaugata mai sus in TempData si o trimit in view prin Viewbag
            var oldMessages = TempData["listOldMessages"] as List<Message>;
            ViewBag.oldMessages = oldMessages;
            ViewBag.chatId = chatId;
            Chat c = db.Chats.Find(chatId);
            ViewBag.groupName = null;
            if(c != null && c.GroupId != null)
            {
                Group g = db.Groups.Find(c.GroupId);
                ViewBag.groupName = g.Name;
            }
            ViewBag.SenderId = senderId;   
            Message message = new Message();
            ViewBag.groupAllowDelete = TempData["groupAllowDelete"];
            ViewBag.allowDelete = TempData["allowDelete"];
            ViewBag.isAdmin = TempData["isAdmin"];
            ViewBag.myId = senderId;
            return View("MessageBox", message); 
        }
        public ActionResult DeleteMessage(int id)
        {
            Message deletedMessage = db.Messages.Find(id);
            string userId = User.Identity.GetUserId();
            Profile currentProfile = db.Profiles.SingleOrDefault(p => p.UserId == userId);
            int chatId = deletedMessage.ChatId;
            Chat currentChat = db.Chats.Find(chatId);
            currentChat.Messages.Remove(deletedMessage);
            db.Messages.Remove(deletedMessage);
            db.SaveChanges();
            if(currentChat.GroupId == null)
            {
                Profile friend = currentChat.Profiles.SingleOrDefault(p => p.Id != currentProfile.Id);
                return RedirectToAction("StartNormalChat", new { id = friend.Id });
            }
            else
            {
                return RedirectToAction("StartGroupChat", new { id = chatId });
            }
            

        }
        public ActionResult EditMessage(int id)
        {
            Message message = db.Messages.Find(id);
            ViewBag.Message = message;
            return View(message);
        }
        [HttpPut]
        public ActionResult EditMessage(int id, Message requestMessage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Message message = db.Messages.Find(id);
                    if (TryUpdateModel(message))
                    {
                        message.Content = requestMessage.Content;   
                        db.SaveChanges();

                    }
                    return RedirectToAction("EditMessage", "Chat", new { id = message.Id });
                }
                else
                {
                    return RedirectToAction("EditMessage", "Chat", new { id = id });
                }

            }
            catch (Exception e)
            {
                return RedirectToAction("EditMessage", "Chat", new { id = id });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult NewMessage(Message message)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    /*
                    ViewBag.Campchat = message.ChatId;
                    ViewBag.Campsender = message.SenderId;
                    ViewBag.Campcontent = message.Content;
                    */
                    // !! SendDate are valoarea 01.01.2018 00:00:00 = null in baza de date; modificare explicita 
                    //message.Sender = sender;
                    Profile sender = db.Profiles.Find(message.SenderId);
                    message.Sender = sender;
                    message.SendDate = DateTime.Now;
                    //  ViewBag.Campdate = message.SendDate;
                    db.Messages.Add(message);
                    Chat chat = db.Chats.Find(message.ChatId);
                    chat.Messages.Add(message);
                    ViewBag.oldMessages = chat.Messages.OrderBy(x => x.SendDate).ToList();
                    db.SaveChanges();
                    ViewBag.myId = message.SenderId;
                    TempData["message"] = "Message was sent!";
                    //return tot catre view-ul MessageBox pt a continua conversatia 
                    ViewBag.allowDelete = false;
                    if (User.IsInRole("Administrator"))
                    {
                        ViewBag.allowDelete = true;
                    }
                    if(chat.Profiles.Count() > 2)
                    {
                        ViewBag.groupAllowDelete = true;
                    }
                    else
                    {
                        ViewBag.groupAllowDelete = false;
                    }
                    return View("MessageBox", message);
                }
                else
                {
                    return View("MessageBox", message);
                }
            }
            catch (Exception e)
            {
                return View("MessageBox", message);
            }
        }
        
    }
}