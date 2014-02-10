using System;
using System.Collections.Generic;
using KitchenPC.Recipes;

namespace KitchenPC.Modeler
{
   public sealed class RecipeNode
   {
      static int nextkey = 0;

      public Int32 Key; //Internally, recipes will have numeric keys for faster hashing
      public Guid RecipeId;
      public IEnumerable<IngredientUsage> Ingredients;
      public RecipeTags Tags; //Tags from DB
      public Byte Rating; //Public rating from DB
      public Boolean Hidden; //Recipe is hidden (won't be picked at random)
      public RecipeNode[] Suggestions; //Users who like this recipe might also like these recipes (in order of weight)

      public RecipeNode()
      {
         this.Key = nextkey++;
      }

      public override int GetHashCode()
      {
         return Key;
      }
   }
}