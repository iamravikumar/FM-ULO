using System;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web
{
    public enum UloExceptionCodes
    {
        Unknown,
        CantAccessExceptionHandlerPathFeature,
        NoEmailTemplate,
        ExternalLoginCouldNotUnprotectTheTicket,
    }

    public class UloException : CodedException<UloExceptionCodes>
    {
        public UloException(UloExceptionCodes code, string message = null)
            : base(code, message)
        { }
        public UloException(UloExceptionCodes code, Exception ex)
            : base(code, ex)
        { }
    }
}
