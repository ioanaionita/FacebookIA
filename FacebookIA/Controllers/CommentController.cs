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
    public class CommentController : Controller
    {
        private ApplicationDbContext _db;
        // GET: Comment
        public ActionResult Index(int id)
        {
            //iau url-ul pozei pentru a o afisa
            Photo currentPhoto = _db.Photos.Find(id);
            ViewBag.currentPhoto = currentPhoto.Description;
            ViewBag.currentPhotoId = currentPhoto.Id;
            List<Comment> comments = new List<Comment>();
            foreach(var comment in _db.Comments)
            {
                if(comment.PhotoId == id)
                {
                    comments.Add(comment);
                }
            }
            ViewBag.comments = comments;
            Album currentAlbum = _db.Albums.Find(currentPhoto.AlbumId);
            ViewBag.albumId = currentAlbum.Id;
            ViewBag.albumName = currentAlbum.Name;
            ViewBag.allowDelete = false;
            if (User.IsInRole("Administrator"))
            {
                ViewBag.allowDelete = true;
            }
            return View();
        }
        [HttpPost]
        public ActionResult New(Comment comment, int id)
        {
            comment.PhotoId = id;
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Profile profile = _db.Profiles.SingleOrDefault(p => p.UserId == currentUserId);
            comment.ProfileId = profile.Id;
            comment.FirstNameUser = profile.FirstName;
            comment.LastNameUser = profile.LastName;
            comment.DateCreated = DateTime.Now;
            comment.UserId = currentUserId;
            Photo currentPhoto = _db.Photos.Find(id);
            Album currentAlbum = _db.Albums.Find(currentPhoto.AlbumId);
            //Iau profilul curent si vreau sa il compar cu cel care a adaugat acest comentariu.
            Profile ownerProfile = _db.Profiles.SingleOrDefault(p => p.UserId == currentAlbum.UserId);
            //Daca cel care are poza adauga un comentariu, statusul comentariului va deveni direct 1
            //status = 1 -> comentariu acceptat, ce va fi afisat
            //status = 0 -> comentariu pending, asteapta sa fie acceptat sau refuzat de cel ce are poza
            //status = -1 -> comentariu refuzat, nu va fi niciodata afisat

            if(profile == ownerProfile)
            {
                comment.AcceptedStatus = 1;    
            }
            else
            {
                comment.AcceptedStatus = 0;
                TempData["pending"] = "Comentariul va fi acceptat/refuzat de catre proprietarul pozei.";
            }
            _db.Comments.Add(comment);
            _db.SaveChanges();

            return RedirectToAction("Index", new { id = comment.PhotoId });
        }

        //[HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment comment = _db.Comments.Find(id);
            int photoId = comment.PhotoId;
            _db.Comments.Remove(comment);
            _db.SaveChanges();
            TempData["message"] = "Comentariul a fost sters!";
            return RedirectToAction("Index", new { id = photoId });
        }
        public ActionResult AcceptComment(int id)
        {
            Comment comment = _db.Comments.Find(id);
            comment.AcceptedStatus = 1;
            _db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult RejectComment(int id)
        {
            Comment comment = _db.Comments.Find(id);
            comment.AcceptedStatus = -1;
            _db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
    }
}