using FacebookDAW.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Facebook.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int ReceiverId { get; set; }
        public virtual Profile Receiver { get; set; }

        public virtual ICollection<Profile> FriendRequests { get; set; }

        //id poza si id user care a dat like
        public virtual ICollection<Tuple<Photo, Profile>> Likes { get; set; }
    }
}