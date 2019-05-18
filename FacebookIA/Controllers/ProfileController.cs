using Facebook.Models;
using FacebookDAW.Models;
using FacebookIA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Facebook.Controllers
{
    public class ProfileController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();
        // GET: Profile
        public ActionResult Index(string name = "")
        {
            // preluam lista de categorii din metoda GetAllCategories()
            //profile.Groups = GetAllGroups(profile);
            //profile.Friends = GetAllFriends(profile);
            //profile.UserId = User.Identity.GetUserId();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            //transmitem catre Index view toate profilurile, in afara de cel al utilizatorului curent (People you may know);
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(name == "")
            {
                var profiles = db.Profiles.Where(p => p.UserId != currentUserId);
                ViewBag.Profiles = profiles;
            }
            else
            {
                name = name.ToLower();
                List<Profile> foundProfiles = new List<Profile>();
                foreach(Profile p in db.Profiles)
                {
                    string firstName = p.FirstName.ToLower();
                    string lastName = p.LastName.ToLower();
                    if(firstName.Contains(name) || lastName.Contains(name))
                    {
                        foundProfiles.Add(p);
                    }
                }
                ViewBag.Profiles = foundProfiles;
            }
            
            return View();
        }
        public ActionResult Show(int id)
        {
            Profile profile = db.Profiles.Find(id);
            
            ViewBag.Private = profile.ProfileVisibility;
            if(profile.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value)
            {
                ViewBag.Private = false;
            }
            ViewBag.Profile = profile;
            ViewBag.DateOfBirth = profile.DateOfBirth.Date.ToString("dd.MM.yyyy");
            ViewBag.allowEdit = false;
            ViewBag.currentUser = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.currentProfile = profile.Id;
            ViewBag.guestUser = false;
            var albums = db.Albums.Where(a => a.UserId == profile.UserId);
            ViewBag.albums = albums;
            if (User.FindFirst(ClaimTypes.NameIdentifier).Value == null)
            {
                ViewBag.guestUser = true;
                return View(profile);

            }
            if (profile.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value && (User.IsInRole("Administrator") || User.IsInRole("Editor")))
            {
                ViewBag.allowEdit = true;
            }
            if (User.IsInRole("Administrator"))
            {
                ViewBag.allowEdit = true;
            }
            if (TempData.ContainsKey("update"))
            {
                ViewBag.update = TempData["update"].ToString();
            }
            //iau userul curent
            string idUser = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Profile profilUser = db.Profiles.SingleOrDefault(p => p.UserId == idUser);

            //verific daca userul curent e prieten cu profilul pe care a intrat
            //Daca sunt prieteni, voi afisa "Friends" in loc de butonul de cerere de prietenie        
            if (profilUser.Friends.Contains(profile) && profile.Friends.Contains(profilUser))
            {
                ViewBag.alreadyFriends = true;
            }
            else
            {
                ViewBag.alreadyFriends = false;
            }

            //Daca userul curent a mai trimis o cerere de prietenie catre profilul pe care se afla,
            //atunci va aparea "Friend request sent" si nu va mai putea trimite o alta cerere.
            if (profilUser.SentFriendRequests.Contains(profile))
            {
                ViewBag.friendRequestSent = true;
            }
            else
            {
                ViewBag.friendRequestSent = false;
            }
            //Daca userul curent a primit el cerere de la profil (profilul i-a trimis lui o cerere),
            //atunci userului curent ii va aparea sa accepte cererea primita, "Accept friend request"
            if (profile.SentFriendRequests.Contains(profilUser))
            {
                ViewBag.acceptFriendRequest = true;
            }
            else
            {
                ViewBag.acceptFriendRequest = false;
            }
      
            return View(profile);
        }


        public ActionResult New()
        {
            Profile profile = new Profile();
            profile.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return View(profile);
        }

        [NonAction] //pentru aceasta metoda si dropdown (Vezi curs Pag 16-18)
        public List<Group> GetAllGroups(Profile profile)
        {
            // generam o lista goala
            var selectList = new List<Group>();
            // Extragem toate grupurile din baza de date
            var groups = from g in db.Groups select g;
            // iteram prin grupuri

            foreach (var g in groups)
            {
                //Caut grupurile care contin profilul cautat in lista lor de profile
                if (g.Profiles.Contains(profile))
                {
                    selectList.Add(g);
                }
            }
            // returnam lista de grupuri
            return selectList;
        }
        [NonAction] //pentru aceasta metoda si dropdown (Vezi curs Pag 16-18)
        public List<Profile> GetAllFriends(Profile profile)
        {
            // generam o lista goala
            var selectList = new List<Profile>();
            // Extragem toate profilurile din baza de date
            var friends = from f in db.Profiles select f;
            // iteram prin profiluri
            foreach (var f in friends)
            {
                //Caut profilurile prietene cu profilul meu
                if (f.Friends.Contains(profile))
                {
                    selectList.Add(f);
                }
            }
            // returnam lista de prieteni
            return selectList;
        }

        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "User, Editor, Administrator")]
        public ActionResult MyProfile(string userId)
        {
            int ok = 0; //spune daca am gasit profilul ce apartine Userului cu id-ul userId
            Profile myProfile = new Profile();
            foreach (var profile in db.Profiles)
            {
                if (profile.UserId == userId)
                {
                    ok = 1;
                    myProfile = profile;
                }
            }
            if (ok == 1)
                return RedirectToAction("Show", new { id = myProfile.Id });
            else
             return RedirectToAction("New");
        }

        //verificare ca un user sa nu-si creeze mai multe profiluri !!!
        [HttpPost]
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(Profile profile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Profiles.Add(profile);
                    db.SaveChanges();
                    TempData["message"] = "The profile has been added!";
                    return RedirectToAction("Index", "Profile");
                }
                else
                {
                    return View(profile);
                }
            }
            catch (Exception e)
            {
                return View(profile);
            }
        }
        [Authorize(Roles = "User, Editor,Administrator")]
        public ActionResult Edit(int id)
        {
            Profile profile = db.Profiles.Find(id);
            if(profile.UserId != User.FindFirst(ClaimTypes.NameIdentifier).Value && !User.IsInRole("Administrator"))
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Profile = profile;
            return View(profile);
        }

        [HttpPut]
        public ActionResult Edit(int id, Profile requestProfile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Profile profile = db.Profiles.Find(id);
                    //if (TryUpdateModelAsync<Profile> (profile))
                    //{
                        profile.FirstName = requestProfile.FirstName;
                        profile.LastName = requestProfile.LastName;
                        profile.DateOfBirth = requestProfile.DateOfBirth;
                        profile.City = requestProfile.City;
                        profile.Country = requestProfile.Country;
                        profile.ProfileVisibility = requestProfile.ProfileVisibility;
                        db.SaveChanges();
                        TempData["update"] = "The profile has been updated!";
                        
                    //}
                    return RedirectToAction("Show", new { id = profile.Id });
                }
                else
                {
                    return View();
                }

            }
            catch(Exception e)
            {
                return View();
            }
        }
        //[HttpPost]
        public ActionResult AddFriend(int id)
        {
            
            Profile profil = db.Profiles.Find(id);
            string idUser = User.Identity.GetUserId();
            Profile profilSender = db.Profiles.SingleOrDefault(p => p.UserId == idUser);
                if(profilSender != null)
                    {
                //caut in tabelul de notificari notificarile care corespund profilului curent
                Notification notification = db.Notifications.SingleOrDefault(n => n.ReceiverId == profil.Id);
                //daca profilul curent nu are inca o lista de notificari asociata
                if (notification == null)
                    {
                    notification = new Notification();
                    notification.ReceiverId = profil.Id;
                    notification.FriendRequests = new List<Profile>();
                    db.Notifications.Add(notification);
                    db.SaveChanges();

                    }
                //profilul curent a primit o cerere de la profilul care i-a trimis
                //notificarea apartine profilului pe care sunt, el primeste cererea de prietenie
                notification.FriendRequests.Add(profilSender);
                
                //ii notez si utilizatorului care a trimis cererea
                profilSender.SentFriendRequests.Add(profil);
                if (profil.Friends.Contains(profilSender))
                {
                    profil.Friends.Remove(profilSender);
                }
                if (profilSender.Friends.Contains(profil))
                {
                    profilSender.Friends.Remove(profil);
                }
                db.SaveChanges();
                }

            return RedirectToAction("Show", new { id = profil.Id });
        }
        public ActionResult AcceptFriendRequest(int id)
        {
            Profile profil = db.Profiles.Find(id);
            string idUser = User.Identity.GetUserId();
            Profile profilSender = db.Profiles.SingleOrDefault(p => p.UserId == idUser);
            //caut notificarea asociata userului, deoarece userul a primit cererea de prietenie 
            //de la profilul pe care se afla
            Notification notification = db.Notifications.SingleOrDefault(n => n.ReceiverId == profilSender.Id);
            notification.FriendRequests.Remove(profil);
            profil.SentFriendRequests.Remove(profilSender);
            db.SaveChanges();
            if(profil.Friends == null)
            {
                profil.Friends = new List<Profile>();
            }
            if (!profil.Friends.Contains(profilSender))
            {
                profil.Friends.Add(profilSender);
            }
            if (profilSender.Friends == null)
            {
                profilSender.Friends = new List<Profile>();
            }
            if (!profilSender.Friends.Contains(profil))
            {
                profilSender.Friends.Add(profil);
            }
            db.SaveChanges();
            //odata cu un prieten nou, e instantiat si un nou chat gol cu acesta 
            Chat chat = new Chat();
            chat.Messages = new List<Message>();
            chat.Profiles = new List<Profile>(new Profile[] { profilSender, profil });
            chat.Name = profil.FirstName + profilSender.FirstName;
            db.Chats.Add(chat);
            db.SaveChanges();
            
            
            return RedirectToAction("Show", new { id = profil.Id });
        }
        public ActionResult FriendsAndGroups(int id)
        {
            //int id = int.Parse(profileId);
            Profile profile = db.Profiles.Find(id);
            ViewBag.profile = profile;
            ViewBag.numberOfFriends = profile.Friends.Count();
            ViewBag.friends = profile.Friends;
            ViewBag.numberOfGroups = profile.Groups.Count();
            ViewBag.groups = profile.Groups;
            return View();
        }

        public ActionResult Search()
        {
            string nameSearched = Request.Form["search"];
            nameSearched = nameSearched.Trim();
            return RedirectToAction("Index", new { name = nameSearched });
        }
    }
}