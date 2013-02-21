﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Hosting;
using System.Web.Security;
using ScottyApps.EFCodeFirstProviders.Entities;
using ScottyApps.Utilities.DbContextExtensions;

namespace ScottyApps.EFCodeFirstProviders.Providers
{
    internal class RoleComparer : IEqualityComparer<Role>
    {
        public bool Equals(Role x, Role y)
        {
            return x.Name.ToLower() == y.Name.ToLower();
        }

        public int GetHashCode(Role obj)
        {
            return obj.GetHashCode();
        }
    }

    public class EFRoleProvider : RoleProvider
    {
        // TODO shift back to private
        public string ConnectionString { get; set; }

        #region override methods

        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (String.IsNullOrEmpty(name))
            {
                name = "EFRoleProvider";
            }

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Scotty EF Code First Role Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            ApplicationName =
                (string)
                ProviderUtils.GetConfigValue(config, "applicationName", HostingEnvironment.ApplicationVirtualPath);

            // Read connection string.
            ConnectionStringSettings connectionStringSettings =
                ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (connectionStringSettings == null || connectionStringSettings.ConnectionString.Trim() == string.Empty)
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            ConnectionString = connectionStringSettings.ConnectionString;
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var roles = GetRolesForUser(username);
            if (roles == null || roles.Length == 0)
            {
                return false;
            }

            return roles.Any(r => r.ToLower() == roleName.ToLower());
        }

        public List<Role> GetRoles(string username)
        {
            using (var ctx = CreateContext())
            {
                ObjectParameter paraUserName = new ObjectParameter("userName", username);
                ObjectParameter paraAppName = new ObjectParameter("appName", ApplicationName);
                var roles = (ctx as IObjectContextAdapter).ObjectContext.ExecuteStoreQuery<Role>(
                    "ufn_GetRolesForUser", username, ApplicationName).ToList();
                    //ctx.Database.SqlQuery<Role>("exec dbo.ufn_GetRolesForUser @userName='{0}', @appName='{1}'",
                    //                              username, ApplicationName).ToList();
                return roles;
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var ctx = CreateContext())
            {
                var roleNames =
                    ctx.Database.SqlQuery<string>("exec dbo.usp_GetRolesForUser @userName='{0}', @appName='{1}'",
                                                  username, ApplicationName).ToList();
                return roleNames.ToArray();
            }
        }

