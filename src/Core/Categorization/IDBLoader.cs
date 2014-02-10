using System.Collections.Generic;

namespace KitchenPC.Categorization
{
   public interface IDBLoader
   {
      IEnumerable<IIngredientCommonality> LoadCommonIngredients();
      IEnumerable<IRecipeClassification> LoadTrainingData();
   }
}