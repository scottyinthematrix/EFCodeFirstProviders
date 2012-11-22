using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Provider;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
using ScottyApps.EFCodeFirstProviders.Entities;

namespace ScottyApps.EFCodeFirstProviders.Providers
{
    public class EFMembershipProvider : MembershipProvider
    {
        #region members
        private int maxInvalidPasswordAttempts;
        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        private int minRequiredNonAlphanumericCharacters;
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonAlphanumericCharacters; }
        }

        private int minRequiredPasswordLength;
        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        private int passwordAttemptWindow;

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </returns>
        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        private MembershipPasswordFormat passwordFormat;
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return passwordFormat; }
        }

        private string passwordStrengthRegularExpression;
        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        private bool requiresQuestionAndAnswer;
        public override bool RequiresQuestionAndAnswer
        {
            get { return requiresQuestionAndAnswer; }
        }

        private bool requiresUniqueEmail;
        public override bool RequiresUniqueEmail
        {
            get { return requiresUniqueEmail; }
        }

        private bool enablePasswordReset;
        public override bool EnablePasswordReset
        {
            get { return enablePasswordReset; }
        }

        private bool enablePasswordRetrieval;
        public override bool EnablePasswordRetrieval
        {
            get { return enablePasswordRetrieval; }
        }

        public string ConnectionString { get; set; }

        #endregion

        #region override methods

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Initialize values from web.config.
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "EFMembershipProvider";
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Scotty EF Code First Membership Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            ApplicationName = Convert.ToString(ProviderUtils.GetConfigValue(config, "applicationName", HostingEnvironment.ApplicationVirtualPath));
            maxInvalidPasswordAttempts = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "maxInvalidPasswordAttempts", "5"));
            passwordAttemptWindow = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "passwordAttemptWindow", "10"));
            minRequiredNonAlphanumericCharacters = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "minRequiredNonAlphanumericCharacters", "1"));
            minRequiredPasswordLength = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "minRequiredPasswordLength", "7"));
            passwordStrengthRegularExpression = Convert.ToString(ProviderUtils.GetConfigValue(config, "passwordStrengthRegularExpression", string.Empty));
            enablePasswordReset = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "enablePasswordReset", "true"));
            enablePasswordRetrieval = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "enablePasswordRetrieval", "false"));
            requiresQuestionAndAnswer = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "requiresQuestionAndAnswer", "true"));
            requiresUniqueEmail = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "requiresUniqueEmail", "true"));

            if (!string.IsNullOrEmpty(passwordStrengthRegularExpression))
            {
                passwordStrengthRegularExpression = passwordStrengthRegularExpression.Trim();
                if (!string.IsNullOrEmpty(passwordStrengthRegularExpression))
                {
                    try
                    {
                        new Regex(passwordStrengthRegularExpression);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ProviderException(ex.Message, ex);
                    }
                }

                if (minRequiredPasswordLength < minRequiredNonAlphanumericCharacters)
                {
                    throw new ProviderException("Minimal required non alphanumeric characters cannot be longer than the minimum required password length.");
                }
            }

            string format = config["passwordFormat"] ?? "Hashed";

            switch (format)
            {
                case "Hashed":
                    passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }

            // Initialize SqlConnection.
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (connectionStringSettings == null || string.IsNullOrEmpty(connectionStringSettings.ConnectionString.Trim()))
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            ConnectionString = connectionStringSettings.ConnectionString;

            // Get encryption and decryption key information from the configuration.
            //Configuration configuration = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            //machineKey = (MachineKeySection)configuration.GetSection("system.web/machineKey");

            //if (machineKey.ValidationKey.Contains("AutoGenerate"))
            //{
            //    if (PasswordFormat != MembershipPasswordFormat.Clear)
            //    {
            //        throw new ProviderException("Hashed or Encrypted passwords are not supported with auto-generated keys.");
            //    }
            //}

        }

        protected override void OnValidatingPassword(ValidatePasswordEventArgs e)
        {
            base.OnValidatingPassword(e);
        }
        public override string ApplicationName { get; set; }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (!ValidateUser(username, oldPassword))
            {
                return false;
            }
            if (!CheckPwdComplexity(newPassword))
            {
                return false;
            }
            using (MembershipContext ctx = new MembershipContext())
            {
                User user =
                    ctx.Users.First(u => string.Compare(u.Name, username, StringComparison.OrdinalIgnoreCase) == 0);
                user.Password = EncryptPassword(newPassword);
                ctx.SaveChanges();
            }

            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (!ValidateUser(username, password))
            {
                return false;
            }

            using (MembershipContext ctx = new MembershipContext())
            {
                User user =
                    ctx.Users.First(u => string.Compare(u.Name, username, StringComparison.OrdinalIgnoreCase) == 0);
                user.PasswordQuestion = newPasswordQuestion;
                user.PasswordAnswer = newPasswordAnswer;
                ctx.SaveChanges();
            }

            return true;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isConfirmed, object providerUserKey, out MembershipCreateStatus status)
        {
            status = MembershipCreateStatus.Success;

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (!CheckPwdComplexity(password))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            MembershipUser membershipUser = null;
            using (MembershipContext ctx = new MembershipContext())
            {
                User user = null;
                // check username uniqueness
                user = GetUser(u => u.Name.ToLower() == username.ToLower(), ctx);
                if (user != null)
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                    return null;
                }

                // check email uniqueness
                if (RequiresUniqueEmail)
                {
                    user = GetUser(u => u.Email.ToLower() == email.ToLower(), ctx);
                    if (user != null)
                    {
                        status = MembershipCreateStatus.DuplicateEmail;
                        return null;
                    }
                }

                user = new User
                                {
                                    Name = username,
                                    Password = EncryptPassword(password),
                                    Email = email,
                                    CreateDate = DateTime.Now,
                                    Application = ProviderUtils.EnsureApplication(ApplicationName, ctx),
                                    Id = Guid.NewGuid(),
                                    IsConfirmed = false,  // this should always be false
                                    PasswordAnswer = passwordAnswer,
                                    PasswordQuestion = passwordQuestion
                                };
                ctx.Users.Add(user);
                ctx.SaveChanges();

                membershipUser = GetMembershipUserFromUser(user);
            }

            return membershipUser;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (MembershipContext ctx = new MembershipContext())
            {
                User user = GetUser(u => u.Name.ToLower() == username.ToLower(), ctx);
                if (user == null)
                {
                    return false;
                }

                // TODO delete related data : UsersInRoles, Profiles
            }

            return true;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            using (MembershipContext ctx = new MembershipContext())
            {
                var query = ctx.Users.Where(MatchApplication());
                if (!string.IsNullOrEmpty(emailToMatch.Trim()))
                {
                    query = query.Where(u => u.Email.ToLower().Contains(emailToMatch.ToLower()));
                }
                totalRecords = query.Count();

                if (totalRecords > 0)
                {
                    query.Skip(pageIndex * pageSize).Take(pageSize).ToList().ForEach(u => membershipUsers.Add(GetMembershipUserFromUser(u)));
                }
            }

            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            using (MembershipContext ctx = new MembershipContext())
            {
                var query = ctx.Users.Where(MatchApplication());
                if (!string.IsNullOrEmpty(usernameToMatch.Trim()))
                {
                    query = query.Where(u => u.Name.ToLower().Contains(usernameToMatch.ToLower()));
                }
                totalRecords = query.Count();

                if (totalRecords > 0)
                {
                    query.Skip(pageIndex * pageSize).Take(pageSize).ToList().ForEach(u => membershipUsers.Add(GetMembershipUserFromUser(u)));
                }
            }

            return membershipUsers;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            using (MembershipContext ctx = new MembershipContext())
            {
                var query = ctx.Users.Where(MatchApplication());
                totalRecords = query.Count();

                if (totalRecords > 0)
                {
                    query
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize)
                        .ToList()
                        .ForEach(u => membershipUsers.Add(GetMembershipUserFromUser(u)));
                }
            }

            return membershipUsers;
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            using (MembershipContext context = new MembershipContext())
            {
                return context.Users.Where(MatchApplication()).Count(u => u.LastActiveDate > compareTime);
            }
        }

        public override string GetPassword(string username, string answer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException(Resource.msg_PwdRetrivalNotEnabled);
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException(Resource.msg_CannotRetrieveHashedPwd);
            }

            string pwd;

            using (MembershipContext ctx = new MembershipContext())
            {
                User user = GetUser(u => u.Name.ToLower() == username.ToLower(), ctx);
                if (user == null)
                {
                    throw new EFMemberException(EFMembershipValidationStatus.UserNotExist, string.Format(Resource.msg_UserNotExist, username));
                }
                if (!user.IsConfirmed)
                {
                    throw new EFMemberException(EFMembershipValidationStatus.UserNotConfirmed, string.Format(Resource.msg_UserNotConfirmed, username));
                }
                if (user.IsLockedOut)
                {
                    throw new EFMemberException(EFMembershipValidationStatus.UserIsLockedOut, string.Format(Resource.msg_UserLockedOut, username));
                }

                pwd = user.Password;
                if (PasswordFormat == MembershipPasswordFormat.Encrypted)
                {
                    pwd = DecryptPassword(user.Password);
                }
            }

            return pwd;
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        /// <param name="username">The name of the user to get information for. </param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user. </param>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser membershipUser = null;
            using (MembershipContext ctx = new MembershipContext())
            {
                User user = GetUser(u => u.Name.ToLower() == username.ToLower(), ctx);
                if (user != null)
                {
                    membershipUser = GetMembershipUserFromUser(user);
                    if (userIsOnline)
                    {
                        user.LastActiveDate = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
            }

            return membershipUser;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            Guid userId = (Guid)providerUserKey;
            MembershipUser membershipUser = null;
            using (MembershipContext ctx = new MembershipContext())
            {
                User user = GetUser(u => u.Id == userId, ctx);
                if (user != null)
                {
                    membershipUser = GetMembershipUserFromUser(user);
                    if (userIsOnline)
                    {
                        user.LastActiveDate = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
            }

            return membershipUser;
        }

        public override string GetUserNameByEmail(string email)
        {
            string userName = string.Empty;

            using (MembershipContext ctx = new MembershipContext())
            {
                var user = ctx.Users.Where(MatchApplication()).Single(u => u.Email.ToLower() == email.ToLower());
                if (user != null)
                {
                    userName = user.Name;
                }
            }

            return userName;
        }

        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new ProviderException(Resource.msg_PwdResetNotEnabled);
            }

            if (RequiresQuestionAndAnswer && string.IsNullOrEmpty(answer))
            {
                throw new ProviderException(Resource.msg_AnswerRequiredOnPwdReset);
            }

            string newPwd;
            using (MembershipContext ctx = new MembershipContext())
            {
                var user = GetUser(u => u.Name.ToLower() == username.ToLower(), ctx);
                if (user == null)
                {
                    throw new ProviderException(string.Format(Resource.msg_UserNotExist, username));
                }
                if (user.IsLockedOut)
                {
                    throw new ProviderException(string.Format(Resource.msg_UserLockedOut, username));
                }
                if (string.Compare(user.PasswordAnswer, answer, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new ProviderException(string.Format(Resource.msg_AnswerNotMatched, username));
                }

                newPwd = Membership.GeneratePassword(MinRequiredPasswordLength,
                                                              MinRequiredNonAlphanumericCharacters);
                user.Password = EncryptPassword(newPwd);
                user.LastPasswordChangedDate = DateTime.Now;
                ctx.SaveChanges();
            }

            return newPwd;
        }

        public override bool UnlockUser(string userName)
        {
            using (MembershipContext ctx = new MembershipContext())
            {
                var user = GetUser(u => u.Name.ToLower() == userName.ToLower(), ctx);
                if (user == null)
                {
                    throw new ProviderException(Resource.msg_UserNotExist);
                }
                user.IsLockedOut = false;
                user.LastLockoutDate = DateTime.Now;
                ctx.SaveChanges();
            }
            return true;
        }

        public override void UpdateUser(MembershipUser membershipUser)
        {
            using (MembershipContext ctx = new MembershipContext())
            {
                var user = GetUser(u => u.Name.ToLower() == membershipUser.UserName.ToLower(), ctx);

                if (RequiresUniqueEmail)
                {
                    var userWithSameEmail = GetUser(u => u.Email.ToLower() == membershipUser.Email.ToLower(), ctx);
                    if (userWithSameEmail != null && userWithSameEmail.Name != user.Email)
                    {
                        throw new ProviderException(Resource.msg_EmailDuplication);
                    }
                }
                user.Email = membershipUser.Email;

                user.IsConfirmed = membershipUser.IsApproved;
                if (user.IsLockedOut && !membershipUser.IsLockedOut)
                {
                    user.IsLockedOut = membershipUser.IsLockedOut;
                    user.LastLockoutDate = DateTime.Now;
                }

                ctx.SaveChanges();
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            using (MembershipContext ctx = new MembershipContext())
            {
                User user = ctx.Users.First(u => String.Compare(u.Name, username, StringComparison.OrdinalIgnoreCase) == 0);
                if (user == null)
                {
                    throw new EFMemberException(EFMembershipValidationStatus.UserNotExist, string.Format(Resource.msg_UserNotExist, username));
                }
                if (!user.IsConfirmed)
                {
                    throw new EFMemberException(EFMembershipValidationStatus.UserNotConfirmed, string.Format(Resource.msg_UserNotConfirmed, username));
                }
                if (!user.IsLockedOut)
                {
                    throw new EFMemberException(EFMembershipValidationStatus.UserIsLockedOut, string.Format(Resource.msg_UserLockedOut, username));
                }

                string encryptedPwd = EncryptPassword(password);
                if (encryptedPwd != user.Password)
                {
                    // update falure password attempt count
                    DateTime? lastFailureTry = user.FailedPasswordAttempWindowStart;
                    if (lastFailureTry == null || DateTime.Now > lastFailureTry.Value.AddMinutes(PasswordAttemptWindow))
                    {
                        user.FailedPasswordAttempWindowStart = DateTime.Now;
                        user.FailedPasswordAttempCount = 1;
                    }
                    else
                    {
                        ++user.FailedPasswordAttempCount;
                    }

                    if (user.FailedPasswordAttempCount >= MaxInvalidPasswordAttempts)
                    {
                        user.IsLockedOut = true;
                        user.LastLockoutDate = DateTime.Now;
                    }
                    ctx.SaveChanges();

                    throw new EFMemberException(EFMembershipValidationStatus.WrongPassword, string.Format(Resource.msg_WrongPassword, username));
                }

                // update last date
                user.LastActiveDate = DateTime.Now;
                user.LastLoginDate = DateTime.Now;
                ctx.SaveChanges();
            }

            return true;
        }

        #endregion

        #region private methods

        // TODO
        // encryp according to the PasswordFormat
        private string EncryptPassword(string clearText)
        {
            string encrypted = clearText;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    // TODO
                    break;
                case MembershipPasswordFormat.Hashed:
                    break;
            }

            return encrypted;
        }
        // TODO
        // decrypt according to the PasswordFormat
        private string DecryptPassword(string password)
        {
            string decrypted = password;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException(Resource.msg_CannotDecryptHashedPwd);
                case MembershipPasswordFormat.Encrypted:
                    // TODO you know what should be done here.
                    break;
            }
            return decrypted;
        }
        private bool CheckPwdComplexity(string clearText)
        {
            return (
                       !string.IsNullOrEmpty(clearText)
                       && clearText.Length >= MinRequiredPasswordLength
                       && GetSpecialCharsCount(clearText) >= minRequiredNonAlphanumericCharacters
                   );
        }
        private int GetSpecialCharsCount(string s)
        {
            int count = 0;
            s.ToList().ForEach(c =>
            {
                int i;
                if (!int.TryParse(c.ToString(CultureInfo.InvariantCulture), out i)
                    && !(('a' < c && c < 'z') || ('A' < c && c < 'Z')))
                {
                    ++count;
                }
            });
            return count;
        }

        private MembershipUser GetMembershipUserFromUser(User user)
        {
            return new MembershipUser(
                                      Name,
                                      user.Name,
                                      user.Id,
                                      user.Email,
                                      user.PasswordQuestion,
                                      user.Comment,
                                      user.IsConfirmed,
                                      user.IsLockedOut,
                                      user.CreateDate,
                                      user.LastLoginDate.GetValueOrDefault(),
                                      user.LastActiveDate.GetValueOrDefault(),
                                      user.LastPasswordChangedDate.GetValueOrDefault(),
                                      user.LastLockoutDate.GetValueOrDefault());
        }

        private User GetUser(Expression<Func<User, bool>> query, MembershipContext context)
        {
            User user = context.Users.Where(query).Where(MatchApplication()).First();

            return user;
        }

        /// <summary>
        /// Matches the local application name.
        /// </summary>
        /// <returns>Status whether passed in user matches the application.</returns>
        private Expression<Func<User, bool>> MatchApplication()
        {
            return user => user.Application.Name.ToLower() == ApplicationName.ToLower();
        }

        #endregion

    }
}
