using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ScottyApps.EFCodeFirstProviders.Entities;

namespace ScottyApps.EFCodeFirstProviders.Providers
{
    public class EFFunctionProvider
    {
        private string ConnectionString { get; set; }
        public string ApplicationName { get; set; }

        public EFFunctionProvider(string connStr, string appName)
        {
            Initialize(connStr, appName);
        }

        public void Initialize(string connStr, string appName)
        {
            // TODO in future, need to read from configuration file to populate parameters
            this.ConnectionString = connStr;
            this.ApplicationName = appName;
        }

        public bool RoleHasFunction(string roleName, string funcName)
        {
            throw new NotImplementedException("");
        }
        public IEnumerable<Function> GetFuntionsForRole(string roleName, string funcNameToMatch)
        {
            throw new NotImplementedException("");
        }
        public void CreateFunction(string funcName, string parentFuncName)
        {
            throw new NotImplementedException(""); 
        }
        public bool DeleteFunction(string funcName)
        {
            throw new NotImplementedException("");
        }
        public bool FuncExist(string funcName)
        {
            throw new NotImplementedException("");
        }
        public void AddFunctionsToRoles(string[] funcNames, string[] roleNames)
        {
            throw new NotImplementedException("");
        }

        private MembershipContext CreateContext()
        {
            return new MembershipContext(ConnectionString);
        }

        private Expression<Func<Function, bool>> MatchApplication()
        {
            return f => f.Application.Name == ApplicationName;
        }

    }
}
