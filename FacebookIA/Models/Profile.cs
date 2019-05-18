using Facebook.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FacebookDAW.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "FirstName is required")]
        [RegularExpression(@"^[A-Za-z]*$", ErrorMessage = "Firstname is invalid")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "LastName is required")]
        [RegularExpression(@"^[A-Za-z]*$", ErrorMessage = "Lastname is invalid")]
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "City is required")]
        [RegularExpression(@"^[A-Za-z]*$", ErrorMessage = "City name is invalid")]
        public string City { get; set; }
        [Required(ErrorMessage = "Country is required")]
        [RegularExpression(@"^[A-Za-z]*$", ErrorMessage = "Country name is invalid")]
        public string Country { get; set; }
        public Boolean ProfileVisibility { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<Profile> Friends { get; set; }
        //se retine profilul persoanei careia i-am trimis cerere de prietenie
        public virtual ICollection<Profile> SentFriendRequests { get; set; }
        public virtual ICollection<Chat> Chats { get; set; }
        public virtual ICollection<Photo> LikedPhotos { get; set; }
    }
}