using System;
using System.Configuration;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottyApps.EFCodeFirstProviders.Providers;

namespace ScottyApps.EFCodeFirstProviders.DomainTest
{
    [TestClass]
    public class EFMembershipProviderTest
    {
        private EFMembershipProvider _provider;
        [TestInitialize]
        public void Initialize()
        {
            _provider = new EFMembershipProvider();
            _provider.ApplicationName = "SalesMgt";
            _provider.ConnectionString = ConfigurationManager.ConnectionStrings["membershipDb"].ConnectionString;

            _provider.Initialize();
        }
        [TestMethod]
        public void TestChangePassword()
        {
            string userName = "scotty";
            Assert.IsTrue(_provider.ChangePassword(userName, "ZAQ!xsw2", "cppfans#"));
        }
        [TestMethod]
        public void TestUnlockUser()
        {
            string userName = "scotty";
            Assert.IsTrue(_provider.UnlockUser(userName));
        }
        // TODO test CheckPwdComplexity
        // TODO test Encrypt/Decrypt Password
    }
}
