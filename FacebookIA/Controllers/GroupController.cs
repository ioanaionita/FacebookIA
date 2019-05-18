using Facebook.Models;
using FacebookDAW.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Facebook.Controllers
{
    public class GroupController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();
        // GET: Group
        public ActionResult Index()

        {
            if(User.Identity.GetUserId() == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.noGroups = false;
            if (db.Groups == null)
            {
                ViewBag.noGroups = true;
            }
            else
            {
                List<Group> groups = new List<Group>();
                string userId = User.Identity.GetUserId();
                Profile profilCurrent = db.Profiles.SingleOrDefault(p => p.UserId == userId);
                foreach (Group g in db.Groups)
                {
                    if (!profilCurrent.Groups.Contains(g))
                    {
                        groups.Add(g);
                    }
                }
                ViewBag.Groups = groups;
                if (TempData.ContainsKey("grup"))
                {
                    ViewBag.grup = TempData["grup"].ToString();
                }
            }
            return View();
        }

        public ActionResult New()
        {
            string currentUser = User.Identity.GetUserId();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Group group = new Group();
            //var myFriends = creatorProfile.Friends.Select(c => new {

             //               Id = c.Id,
            //                FirstName = c.FirstName}).ToList();
            //ViewBag.Friends = new MultiSelectList(myFriends, "Id", "FirstName");
            return View(group);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(Group group)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string userId = User.Identity.GetUserId();
                    Profile creatorProfile = db.Profiles.SingleOrDefault(p => p.UserId == userId);
                    group.Admins = new List<Profile>();
                    group.Profiles = new List<Profile>();
                    group.Admins.Add(creatorProfile);
                    group.Profiles.Add(creatorProfile);
                    group.CreatedDate = DateTime.Now;
                    creatorProfile.Groups.Add(group);
                    db.Groups.Add(group);
                    Chat chat = new Chat();
                    chat.GroupId = group.Id;
                    chat.Profiles = new List<Profile>();
                    chat.Profiles.Add(creatorProfile);
                    db.Chats.Add(chat);
                    db.SaveChanges();
                    TempData["grup"] = "The group has been added!";
                    return RedirectToAction("Index", "Group");
                }
                else
                {
                    return View(group);
                }
            }
            catch (Exception e)
            {
                return View(group);
            }
        }
        public ActionResult JoinGroup(int id)
        {
            string currentUser = User.Identity.GetUserId();
            Profile profilCurent = db.Profiles.SingleOrDefault(p => p.UserId == currentUser);
            Group currentGroup = db.Groups.Find(id);
            currentGroup.Profiles.Add(profilCurent);
            profilCurent.Groups.Add(currentGroup);
            Chat groupChat = db.Chats.SingleOrDefault(c => c.GroupId == currentGroup.Id);
            groupChat.Profiles.Add(profilCurent);
            db.SaveChanges();
            return RedirectToAction("Index", "Group");
        }
    }
}