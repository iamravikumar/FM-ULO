using System;

namespace GSA.UnliquidatedObligations.Utility
{
    public class NotNowException : Exception
    {
        #region Constructors

        public NotNowException()
        {
        }

        public NotNowException(string message) : base(message)
        {
        }

        public NotNowException(string message, Exception inner) : base(message, inner)
        {
        }

        #endregion
    }

    public class ReadOnlyException : NotNowException
    {
        #region Constructors

        public ReadOnlyException()
        {
        }

        public ReadOnlyException(string message) : base(message)
        {
        }

        public ReadOnlyException(string message, Exception inner) : base(message, inner)
        {
        }

        #endregion
    }
}
