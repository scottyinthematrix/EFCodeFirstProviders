using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using ScottyApps.Utilities.DbContextExtensions;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    // TODO
    // some fields may never be needed
    public class User : EntityBase
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsAnonymous { get; set; }
        public DateTime? LastActiveDate { get; set; }

        // NOTE below fields originally belongs to a separate entity: Membership
        [Required]
        public string Password { get; set; }

        public string Email { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        // this is used to be IsApproved
        public bool IsConfirmed { get; set; }
        public bool IsLockedOut { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastPasswordChangedDate { get; set; }
        public DateTime? LastLockoutDate { get; set; }
        public int FailedPasswordAttempCount { get; set; }
        public DateTime? FailedPasswordAttempWindowStart { get; set; }
        public int FailedPasswordAnswerAttempCount { get; set; }
        public DateTime? FailedPasswordAnswerAttempWindowStart { get; set; }

        [MaxLength(256)]
        public string Comment { get; set; }
        public virtual Application Application { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<Profile> Profiles { get; set; }

        public User()
        {
            CreateDate = DateTime.Now;
            Roles = new Collection<Role>();
            Profiles = new Collection<Profile>();
        }
    }
}
