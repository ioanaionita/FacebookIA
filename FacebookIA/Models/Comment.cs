using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FacebookDAW.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }

        public int PhotoId { get; set; }
        public virtual Photo Photo { get; set; }

        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }

        public string FirstNameUser { get; set; }
        public string LastNameUser { get; set; }
        //acest camp imi va spune daca un comentariu a fost acceptat sau nu
        //doar daca e acceptat va fi afisat
        public int AcceptedStatus { get; set; }

        public DateTime DateCreated { get; set; }
        
    }
}