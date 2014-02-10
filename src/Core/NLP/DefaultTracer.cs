using log4net;

namespace KitchenPC.NLP
{
   /// <summary>Implementation of ITracer that uses Log4Net</summary>
   public class DefaultTracer : ITracer
   {
      readonly ILog log;

      public DefaultTracer()
      {
         log = LogManager.GetLogger(typeof (Parser));
         log.Info("Initialized logger for new NLP parser.");
      }

      public void Trace(TraceLevel level, string message, params object[] args)
      {
         switch (level)
         {
            case TraceLevel.Debug:
               log.DebugFormat(message, args);
               break;
            case TraceLevel.Error:
               log.ErrorFormat(message, args);
               break;
            case TraceLevel.Fatal:
               log.FatalFormat(message, args);
               break;
            case TraceLevel.Info:
               log.InfoFormat(message, args);
               break;
            case TraceLevel.Warn:
               log.WarnFormat(message, args);
               break;
         }
      }
   }
}