using System;
using Microsoft.AspNetCore.Mvc;

namespace GSA.UnliquidatedObligations.Web.Models
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel
    {
        public string SprintName { get; set; }
        public string RequestId { get; set; }
        public string Path { get; set; }
        public Type ExceptionType { get; set; }
        public string ExceptionCode { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionStack { get; set; }
    }
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
