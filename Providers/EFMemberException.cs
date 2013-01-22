using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScottyApps.EFCodeFirstProviders.Providers
{
    public class EFMemberException : Exception
    {
        public EFMembershipValidationStatus Status { get; set; }
        public EFMemberException(EFMembershipValidationStatus status, string message)
            : base(message)
        {
            this.Status = status;
        }
    }

    public enum EFMembershipValidationStatus
    {
        UserNotExist = 0,
        WrongPassword,
        WrongAnswer,
        UserNotConfirmed,
        UserIsLockedOut
    }
}
