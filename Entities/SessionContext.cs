using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace ScottyApps.EFCodeFirstProviders.Entities
{
    public class SessionContext : DbContext
    {
        public DbSet<Session> Sessions { get; set; }
    }
}
