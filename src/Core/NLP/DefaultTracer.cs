using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KitchenPC.Core.NLP;

/// <summary>Implementation of ITracer that uses Microsoft.Extensions.Logging.</summary>
public class DefaultTracer : ITracer
{
   private readonly ILogger log;

   public DefaultTracer() : this(NullLoggerFactory.Instance) { }

   public DefaultTracer(ILoggerFactory loggerFactory) =>
      log = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)))
         .CreateLogger<Parser>();

   public void Trace(TraceLevel level, string message, params object[] args)
   {
      var formattedMessage = string.Format(CultureInfo.InvariantCulture, message, args);
      log.Log(MapLevel(level), "{Message}", formattedMessage);
   }

   private static LogLevel MapLevel(TraceLevel level) =>
      level switch
      {
         TraceLevel.Debug => LogLevel.Debug,
         TraceLevel.Error => LogLevel.Error,
         TraceLevel.Fatal => LogLevel.Critical,
         TraceLevel.Info => LogLevel.Information,
         TraceLevel.Warn => LogLevel.Warning,
         _ => LogLevel.None,
      };
}
