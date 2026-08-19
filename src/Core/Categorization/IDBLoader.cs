using System.Collections.Generic;

namespace KitchenPC.Core.Categorization
{
   public interface IDBLoader
   {
      IEnumerable<IIngredientCommonality> LoadCommonIngredients();
      IEnumerable<IRecipeClassification> LoadTrainingData();
   }
}
