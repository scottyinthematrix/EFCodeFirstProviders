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
using ScottyApps.Utilities.DbContextExtensions;

namespace ScottyApps.EFCodeFirstProviders.Providers
{
    public class EFMembershipProvider : MembershipProvider
    {
        #region members
        private int _maxInvalidPasswordAttempts;
        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        private int _minRequiredNonAlphanumericCharacters;
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        private int _minRequiredPasswordLength;
        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        private int _passwordAttemptWindow;

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>
        /// The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </returns>
        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        private MembershipPasswordFormat _passwordFormat;
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        private string _passwordStrengthRegularExpression;
        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        private bool _requiresQuestionAndAnswer;
        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        private bool _requiresUniqueEmail;
        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        private bool _enablePasswordReset;
        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        private bool _enablePasswordRetrieval;
        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
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
            _maxInvalidPasswordAttempts = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "maxInvalidPasswordAttempts", "5"));
            _passwordAttemptWindow = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "passwordAttemptWindow", "10"));
            _minRequiredNonAlphanumericCharacters = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "minRequiredNonAlphanumericCharacters", "1"));
            _minRequiredPasswordLength = Convert.ToInt32(ProviderUtils.GetConfigValue(config, "minRequiredPasswordLength", "7"));
            _passwordStrengthRegularExpression = Convert.ToString(ProviderUtils.GetConfigValue(config, "passwordStrengthRegularExpression", string.Empty));
            _enablePasswordReset = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "enablePasswordReset", "true"));
            _enablePasswordRetrieval = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "enablePasswordRetrieval", "false"));
            _requiresQuestionAndAnswer = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "requiresQuestionAndAnswer", "true"));
            _requiresUniqueEmail = Convert.ToBoolean(ProviderUtils.GetConfigValue(config, "requiresUniqueEmail", "true"));

            if (!string.IsNullOrEmpty(_passwordStrengthRegularExpression))
            {
                _passwordStrengthRegularExpression = _passwordStrengthRegularExpression.Trim();
                if (!string.IsNullOrEmpty(_passwordStrengthRegularExpression))
                {
                    try
                    {
                        new Regex(_passwordStrengthRegularExpression);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ProviderException(ex.Message, ex);
                    }
                }

                if (_minRequiredPasswordLength < _minRequiredNonAlphanumericCharacters)
                {
                    throw new ProviderException("Minimal required non alphanumeric characters cannot be longer than the minimum required password length.");
                }
            }

            string format = config["passwordFormat"] ?? "Hashed";

            switch (format)
            {
                case "Hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
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
            using (var ctx = CreateContext())
            {
                User user;
                if (!ValidateUser(username, oldPassword, ctx, out user))
                {
                    return false;
                }
                if (!CheckPwdComplexity(newPassword))
                {
                    return false;
                }
                user.Password = EncryptPassword(newPassword);
                ctx.SaveChanges();
            }

            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            using (var ctx = CreateContext())
            {
                User user;
                if (!ValidateUser(username, password, ctx, out user))
                {
                    return false;
                }

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
            User user = null;
            using (var ctx = CreateContext())
            {
                // check username uniqueness
                if (UserNameExist(username, ctx))
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                    return null;
                }

                // check email uniqueness
                if (RequiresUniqueEmail)
                {
                    if (UserEmailExist(email, ctx))
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
            }

            membershipUser = GetMembershipUserFromUser(user);
            return membershipUser;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var predicate = MatchApplication().ToEasyPredicate().And(u => u.Name.ToLower() == username.ToLower());
            int affectedRows = 0;
            using (var ctx = CreateContext())
            {
                //User user = GetUser(u => u.Name.ToLower() == username.ToLower(), ctx);
                //if (user == null)
                //{
                //    return false;
                //}

                // TODO delete related data : UsersInRoles, Profiles - this could be a fucking problem in the future
                affectedRows = ctx.Users.Delete(predicate.ToExpressionFunc());
            }

            return affectedRows > 0;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            List<User> users = null;

            totalRecords = 0;

            var predicate = MatchApplication().ToEasyPredicate();
            if (!string.IsNullOrEmpty(emailToMatch))
            {
                predicate = predicate.And(u => u.Email.ToLower().Contains(emailToMatch.ToLower()));
            }
            using (var ctx = CreateContext())
            {
                users = ctx.Users.Where(predicate.ToExpressionFunc())
                               .ToPagedList(pageIndex, pageSize, out totalRecords);
            }

            if (totalRecords > 0)
            {
                users.ForEach(u => membershipUsers.Add(GetMembershipUserFromUser(u)));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            List<User> users = null;

            totalRecords = 0;

            var predicate = MatchApplication().ToEasyPredicate();
            if (!string.IsNullOrEmpty(usernameToMatch))
            {
                predicate = predicate.And(u => u.Email.ToLower().Contains(usernameToMatch.ToLower()));
            }

            using (var ctx = CreateContext())
            {
                users = ctx.Users.Where(predicate.ToExpressionFunc())
                               .ToPagedList(pageIndex, pageSize, out totalRecords);
            }
            if (totalRecords > 0)
            {
                users.ForEach(u => membershipUsers.Add(GetMembershipUserFromUser(u)));
            }

            return membershipUsers;
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();
            List<User> users = null;

            totalRecords = 0;

            using (var ctx = CreateContext())
            {
                users = ctx.Users.Where(MatchApplication()).ToPagedList(pageIndex, pageSize, out totalRecords);
            }

            if (totalRecords > 0)
            {
                users.ForEach(u => membershipUsers.Add(GetMembershipUserFromUser(u)));
            }

            return membershipUsers;
        }

        public override int GetNumberOfUsersOnline()
        {
            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);

            using (var context = CreateContext())
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
            if (string.IsNullOrEmpty(answer))
            {
                throw new ArgumentNullException("answer", Resource.msg_AnswerRequiredOnGetPwd);
            }

            User user = null;
            using (var ctx = CreateContext())
            {
                user = GetUserByName(username, ctx);
            }
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
            if (String.Compare(user.PasswordAnswer, answer, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new EFMemberException(EFMembershipValidationStatus.WrongAnswer, string.Format(Resource.msg_AnswerNotMatched, username));
            }

            return DecryptPassword(user.Password);
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
            using (var ctx = CreateContext())
            {
                var user = GetUserByName(username, ctx);
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
            using (var ctx = CreateContext())
            {
                var user = GetUser(u => u.Id == userId, ctx);
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
            string userName;

            var predicate = MatchApplication().ToEasyPredicate().And(u => u.Email.ToLower() == email.ToLower());

            using (var ctx = CreateContext())
            {
                userName = ctx.Users.Where(predicate.ToExpressionFunc()).Select(u => u.Name).Single();
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
            using (var ctx = CreateContext())
            {
                var user = GetUserByName(username, ctx);
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
            using (var ctx = CreateContext())
            {
                var user = GetUserByName(userName, ctx);
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
            using (MembershipContext ctx = CreateContext())
            {
                var user = GetUser(u => u.Name.ToLower() == membershipUser.UserName.ToLower(), ctx);

                if (RequiresUniqueEmail)
                {
                    var userWithSameEmail = GetUserByEmail(membershipUser.Email, ctx);
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
            using (MembershipContext ctx = CreateContext())
            {
                User user = null;
                if (ValidateUser(username, password, ctx, out user))
                {
                    ctx.SaveChanges();
                }
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
                       && GetSpecialCharsCount(clearText) >= _minRequiredNonAlphanumericCharacters
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
        private bool ValidateUser(string username, string password, MembershipContext ctx, out User user)
        {
            user = GetUser(u => u.Name.ToLower() == username.ToLower(), ctx);
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
            return true;
        }

        private User GetUser(Expression<Func<User, bool>> query, MembershipContext context)
        {
            User user = context.Users.Where(query).Where(MatchApplication()).Single();

            return user;
        }
        private User GetUserByName(string userName, MembershipContext ctx)
        {
            return GetUser(u => u.Name.ToLower() == userName.ToLower(), ctx);
        }
        private bool UserNameExist(string userName, MembershipContext ctx)
        {
            bool exist = false;
            var predicate = MatchApplication().And(MatchUserName(userName));
            exist = ctx.Users.Count(predicate.ToExpressionFunc()) > 0;
            return exist;
        }
        private User GetUserByEmail(string email, MembershipContext ctx)
        {
            return GetUser(u => u.Email.ToLower() == email.ToLower(), ctx);
        }
        private bool UserEmailExist(string email, MembershipContext ctx)
        {
            bool exist = false;
            var predicate = MatchApplication().And(MatchEmail(email));
            exist = ctx.Users.Count(predicate.ToExpressionFunc()) > 0;
            return exist;
        }

        /// <summary>
        /// Matches the local application name.
        /// </summary>
        /// <returns>Status whether passed in user matches the application.</returns>
        private Expression<Func<User, bool>> MatchApplication()
        {
            return user => user.Application.Name.ToLower() == ApplicationName.ToLower();
        }
        private Expression<Func<User, bool>> MatchUserName(string userName)
        {
            return user => user.Name.ToLower() == userName.ToLower();
        }
        private Expression<Func<User, bool>> MatchEmail(string email)
        {
            return user => user.Email.ToLower() == email.ToLower();
        }
        private MembershipContext CreateContext()
        {
            return new MembershipContext(ConnectionString);
        }

        #endregion

    }
}
