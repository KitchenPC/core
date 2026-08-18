using System.Collections.Generic;
using KitchenPC.Core.Modeler;

namespace KitchenPC.Core.Context;

public class DBModelerLoader : IModelerLoader
{
   private readonly IDBAdapter adapter;
   private IEnumerable<RecipeBinding> recipedata;
   private IEnumerable<IngredientBinding> ingredientdata;
   private IEnumerable<RatingBinding> ratingdata;

   public DBModelerLoader(IDBAdapter adapter)
   {
      this.adapter = adapter;
   }

   public IEnumerable<RecipeBinding> LoadRecipeGraph()
   {
      if (recipedata == null)
         recipedata = adapter.LoadRecipeGraph();

      return recipedata;
   }

   public IEnumerable<IngredientBinding> LoadIngredientGraph()
   {
      if (ingredientdata == null)
         ingredientdata = adapter.LoadIngredientGraph();

      return ingredientdata;
   }

   public IEnumerable<RatingBinding> LoadRatingGraph()
   {
      if (ratingdata == null)
         ratingdata = adapter.LoadRatingGraph();

      return ratingdata;
   }
}
