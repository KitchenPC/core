namespace KitchenPC.NLP
{
   public enum MatchPrecision
   {
      None,
      Partial,
      Exact
   };

   public enum MatchResult
   {
      NoMatch,
      UnknownUnit,
      NoForm,
      UnknownForm,
      IncompatibleForm,
      PartialMatch,
      Match
   }

   public enum AnomalousResult
   {
      Fallthrough,
      AutoConvert
   }

   public enum TraceLevel
   {
      Debug,
      Error,
      Info,
      Fatal,
      Warn
   }
}