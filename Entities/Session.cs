using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    public class Session
    {
        [Key]
        public string Id { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public int Flags { get; set; }

        public int LockCookie { get; set; }
        public bool Locked { get; set; }
        public byte[] Item { get; set; }
        public int Timeout { get; set; }
    }
}
