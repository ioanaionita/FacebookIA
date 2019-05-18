using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FacebookIA.Models;
using FacebookDAW.Models;
using Facebook.Models;

namespace FacebookIA.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
        {

        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
        {
        }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }
       
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Profile>().HasMany(c => c.Groups);
            builder.Entity<Profile>().HasMany(c => c.Friends);
            builder.Entity<Profile>().HasMany(c => c.SentFriendRequests);
            builder.Entity<Profile>().HasMany(c => c.Chats);
            builder.Entity<Profile>().HasMany(c => c.LikedPhotos);
            builder.Entity<Photo>().HasMany(c => c.Comments).WithOne(c => c.Photo);
            builder.Entity<Photo>().HasMany(c => c.PeopleThatLiked);
            builder.Entity<Notification>().HasMany(c => c.FriendRequests);
            //builder.Entity<Notification>().HasMany(c => c.Likes);
            builder.Entity<Group>().HasMany(c => c.Profiles);
            builder.Entity<Group>().HasMany(c => c.Admins);
            builder.Entity<Chat>().HasMany(c => c.Profiles);
            builder.Entity<Chat>().HasMany(c => c.Messages);
            builder.Entity<Album>().HasMany(c => c.Photos);

            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
