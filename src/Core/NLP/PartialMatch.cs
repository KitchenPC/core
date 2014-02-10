using System;
using KitchenPC.Ingredients;

namespace KitchenPC.NLP
{
   public class PartialMatch : Match
   {
      public override MatchResult Status
      {
         get
         {
            return MatchResult.PartialMatch;
         }
      }

      public PartialMatch(string input, Ingredient ingredient, string prep) : base(input, null)
      {
         this.usage = new IngredientUsage(ingredient, null, null, prep);
      }

      public override string ToString()
      {
         if (String.IsNullOrEmpty(usage.PrepNote))
            return String.Format("[PartialMatch] Ingredient: {0}", usage.Ingredient.Name);
         else
            return String.Format("[PartialMatch] Ingredient: {0} ({1})", usage.Ingredient.Name, usage.PrepNote);
      }
   }
}