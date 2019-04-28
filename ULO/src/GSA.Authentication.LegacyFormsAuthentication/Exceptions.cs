using System;

namespace GSA.Authentication.LegacyFormsAuthentication
{
    public class UnexpectedSwitchValueException : Exception
    {
        public UnexpectedSwitchValueException(object o)
            : base(string.Format("Did not expect val [{0}] in the switch statement", o))
        {
        }
    }
}
