using System;

namespace KitchenPC.NLP
{
   public class AnomalousIngredientNode : IngredientNode
   {
      public AnomalousIngredientNode(Guid id, string name, UnitType convtype, Weight unitweight, DefaultPairings pairings) : base(id, name, convtype, unitweight, pairings)
      {
      }
   }
}