        public override void CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            using (var ctx = CreateContext())
            {
                if (RoleExists(roleName, ctx))
                {
                    throw new ProviderException(Resource.msg_RoleNameDuplication);
                }

                var app = ProviderUtils.EnsureApplication(ApplicationName, ctx);
                var role = new Role
                               {
                                   Id = Guid.NewGuid(),
                                   Name = roleName,
                                   Application = app
                               };
                ctx.Roles.Add(role);
                ctx.SaveChanges();
            }
        }
        // TODO add a child role
        public void CreateRole(string roleName, string parentRoleName)
        {
            
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            using (var ctx = CreateContext())
            {
                var role = GetRole(r => r.Name.ToLower() == roleName, ctx);
                if (role == null)
                {
                    return false;
                }

                ctx.Roles.Remove(role);
                ctx.SaveChanges();
            }

            // NOTE
            // if roles get deleted, corresponding records in usersinroles will be deleted at the same time
            // (this is an automatic and default behavior of code first model builder)
            // which means, parameter throwOnPopulatedRole is never used
            return true;
        }

        public override bool RoleExists(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException("roleName");
            }

            bool exist = false;
            using (var ctx = CreateContext())
            {
                exist = RoleExists(roleName, ctx);
            }
            return exist;
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null || usernames.Length == 0
                || roleNames == null || roleNames.Length == 0)
            {
                return;
            }
            var loweredUserNames = usernames.Select(s => s.ToLower()).Distinct().ToArray();
            var loweredRoleNames = roleNames.Select(s => s.ToLower()).Distinct().ToArray();
            using (var ctx = CreateContext())
            {
                var queryUsers = from u in ctx.Users.Include("Roles")
                                 where
                                     u.Application.Name.ToLower() == ApplicationName.ToLower()
                                     && loweredUserNames.Contains(u.Name.ToLower())
                                 select u;
                var users = queryUsers.ToList();

                var queryRoles = from r in ctx.Roles
                                 where
                                     r.Application.Name.ToLower() == ApplicationName.ToLower() &&
                                     loweredRoleNames.Contains(r.Name.ToLower())
                                 select r;
                var roles = queryRoles.ToList();

                foreach (var user in users)
                {
                    var newRoles = roles.Except(user.Roles, new RoleComparer());
                    newRoles.ToList().ForEach(r => user.Roles.Add(r));
                }

                ctx.SaveChanges();
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            if (usernames == null || usernames.Length == 0
                || roleNames == null || roleNames.Length == 0)
            {
                return;
            }

            var loweredUserNames = usernames.Select(s => s.ToLower()).Distinct().ToArray();
            var loweredRoleNames = roleNames.Select(s => s.ToLower()).Distinct().ToArray();
            using (var ctx = CreateContext())
            {
                var queryUsers = from u in ctx.Users.Include("Roles")
                                 where
                                     u.Application.Name.ToLower() == ApplicationName.ToLower()
                                     && loweredUserNames.Contains(u.Name.ToLower())
                                 select u;
                var users = queryUsers.ToList();

                var queryRoles = from r in ctx.Roles
                                 where
                                     r.Application.Name.ToLower() == ApplicationName.ToLower() &&
                                     loweredRoleNames.Contains(r.Name.ToLower())
                                 select r;
                var roles = queryRoles.ToList();

                foreach (var user in users)
                {
                    var newRoles = roles.Except(user.Roles, new RoleComparer());
                    newRoles.ToList().ForEach(r => user.Roles.Remove(r));
                }

                ctx.SaveChanges();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (var ctx = CreateContext())
            {
                var userNames =
                    ctx.Database.SqlQuery<string>("exec dbo.usp_GetUsersInRole @roleName='{0}', @appName='{1}'",
                                                  roleName, ApplicationName)
                       .ToList();
                return userNames.ToArray();
            }
        }

        public override string[] GetAllRoles()
        {
            using (var ctx = CreateContext())
            {
                var roleNames = ctx.Roles.Where(MatchApplication()).Select(r => r.Name).ToArray();
                return roleNames;
            }
        }
        // TODO return roles in JSON
        public IEnumerable<Role> GetRoles()
        {
            throw new NotImplementedException("");
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (var ctx = CreateContext())
            {
                var userNames =
                    ctx.Database.SqlQuery<string>(
                        "exec dbo.usp_GetUsersInRole @roleName='{0}', @appName='{1}', @userName='{2}'", roleName,
                        ApplicationName, usernameToMatch)
                       .ToList();
                return userNames.ToArray();
            }
        }

        public override string ApplicationName { get; set; }

        #endregion

        #region private methods

        private MembershipContext CreateContext()
        {
            return new MembershipContext(ConnectionString);
        }

        private Expression<Func<Role, bool>> MatchApplication()
        {
            return r => r.Application.Name == ApplicationName;
        }

        private Expression<Func<Role, bool>> MatchName(string roleName)
        {
            return r => r.Name.ToLower() == roleName.ToLower();
        }

        private Role GetRole(Expression<Func<Role, bool>> predicate, MembershipContext ctx)
        {
            return ctx.Roles.Where(MatchApplication()).Where(predicate).Single();
        }

        private bool RoleExists(string roleName, MembershipContext ctx)
        {
            bool exist = false;
            var predicate = MatchApplication().And(MatchName(roleName));
            exist = ctx.Roles.Count(predicate.ToExpressionFunc()) > 0;
            return exist;
        }

        #endregion
    }
}
