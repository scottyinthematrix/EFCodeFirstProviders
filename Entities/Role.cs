﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    public class Role
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string Description { get; set; }

        public virtual Role Parent { get; set; }
        public virtual ICollection<Role> Children { get; set; }
        public virtual Application Application { get; set; }
        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<Function> Functions { get; set; }
    }
}