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
            _provider = new EFRoleProvider();
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
        [TestMethod]
        public void TestCreateRole()
        {
            var roleName = "CQ-IT-Mgr";
            var pRoleName = "ITManager";

            _provider.CreateRole(roleName, pRoleName);
            Assert.IsTrue(_provider.RoleExists(roleName), "{0} (under {1}) should have been created", roleName, pRoleName);
        }
        [TestMethod]
        public void TestDeleteRole()
        {
            var roleName = "CQ-IT-Mgr";
            _provider.DeleteRole(roleName, false);

            Assert.IsFalse(_provider.RoleExists(roleName), "the role {0} should be deleted", roleName);
        }
        [TestMethod]
        public void TestGetUsers()
        {
            var roleName = "Passenger";
            var users = _provider.GetUsers(roleName);

            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count == 2);

            var userNameToMatch = "i";
            users = _provider.GetUsers(roleName, userNameToMatch);
            Assert.IsNotNull(users);
            Assert.AreEqual("juicy", users[0].Name);

            roleName = "WH-Market-Mgr";
            users = _provider.GetUsers(roleName);
            Assert.AreEqual("juicy", users[0].Name);
        }
        [TestMethod]
        public void TestGetAllRoles()
        {
            var roles = _provider.GetRoles();
            Assert.IsNotNull(roles);
            Assert.IsTrue(roles.Count > 0);
        }
    }
}
