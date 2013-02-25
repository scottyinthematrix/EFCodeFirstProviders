using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ScottyApps.EFCodeFirstProviders.Entities;
using ScottyApps.Utilities.DbContextExtensions;

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
            var funcs = GetFunctionsInRole(roleName);
            if (funcs == null || funcs.Count == 0)
            {
                return false;
            }

            return funcs.Exists(f => f.Name.ToLower() == funcName.ToLower());
        }
        public bool UserHasFunction(string userName, string funcName)
        {
            var funcs = GetFunctionsForUser(userName);
            if (funcs == null || funcs.Count == 0)
            {
                return false;
            }

            return funcs.Exists(f => f.Name.ToLower() == funcName.ToLower());
        }
        public List<Function> GetFunctionsInRole(string roleName)
        {
            using (var ctx = CreateContext())
            {
                var funcs =
                    ctx.Functions.SqlQuery("exec usp_GetFuncsInRole @p0,@p1", roleName, ApplicationName).ToList();
                return funcs;
            }
        }
        public List<Function> GetFunctionsForUser(string userName)
        {
            using (var ctx = CreateContext())
            {
                var functions =
                    ctx.Functions.SqlQuery("exec usp_GetFuncsForUser @p0,@p1", userName, ApplicationName).ToList();
                return functions;
            }
        }
        public List<Role> GetRolesForFunc(string funcName)
        {
            using (var ctx = CreateContext())
            {
                var roles = ctx.Roles.SqlQuery("exec usp_GetRolesForFunc @p0,@p1", funcName, ApplicationName).ToList();
                return roles;
            }
        }
        public void CreateFunction(string funcName, string parentFuncName)
        {
            using (var ctx = CreateContext())
            {
                if(FuncExist(funcName, ctx))
                {
                    throw new ProviderException(Resource.msg_FuncNameDuplication);
                }

                Function pFunc = null;
                if (!string.IsNullOrEmpty(parentFuncName))
                {
                    pFunc = GetFunctionByName(parentFuncName, ctx);
                    if (pFunc == null)
                    {
                        throw new ProviderException(string.Format(Resource.msg_FuncNotExist, parentFuncName));
                    }
                }

                var func = new Function
                               {
                                   Application = ProviderUtils.EnsureApplication(ApplicationName, ctx),
                                   Id = Guid.NewGuid(),
                                   Name = funcName,
                                   Parent = pFunc
                               };
                ctx.Functions.Add(func);
                ctx.SaveChanges();
            }
        }
        public bool DeleteFunction(string funcName)
        {
            using (var ctx = CreateContext())
            {
                var rowsAffected = ctx.Functions.Delete(MatchName(funcName));
                return rowsAffected > 0;
            }
        }
        public bool FuncExist(string funcName)
        {
            using (var ctx = CreateContext())
            {
                return FuncExist(funcName, ctx);
            }
        }
        private bool FuncExist(string funcName, MembershipContext ctx)
        {
            if (string.IsNullOrEmpty(funcName))
            {
                throw new ArgumentNullException("funcName");
            }

            return ctx.Functions.Count(MatchApplication().And(MatchName(funcName)).ToExpressionFunc()) > 0;
        }
        // TODO add test mothod?
        public void AddFunctionsToRoles(string[] funcNames, string[] roleNames)
        {
            throw new NotImplementedException("");
        }

        private MembershipContext CreateContext()
        {
            return new MembershipContext(ConnectionString);
        }
        private Expression<Func<Function, bool>> MatchName(string funcName)
        {
            return f => f.Name.ToLower() == funcName.ToLower();
        }
        private Expression<Func<Function, bool>> MatchApplication()
        {
            return f => f.Application.Name.ToLower() == ApplicationName.ToLower();
        }
        private Function GetFunction(Expression<Func<Function, bool>> predicate, MembershipContext ctx)
        {
            return ctx.Functions.SingleOrDefault(MatchApplication().And(predicate).ToExpressionFunc());
        }
        private Function GetFunctionByName(string funcName, MembershipContext ctx)
        {
            return GetFunction(MatchName(funcName), ctx);
        }
    }
}
