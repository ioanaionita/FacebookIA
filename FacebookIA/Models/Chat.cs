using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FacebookDAW.Models
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}