using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottyApps.EFCodeFirstProviders.Providers;

namespace ScottyApps.EFCodeFirstProviders.DomainTest
{
    [TestClass]
    public class EFRoleProviderTest
    {
        private EFRoleProvider _provider;
        [TestInitialize]
        public void Initialize()
        {
            _provider= new EFRoleProvider();
            _provider.ApplicationName = "SalesMgt";
            _provider.ConnectionString = ConfigurationManager.ConnectionStrings["membershipDb"].ConnectionString;
        }
        [TestMethod]
        public void TestGetRoles()
        {
            var userName = "scotty";
            var roles = _provider.GetRoles(userName);

            Assert.IsNotNull(roles);
        }
    }
}
