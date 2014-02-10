using System;

namespace KitchenPC.Context
{
   public class IngredientNode
   {
      public Guid Id;
      public string IngredientName;
      public int Popularity;

      public IngredientNode(Guid id, string name, int popularity)
      {
         Id = id;
         IngredientName = name;
         Popularity = popularity;
      }
   }
}