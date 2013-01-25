using System;
using System.ComponentModel.DataAnnotations;
using ScottyApps.Utilities.DbContextExtensions;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    public class Profile : EntityBase
    {
        [Key]
        //[ForeignKey("UserId")]
        public Guid UserId { get; set; }
        [Key]
        public string PropertyName { get; set; }
        public string PropertyValueStrings { get; set; }
        public byte[] PropertyValueBinary { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public virtual User User { get; set; }
    }
}
