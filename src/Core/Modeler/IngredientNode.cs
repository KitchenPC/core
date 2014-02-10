using System;
using System.Collections.Generic;
using KitchenPC.Recipes;

namespace KitchenPC.Modeler
{
   public sealed class IngredientNode
   {
      public static int NextKey = 0;

      public Int32 Key; //Interally, ingredients will have numeric keys for faster hashing
      public Guid IngredientId; //KPC Shopping Ingredient ID
      public UnitType ConvType; //Conversion type for this ingredient
      public IEnumerable<RecipeNode>[] RecipesByTag; //Recipes that use this ingredient (does not include Hidden recipes)
      public RecipeTags AvailableTags; //Which indices in RecipesByTag are not null

      public IngredientNode()
      {
         this.Key = NextKey++;
      }

      public override int GetHashCode()
      {
         return Key;
      }
   }
}