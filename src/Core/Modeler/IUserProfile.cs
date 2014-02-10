using System;
using KitchenPC.Recipes;

namespace KitchenPC.Modeler
{
   /// <summary>
   /// Interface used to transfer user specific runtime properties into the modeler.  KPC will be implementation that builds
   /// this information from the database, and unit tests will create static instances for testing.
   /// </summary>
   public interface IUserProfile
   {
      Guid UserId { get; } //DB User ID
      RecipeRating[] Ratings { get; } //Every recipe ID user has rated with the rating
      PantryItem[] Pantry { get; } //Set of ingredients user has available with amounts
      Guid[] FavoriteIngredients { get; } //Ingredients to favor
      RecipeTags FavoriteTags { get; } //Tags to favor
      Guid[] BlacklistedIngredients { get; } //Ingredients to always avoid
      Guid? AvoidRecipe { get; }
      RecipeTags AllowedTags { get; } //Only allow recipes that contain at least one of these tags (null to allow all)
   }
}