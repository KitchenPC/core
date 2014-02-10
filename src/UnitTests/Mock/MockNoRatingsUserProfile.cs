using System;
using KitchenPC.Modeler;
using KitchenPC.Recipes;
using KitchenPC.UnitTests;

namespace KPCServer.UnitTests
{
   /// <summary>Mock UserProfile object for user with no ratings or fav ing/tags</summary>
   internal class MockNoRatingsUserProfile : IUserProfile
   {
      readonly RecipeRating[] ratings;
      readonly PantryItem[] pantry;

      public MockNoRatingsUserProfile()
      {
         ratings = new RecipeRating[] {};

         pantry = new PantryItem[]
         {
            new PantryItem() {IngredientId = ModelerTests.ING_EGGS, Amt = 6}, //6 eggs
            new PantryItem() {IngredientId = ModelerTests.ING_MILK, Amt = 16}, //16 cups of milk (1 gallon)
            new PantryItem() {IngredientId = ModelerTests.ING_FLOUR, Amt = 8}, //8oz flour
            new PantryItem() {IngredientId = ModelerTests.ING_CHEESE, Amt = 16}, //16oz cheese
            new PantryItem() {IngredientId = ModelerTests.ING_CHICKEN, Amt = 16} //16oz chicken
         };
      }

      public Guid UserId
      {
         get
         {
            return new Guid("ccb283de-c980-46a5-8fb4-1bb55398b8bb");
         }
      } //This is a unique identifier for the user, and is not used by the engine

      public RecipeRating[] Ratings
      {
         get
         {
            return ratings;
         }
      } //This is a list of all the ratings the user has made on recipes in the database.

      public PantryItem[] Pantry
      {
         get
         {
            return pantry;
         }
      } //These are the items the user has available to use, engine will use as many of these as it can

      public Guid[] FavoriteIngredients
      {
         get
         {
            return new Guid[] {};
         }
      } //Engine will tend to favor recipes with these ingredients

      public RecipeTags FavoriteTags
      {
         get
         {
            return 0;
         }
      } //Engine will tend to favor recipes with these tags

      public Guid? AvoidRecipe
      {
         get
         {
            return null;
         }
      }

      public Guid[] BlacklistedIngredients
      {
         get
         {
            return new Guid[] {ModelerTests.ING_MILK};
         }
      } //Engine will never suggest any recipe with these ingredients, no matter what.

      public RecipeTags AllowedTags //Engine will never suggest any recipe that does not contain at least one of these tags.
      {
         get
         {
            return null;
         }
      }
   }
}