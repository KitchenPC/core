using System;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Recipes;

namespace KitchenPC.Core.ShoppingLists;

public class ShoppingListUpdateCommand
{
   public ShoppingListUpdateType Command { get; set; }

   public Recipe NewRecipe { get; set; }
   public Ingredient NewIngredient { get; set; }
   public IngredientUsage NewUsage { get; set; }
   public String NewRaw { get; set; }

   public Guid? RemoveItem { get; set; }
   public ShoppingListModification ModifyItem { get; set; }
}
