using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottyApps.EFCodeFirstProviders.Entities;

namespace ScottyApps.EFCodeFirstProviders.DomainTest
{
    [TestClass]
    public class DomainTest
    {
        [TestMethod]
        public void TriggerTheDbCreationTest()
        {
            Application app = null;
            using(MembershipContext ctx = new MembershipContext("MembershipContext"))
            {
                app = ctx.Applications.Find(Guid.Empty);
            }
            Assert.IsNull(app);
        }
    }
}
