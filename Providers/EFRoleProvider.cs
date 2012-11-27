using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using ScottyApps.EFCodeFirstProviders.Entities;

namespace ScottyApps.EFCodeFirstProviders.Providers
{
    public class EFRoleProvider : RoleProvider
    {

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            using (MembershipContext ctx = new MembershipContext())
            {
                var roleNames = ctx.Database.SqlQuery<string>("dbo.usp_GetRolesForUser @userName @appName", username, ApplicationName).ToList();
                return roleNames.ToArray();
            }
        }

        private void GetParentRoles(Role role, ref Hashtable roles)
        {
            using (MembershipContext ctx = new MembershipContext())
            {
                var query = role.Parent.Parent.Parent;
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }
    }
}
