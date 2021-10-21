using System;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.Recipes;

namespace KitchenPC.UnitTests.Mock;

/// <summary>Mock UserProfile object for user with no ratings or fav ing/tags</summary>
internal class MockNoRatingsUserProfile : IUserProfile
{
   public MockNoRatingsUserProfile()
   {
      Ratings = new RecipeRating[] {};

      Pantry = new PantryItem[]
      {
         new() {IngredientId = ModelerTests.ING_EGGS, Amt = 6}, //6 eggs
         new() {IngredientId = ModelerTests.ING_MILK, Amt = 16}, //16 cups of milk (1 gallon)
         new() {IngredientId = ModelerTests.ING_FLOUR, Amt = 8}, //8oz flour
         new() {IngredientId = ModelerTests.ING_CHEESE, Amt = 16}, //16oz cheese
         new() {IngredientId = ModelerTests.ING_CHICKEN, Amt = 16} //16oz chicken
      };
   }

   public Guid UserId => new Guid("ccb283de-c980-46a5-8fb4-1bb55398b8bb"); //This is a unique identifier for the user, and is not used by the engine

   public RecipeRating[] Ratings { get; }

   public PantryItem[] Pantry { get; }

   public Guid[] FavoriteIngredients => new Guid[] {}; //Engine will tend to favor recipes with these ingredients

   public RecipeTags FavoriteTags => 0; //Engine will tend to favor recipes with these tags

   public Guid? AvoidRecipe => null;

   public Guid[] BlacklistedIngredients => new[] {ModelerTests.ING_MILK}; //Engine will never suggest any recipe with these ingredients, no matter what.

   public RecipeTags AllowedTags => null; //Engine will never suggest any recipe that does not contain at least one of these tags.
}