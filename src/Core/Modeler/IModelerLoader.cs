using System.Collections.Generic;

namespace KitchenPC.Modeler
{
   public interface IModelerLoader
   {
      IEnumerable<RecipeBinding> LoadRecipeGraph();
      IEnumerable<IngredientBinding> LoadIngredientGraph();
      IEnumerable<RatingBinding> LoadRatingGraph();
   }
}