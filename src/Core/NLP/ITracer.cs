namespace KitchenPC.NLP
{
   public interface ITracer
   {
      void Trace(TraceLevel level, string message, params object[] args);
   }
}