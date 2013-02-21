using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ScottyApps.Utilities.DbContextExtensions;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    public class Application : EntityBase
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Function> Functions { get; set; }

        public Application()
        {
            Roles = new Collection<Role>();
            Users = new Collection<User>();
            Functions = new Collection<Function>();
        }
    }
}
