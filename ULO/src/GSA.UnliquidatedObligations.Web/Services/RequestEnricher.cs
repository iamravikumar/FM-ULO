﻿using GSA.UnliquidatedObligations.BusinessLayer;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Web;

namespace GSA.UnliquidatedObligations.Web.Services
{
    public class RequestEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var p = propertyFactory.CreateProperty("EventTimeUtc", DateTime.UtcNow.ToRfc8601());
            logEvent.AddPropertyIfAbsent(p);
            var ctx = HttpContext.Current;
            if (ctx == null) return;
            p = propertyFactory.CreateProperty("CorrelationId", ctx.CorrelationId());
            logEvent.AddPropertyIfAbsent(p);
            p = propertyFactory.CreateProperty("RequestorUserName", ctx.User?.Identity?.Name);
            logEvent.AddPropertyIfAbsent(p);
        }
    }
}