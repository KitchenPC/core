using System;
using KitchenPC.Ingredients;

namespace KitchenPC.NLP
{
   public class Match : Result
   {
      protected IngredientUsage usage;

      public override IngredientUsage Usage
      {
         get
         {
            return usage;
         }
      }

      public override MatchResult Status
      {
         get
         {
            return MatchResult.Match;
         }
      }

      public Match(string input, IngredientUsage usage) : base(input)
      {
         this.usage = usage;
      }

      public override string ToString()
      {
         return String.Format("[Match] Usage: {0}", usage);
      }
   }
}