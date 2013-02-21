using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using ScottyApps.Utilities.DbContextExtensions;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    public class Function : EntityBase
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        public virtual Function Parent { get; set; }
        public virtual Application Application { get; set; }
        public virtual ICollection<Function> Children { get; set; }
        public virtual ICollection<Role> Roles { get; set; }

        public Function()
        {
            Children = new Collection<Function>();
            Roles = new Collection<Role>();
        }
    }
}
