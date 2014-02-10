using System;

namespace KitchenPC.NLP
{
   public class NoMatch : Result
   {
      readonly MatchResult status;

      public override MatchResult Status
      {
         get
         {
            return status;
         }
      }

      public NoMatch(string input, MatchResult status) : base(input)
      {
         this.status = status;
      }

      public override string ToString()
      {
         return String.Format("[NoMatch] Error: {0}", status);
      }
   }
}