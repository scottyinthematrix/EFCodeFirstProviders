using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    // TODO
    // move it to other projects if you'd like a 100 percent persistency unaware entity layer
    public class MembershipContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role and Function : many-to-many
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Functions)
                .WithMany(f => f.Roles)
                .Map(m =>
                         {
                             m.ToTable("FunctionsInRoles");
                             m.MapLeftKey("RoleId");
                             m.MapRightKey("FunctionId");
                         });
            // Role and User : many-to-many
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithMany(u => u.Roles)
                .Map(m =>
                         {
                             m.ToTable("UsersInRoles");
                             m.MapLeftKey("RoleId");
                             m.MapRightKey("UserId");
                         });


            // User and Profile : one-to-many
            modelBuilder.Entity<Profile>()
                .HasRequired(p => p.User)
                .WithMany(u => u.Profiles)
                .HasForeignKey(p => p.UserId);

            // compose key on Profile
            modelBuilder.Entity<Profile>()
                .HasKey(p => new { p.UserId, p.PropertyName });
            // Role.ApplicationId and Role.PId
            modelBuilder.Entity<Role>()
                        .HasRequired(r => r.Application)
                        .WithMany(a => a.Roles)
                        .Map(m => m.MapKey("ApplicationId"))
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Role>()
                        .HasRequired(r => r.Parent)
                        .WithMany(r => r.Children)
                        .Map(m => m.MapKey("PId"));
            // User.ApplicationId
            modelBuilder.Entity<User>()
                        .HasRequired(u => u.Application)
                        .WithMany(a => a.Users)
                        .Map(m => m.MapKey("ApplicationId"))
                        .WillCascadeOnDelete(false);
            // Function.ApplicationId and Function.PId
            modelBuilder.Entity<Function>()
                        .HasRequired(f => f.Application)
                        .WithMany(a => a.Functions)
                        .Map(m => m.MapKey("ApplicationId"));
            modelBuilder.Entity<Function>()
                        .HasRequired(f => f.Parent)
                        .WithMany(f => f.Children)
                        .Map(m => m.MapKey("PId"));
            modelBuilder.Entity<Profile>()
                        .HasRequired(p => p.User)
                        .WithMany(u => u.Profiles)
                        .HasForeignKey(p => p.UserId);
        }

        public DbSet<Application> Applications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Function> Functions { get; set; }
    }
}
