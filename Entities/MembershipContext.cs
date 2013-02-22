using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using ScottyApps.Utilities.DbContextExtensions;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    // TODO
    // move it to other projects if you'd like a 100 percent persistency unaware entity layer
    public class MembershipContext : DbContext
    {
        public MembershipContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer(new MembershipInitializer());
        }
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
                // NOTE this is necessary as otherwise it will cause a circular constraint
                        .WillCascadeOnDelete(false);
            // [Role] Parent-Child
            modelBuilder.Entity<Role>()
                        .HasOptional(r => r.Parent)
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
            // [Function] Parent-Child
            modelBuilder.Entity<Function>()
                        .HasOptional(f => f.Parent)
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

    internal class MembershipInitializer : DropCreateDatabaseIfModelChanges<MembershipContext>
    {
        protected override void Seed(MembershipContext context)
        {
            #region Application

            var app = new Application
                          {
                              Id = new Guid("57835CED-B7F4-4F9B-8DA9-985D2E56CF04"),
                              Name = "SalesMgt",
                              Description = "a sales management system"
                          };
            app.MarkAsAdded();

            #endregion

            #region User

            var userScotty = new User
                           {
                               Id = new Guid("9BAD70A5-5BE4-4578-8A4F-CF14589D2386"),
                               Name = "scotty",
                               Application = app,
                               Comment = "this is scotty and also the administrator",
                               IsAnonymous = false,
                               IsConfirmed = true,
                               IsLockedOut = false,
                               Password = "cppfans",
                               PasswordQuestion = "who am i",
                               PasswordAnswer = "scotty",
                               Email = "scotty.cn@gmail.com",
                               LastActiveDate = DateTime.Parse("2012-11-27 10:24:00.000")
                           };
            userScotty.MarkAsAdded();

            var userJuicy = new User
                           {
                               Id = new Guid("56FED227-CB6A-40E4-8095-1B7866A4592F"),
                               Name = "juicy",
                               Application = app,
                               Comment = "this is juicy but NOT the administrator",
                               IsAnonymous = false,
                               IsConfirmed = true,
                               IsLockedOut = false,
                               Password = "good160",
                               PasswordQuestion = "who am i",
                               PasswordAnswer = "juicy",
                               Email = "juicy.sunny@gmail.com",
                               LastActiveDate = DateTime.Parse("2012-11-27 10:24:00.000")
                           };
            userJuicy.MarkAsAdded();


            #endregion

            #region Role

            var rolePassenger = new Role
                                    {
                                        Id = new Guid("6A98AC1F-34C0-4D6D-BF0C-50BDE75856BE"),
                                        Name = "Passenger",
                                        Description = "people who viewing any pages on this site",
                                        Application = app
                                    };
            var roleMarketMgr = new Role
                                    {
                                        Id = new Guid("11717A81-96FA-4D40-B374-A978526E520F"),
                                        Name = "MarketManager",
                                        Description = "General Market Manager",
                                        Parent = rolePassenger,
                                        Application = app
                                    };
            var roleITMgr = new Role
                                {
                                    Id = new Guid("069E5A9F-327C-4ED3-9265-ABEC259B2F6E"),
                                    Name = "ITManager",
                                    Description = "General it manager",
                                    Parent = rolePassenger,
                                    Application = app
                                };
            var roleMarketMgrWH = new Role
                                      {
                                          Id = new Guid("E64648A3-84F3-4F62-BF32-E867416CF939"),
                                          Name = "WH-Market-Mgr",
                                          Description = "wuhan market manager",
                                          Parent = roleMarketMgr,
                                          Application = app
                                      };
            var roleMarketMgrSH = new Role
                                      {
                                          Id = new Guid("E18B690A-42DA-45F7-BB68-67808200DB5D"),
                                          Name = "SH-Market-Mgr",
                                          Description = "shanghai market manager",
                                          Parent = roleMarketMgr,
                                          Application = app
                                      };
            var roleITMgrWH = new Role
                                  {
                                      Id = new Guid("6109A731-F5EE-453C-840D-0EF803F11E8B"),
                                      Name = "WH-IT-Mgr",
                                      Description = "wuhan it manager",
                                      Parent = roleITMgr,
                                      Application = app
                                  };
            var roleITMgrSH = new Role
                                  {
                                      Id = new Guid("0A55F1F3-4CA9-492B-ABE8-EC4586610967"),
                                      Name = "SH-IT-Mgr",
                                      Description = "shanghai it manager",
                                      Parent = roleITMgr,
                                      Application = app
                                  };
            rolePassenger.MarkAsAdded();
            roleITMgr.MarkAsAdded();
            roleMarketMgr.MarkAsAdded();

            roleITMgr.Children.Add(roleITMgrSH);
            roleITMgr.Children.Add(roleITMgrWH);
            roleMarketMgr.Children.Add(roleMarketMgrSH);
            roleMarketMgr.Children.Add(roleMarketMgrWH);
            roleITMgrSH.MarkAsAdded();
            roleITMgrWH.MarkAsAdded();
            roleMarketMgrSH.MarkAsAdded();
            roleMarketMgrWH.MarkAsAdded();

            #endregion

            userScotty.Roles.Add(roleITMgrSH);
            userScotty.Roles.Add(roleMarketMgr);

            userJuicy.Roles.Add(roleMarketMgrWH);
            userJuicy.Roles.Add(roleITMgr);

            #region Function

            var fnMgmtSalesRpt = new Function
                                     {
                                         Id = new Guid("BC889A9A-C230-427D-A67E-5B298746D697"),
                                         Name = "ManageSalesReport",
                                         Application = app
                                     };
            var fnReadAnyPg = new Function
                                  {
                                      Id = new Guid("779DC035-FD36-466D-8043-ADC1BBACAF38"),
                                      Name = "Read Any Page",
                                      Application = app
                                  };
            var fnViewRpt = new Function
                                {
                                    Id = new Guid("A1DAF450-E675-4E92-8942-9F5B28E0B635"),
                                    Name = "View Report",
                                    Application = app,
                                    Parent = fnMgmtSalesRpt
                                };
            var fnPrnRpt = new Function
                               {
                                   Id = new Guid("19F07D28-26ED-4225-AC11-B2C1FEC60386"),
                                   Name = "Print Report",
                                   Application = app,
                                   Parent = fnMgmtSalesRpt
                               };
            fnMgmtSalesRpt.MarkAsAdded();
            fnReadAnyPg.MarkAsAdded();
            fnViewRpt.MarkAsAdded();
            fnPrnRpt.MarkAsAdded();

            fnMgmtSalesRpt.Children.Add(fnViewRpt);
            fnMgmtSalesRpt.Children.Add(fnPrnRpt);

            #endregion

            roleITMgrSH.Functions.Add(fnMgmtSalesRpt);
            rolePassenger.Functions.Add(fnReadAnyPg);

            app.Users.Add(userScotty);
            app.Users.Add(userJuicy);
            
            app.Roles.Add(roleITMgr);
            app.Roles.Add(roleMarketMgr);

            app.Functions.Add(fnMgmtSalesRpt);
            app.Functions.Add(fnReadAnyPg);

            context.SaveChanges(app);

            // TODO execute other scripts to create functions and stored procedures
        }
    }
}
