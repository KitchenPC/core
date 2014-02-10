using System;

namespace KitchenPC.Categorization
{
   public interface IIngredientCommonality
   {
      Guid IngredientId { get; }
      Single Commonality { get; }
   }
}