using System;
using KitchenPC.Ingredients;

namespace KitchenPC.NLP
{
   public class AnomalousMatch : Match
   {
      readonly AnomalousResult anomaly;

      public AnomalousMatch(string input, AnomalousResult anomaly, IngredientUsage usage) : base(input, usage)
      {
         this.anomaly = anomaly;
      }

      public override string ToString()
      {
         return String.Format("[AnomalousMatch] ({0}) Usage: {1}", anomaly, usage);
      }
   }
}