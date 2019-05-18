using Facebook.Models;
using FacebookDAW.Models;
using FacebookIA;
using FacebookIA.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;


namespace Facebook.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();
        public ActionResult Index()
        {
            Program program = new Program();
            //program.Main();
            ViewBag.loggedUser = false;
            if(User.FindFirst(ClaimTypes.NameIdentifier).Value!= null)
            {
                ViewBag.loggedUser = true;
                string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                Profile currentProfile = db.Profiles.SingleOrDefault(p => p.UserId == userId);
                if (currentProfile != null)
                {
                    Notification notifications = db.Notifications.SingleOrDefault(n => n.ReceiverId == currentProfile.Id);
                    ViewBag.notifications = notifications;
                    if (notifications != null)
                    {
                        ViewBag.notificationsCount = notifications.FriendRequests.Count();
                    }
                }
                //prin albumele profilului vreau sa iau pozele care sunt pending deja
                List<Album> albums = new List<Album>();
                foreach(var album in db.Albums)
                {
                    if(album.UserId == userId)
                    {
                        albums.Add(album);
                    }
                }
                List<Photo> photos = new List<Photo>();
                foreach(var photo in db.Photos)
                {
                    if (albums.Contains(photo.Album))
                    {
                        photos.Add(photo);
                    }
                }
                List<Comment> pendingComments = new List<Comment>();
                foreach(var comment in db.Comments)
                {
                    if (photos.Contains(comment.Photo) && comment.AcceptedStatus == 0)
                    {
                        pendingComments.Add(comment);
                    }
                }
                ViewBag.pendingComments = pendingComments;
                ViewBag.commentsCount = pendingComments.Count();

            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}