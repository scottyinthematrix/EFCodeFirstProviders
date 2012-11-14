using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    public class Profile
    {
        // TODO
        // ok, here's a problem, here the PK consists of PropertyName and UserId, what should i do here?
        public string PropertyName { get; set; }
        public string PropertyValueStrings { get; set; }
        public byte[] PropertyValueBinary { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public virtual User User { get; set; }
    }
}
