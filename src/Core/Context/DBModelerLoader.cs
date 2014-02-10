using System.Collections.Generic;
using KitchenPC.Modeler;

namespace KitchenPC.Context
{
   public class DBModelerLoader : IModelerLoader
   {
      readonly IDBAdapter adapter;
      IEnumerable<RecipeBinding> recipedata;
      IEnumerable<IngredientBinding> ingredientdata;
      IEnumerable<RatingBinding> ratingdata;

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
}