namespace GSA.OpenItems.Web
{
    using System;

    public class AppError : ApplicationException
    {
        public AppError() { }
        public AppError(string message) : base(message) { }
    }
}