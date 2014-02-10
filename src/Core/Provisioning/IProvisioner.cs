using System.Collections.Generic;
using KitchenPC.Data.DTO;

namespace KitchenPC.Data
{
   public interface IProvisioner
   {
      // Core Data (Immutable)
      IngredientForms[] IngredientForms();
      IngredientMetadata[] IngredientMetadata();
      DTO.Ingredients[] Ingredients();
      NlpAnomalousIngredients[] NlpAnomalousIngredients();
      NlpDefaultPairings[] NlpDefaultPairings();
      NlpFormSynonyms[] NlpFormSynonyms();
      NlpIngredientSynonyms[] NlpIngredientSynonyms();
      NlpPrepNotes[] NlpPrepNotes();
      NlpUnitSynonyms[] NlpUnitSynonyms();

      // Recipe Data
      List<DTO.Recipes> Recipes();
      List<RecipeMetadata> RecipeMetadata();
      List<RecipeIngredients> RecipeIngredients();

      // User Data
      List<Favorites> Favorites();
      List<DTO.Menus> Menus();
      List<QueuedRecipes> QueuedRecipes();
      List<RecipeRatings> RecipeRatings();
      List<DTO.ShoppingLists> ShoppingLists();
      List<ShoppingListItems> ShoppingListItems();
   }
}