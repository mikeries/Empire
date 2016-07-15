using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireUWP
{
    class LoginException : Exception
    {
        internal enum LoginExceptions : int
        {
            UnknownUser,
            InvalidUsername,
            IncorrectPassword,
            PasswordsDoNotMatch,
        }

        public LoginException(string message, LoginExceptions errorCode) : base(message)
        {
            this.HResult = (int)errorCode;
        }
    }
}
