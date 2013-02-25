using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottyApps.EFCodeFirstProviders.Providers;

namespace ScottyApps.EFCodeFirstProviders.DomainTest
{
    [TestClass]
    public class EFFunctionProviderTest
    {
        private EFFunctionProvider _provider;
        [TestInitialize]
        public void Initialize()
        {
            _provider = new EFFunctionProvider(
                ConfigurationManager.ConnectionStrings["membershipDb"].ConnectionString,
                "SalesMgt");
        }
        [TestMethod]
        public void TestFuncExist()
        {
            string funcName = "Read Any Page";
            Assert.IsTrue(_provider.FuncExist(funcName));
        }
        [TestMethod]
        public void TestGetFuntionsInRole()
        {
            var roleName = "SH-IT-Mgr";
            var funcs = _provider.GetFunctionsInRole(roleName);
            Assert.IsNotNull(funcs);
            Assert.IsTrue(funcs.Count > 0);
        }
        [TestMethod]
        public void TestGetFunctionsForUser()
        {
            var userName = "scotty";
            var funcs = _provider.GetFunctionsForUser(userName);
            Assert.IsNotNull(funcs);
            Assert.IsTrue(funcs.Count > 0);
        }
        [TestMethod]
        public void TestUserHasFunction()
        {
            var userName = "juicy";
            var funcName = "Read Any Page";
            Assert.IsTrue(_provider.UserHasFunction(userName, funcName));
        }
        [TestMethod]
        public void TestRoleHasFunction()
        {
            var roleName = "SH-IT-Mgr";
            var funcName = "Read Any Page";

            Assert.IsTrue(_provider.RoleHasFunction(roleName, funcName));
        }
        [TestMethod]
        public void TestGetRolesForFunc()
        {
            var funcName = "Read Any Page";
            var roles = _provider.GetRolesForFunc(funcName);
            Assert.IsNotNull(roles);
            Assert.IsTrue(roles.Count == 7);

            funcName = "ManageSalesReport";
            roles = _provider.GetRolesForFunc(funcName);
            Assert.IsNotNull(roles);
            Assert.IsTrue(roles.Count == 1);
        }
        [TestMethod]
        public void TestCreateFunction()
        {
            var funcName = "Edit Report";
            var pFuncName = "ManageSalesReport";
            _provider.CreateFunction(funcName, pFuncName);

            Assert.Inconclusive("happy ending");
        }
        [TestMethod]
        public void TestDeleteFunction()
        {
            var funcName = "Edit Report";

            Assert.IsTrue(_provider.DeleteFunction(funcName));
        }
    }
}
