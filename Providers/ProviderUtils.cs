using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ScottyApps.EFCodeFirstProviders.Entities;

namespace ScottyApps.EFCodeFirstProviders.Providers
{
    internal class ProviderUtils
    {
        delegate Func<T, R> Recursive<T, R>(Recursive<T, R> r);
        static Func<T, R> Y<T, R>(Func<Func<T, R>, Func<T, R>> f)
        {
            Recursive<T, R> rec = r => a => f(r(r))(a);
            return rec(rec);
        }

        /// <summary>
        /// A helper function to retrieve config values from the configuration file.
        /// </summary>
        /// <param name="config">Provider configuration.</param>
        /// <param name="configKey">Key of the configuration that should be read.</param>
        /// <param name="defaultValue">Default value being used if the config does not exist.</param>
        /// <returns>Configuration value or default value if not exisiting.</returns>
        internal static object GetConfigValue(NameValueCollection config, string configKey, object defaultValue)
        {
            object configValue;

            try
            {
                configValue = config[configKey];
                configValue = string.IsNullOrEmpty(configValue.ToString()) ? defaultValue : configValue;
            }
            catch
            {
                configValue = defaultValue;
            }

            return configValue;
        }

        /// <summary>
        /// Ensure that application exists. If not -> create new application.
        /// </summary>
        /// <param name="applicationName">Application name.</param>
        /// <param name="context">Membership data context.</param>
        /// <returns>The application object</returns>
        internal static Application EnsureApplication(string applicationName, MembershipContext context)
        {
            Application application = context.Applications.FirstOrDefault(a => a.Name == applicationName);
            if (application == null)
            {
                // Create application
                application = new Application
                                  {
                                      Id = Guid.NewGuid(),
                                      Description = string.Format("description for {0}", applicationName),
                                      Name = applicationName
                                  };
                context.Applications.Add(application);
                context.SaveChanges();
            }

            return application;
        }

        internal static T CreateContext<T>(string connStr)
            where T : DbContext
        {
            return (T)Activator.CreateInstance(typeof(T), connStr);
        }

    }
}
