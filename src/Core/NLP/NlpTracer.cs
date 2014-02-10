namespace KitchenPC.NLP
{
   public static class NlpTracer
   {
      static ITracer currentTracer;

      public static void SetTracer(ITracer tracer)
      {
         currentTracer = tracer; //BUGBUG: Not thread safe, but probably not tracing on the main site
      }

      public static void Trace(TraceLevel level, string message, params object[] args)
      {
         if (currentTracer == null) //No op
         {
            return;
         }

         currentTracer.Trace(level, message, args);
      }

      public static void ConditionalTrace(bool condition, TraceLevel level, string message, params object[] args)
      {
         if (condition)
         {
            Trace(level, message, args);
         }
      }
   }
}