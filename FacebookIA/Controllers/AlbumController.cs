using Facebook.Models;
using FacebookDAW.Models;
using FacebookIA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Facebook.Controllers
{
    public class AlbumController : Controller
    {
        private ApplicationDbContext _db = ApplicationDbContext.Create();
        // GET: Album
        public ActionResult Index(int id)
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier).Value == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = _db.Profiles.SingleOrDefault(p => p.Id == id).UserId;
            var albums = _db.Albums.Where(a => a.UserId == userId);
            ViewBag.albums = albums;
            Profile profile = _db.Profiles.Find(id);
            ViewBag.FirstName = profile.FirstName;
            ViewBag.LastName = profile.LastName;
            string currentUser = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.Delete = false;
            if(User.IsInRole("Administrator") || currentUser == userId)
            {
                ViewBag.Delete = true;
            }
            ViewBag.Pictures = _db.Photos.ToList();
            return View();
        }
        public ActionResult New(string userId)
        {
            if(User.FindFirst(ClaimTypes.NameIdentifier).Value == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Album album = new Album();
            album.UserId = userId;
            return View(album);
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(Album album)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    album.UserId = userId;
                    _db.Albums.Add(album);
                    string userProfile = album.UserId;
                    int profileId = _db.Profiles.SingleOrDefault(p => p.UserId == userProfile).Id;
                    _db.SaveChanges();
                    TempData["newAlbum"] = "The album has been added!";
                    return RedirectToAction("Index", new { id = profileId });
                }
                else
                {
                    return View(album);
                }
            }
            catch (Exception e)
            {
                return View(album);
            }
        }

        public ActionResult Show (int id)
        {
            Album album = _db.Albums.Find(id);
            var photos = _db.Photos.Where(p => p.AlbumId == id);
            ViewBag.photos = photos;
            string userId = album.UserId;
            ViewBag.allowLike = false;
            if(userId != User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                ViewBag.allowLike = true;
            }
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Profile userProfile = _db.Profiles.SingleOrDefault(u => u.UserId == currentUserId);
            ViewBag.userProfile = userProfile;
            ViewBag.currentProfile = _db.Profiles.SingleOrDefault(p => p.UserId == userId);
            ViewBag.allowDelete = false;
            if(User.IsInRole("Administrator") || userId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                ViewBag.allowDelete = true;
            }
            return View(album);
        }
        //[HttpPost]
        public ActionResult AddPicture(int albumId, IFormFile file)
        {
            if (file != null && file.Length > 0)
                try
                {
                    /*string path = Path.Combine(Server.MapPath("~/Images"),
                                               Path.GetFileName(file.FileName));*/
                    var path = new FileStream("D:\\Ioana\\Dezvoltarea aplicatiilor web\\FacebookIA\\FacebookIA", FileMode.Create);
                    Photo photo = new Photo();
                    photo.Description = "~/Images/" + file.FileName;
                    
                    photo.AlbumId = albumId;
                    _db.Photos.Add(photo);
                    _db.SaveChanges();
                    file.CopyTo(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return RedirectToAction("Show", new { id = albumId });
        }

        public ActionResult Like(int id)
        {
            Photo currentPhoto = _db.Photos.Find(id);
            Album currentAlbum = _db.Albums.SingleOrDefault(a => a.Id == currentPhoto.AlbumId);
            Profile currentProfile = _db.Profiles.SingleOrDefault(p => p.UserId == currentAlbum.UserId);
            string currentUser = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Profile userProfile = _db.Profiles.SingleOrDefault(p => p.UserId == currentUser);
            //Tuple<Photo, Profile> tuplu = new Tuple<Photo, Profile>(currentPhoto, userProfile);
            if(currentPhoto.PeopleThatLiked == null)
            {
                currentPhoto.PeopleThatLiked = new List<Profile>();
            }
            if (!currentPhoto.PeopleThatLiked.Contains(userProfile))
            {
                currentPhoto.PeopleThatLiked.Add(userProfile);
            }
            if(userProfile.LikedPhotos == null)
            {
                userProfile.LikedPhotos = new List<Photo>();
            }
            if (!userProfile.LikedPhotos.Contains(currentPhoto))
            {
                userProfile.LikedPhotos.Add(currentPhoto);
            }
            currentPhoto.Likes = currentPhoto.PeopleThatLiked.ToList().Count();
            _db.SaveChanges();
            return RedirectToAction("Show", new { id = currentAlbum.Id });
        }
        public ActionResult DeletePhoto(int id)
        {
            Photo currentPhoto = _db.Photos.Find(id);
            Album currentAlbum = _db.Albums.SingleOrDefault(a => a.Id == currentPhoto.AlbumId);
            _db.Photos.Remove(currentPhoto);
            _db.SaveChanges();
            return RedirectToAction("Show", new { id = currentAlbum.Id });
        }
        public ActionResult DeleteAlbum(int id)
        {
            Album deletedAlbum = _db.Albums.Find(id);
            int profileId = _db.Profiles.SingleOrDefault(p => p.UserId == deletedAlbum.UserId).Id;
            int albumId = deletedAlbum.Id;
            foreach(Photo p in _db.Photos)
            {
                if(p.AlbumId == albumId)
                {
                    _db.Photos.Remove(p);
                }
            }
            _db.Albums.Remove(deletedAlbum);
            _db.SaveChanges();
            return RedirectToAction("Index", new { id = profileId});
        }
    }
}