using System;

namespace GSA.UnliquidatedObligations.Utility
{
    public class UnexpectedSwitchValueException : Exception
    {
        public UnexpectedSwitchValueException(object o)
            : base(string.Format("Did not expect val [{0}] in the switch statement", o))
        {
        }
    }
}
