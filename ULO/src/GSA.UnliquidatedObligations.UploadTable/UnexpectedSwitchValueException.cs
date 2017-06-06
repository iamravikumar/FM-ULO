using System;

namespace GSA.UnliquidatedObligations.UploadTable
{
    public class UnexpectedSwitchValueException : Exception
    {
        public UnexpectedSwitchValueException(object o)
            : base(string.Format("Did not expect val [{0}] in the switch statement", o))
        {
        }
    }
}
