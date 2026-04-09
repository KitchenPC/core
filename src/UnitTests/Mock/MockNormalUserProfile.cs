using System;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.Recipes;

namespace KitchenPC.UnitTests.Mock;

/// <summary>Mock UserProfile object for test modeling</summary>
internal class MockNormalUserProfile : IUserProfile
{
   public MockNormalUserProfile()
   {
      Ratings = new RecipeRating[]
      {
         new() { RecipeId = new Guid("b11a64a9-95b3-402f-8b82-312bad539d4e"), Rating = 5 },
         new() { RecipeId = new Guid("eb16bb12-6fab-4674-a6c0-11a57878087e"), Rating = 5 },
         new() { RecipeId = new Guid("0fc6c435-d9d1-4d21-a60b-42e3389b60a1"), Rating = 5 },
         new() { RecipeId = new Guid("748fd7a4-fc35-4ee7-a4b5-2c9c5125a25c"), Rating = 4 },
         new() { RecipeId = new Guid("7046fe97-46d8-4506-aa97-debc7dc7febb"), Rating = 4 },
      };

      Pantry = new PantryItem[]
      {
         new() { IngredientId = ModelerTests.ING_EGGS, Amt = 6 }, //6 eggs
         new() { IngredientId = ModelerTests.ING_MILK, Amt = 16 }, //16 cups of milk (1 gallon)
         new() { IngredientId = ModelerTests.ING_FLOUR, Amt = 8 }, //8oz flour
         new() { IngredientId = ModelerTests.ING_CHEESE, Amt = 16 }, //16oz cheese
         new() { IngredientId = ModelerTests.ING_CHICKEN, Amt = 16 }, //16oz chicken
      };
   }

   public Guid UserId => new Guid("bcb283de-c980-46a5-8fb4-1bb55398b8bb"); //This is a unique identifier for the user, and is not used by the engine

   public RecipeRating[] Ratings { get; }

   public PantryItem[] Pantry { get; }

   public Guid[] FavoriteIngredients => new[] { ModelerTests.ING_CHICKEN, ModelerTests.ING_CHEESE }; //Engine will tend to favor recipes with these ingredients

   public RecipeTags FavoriteTags => RecipeTag.Dinner | RecipeTag.Easy; //Engine will tend to favor recipes with these tags

   public Guid? AvoidRecipe => null;

   public Guid[] BlacklistedIngredients => new[] { ModelerTests.ING_MILK }; //Engine will never suggest any recipe with these ingredients, no matter what.

   public RecipeTags AllowedTags => (RecipeTag.Dinner | RecipeTag.Dessert | RecipeTag.NoMeat); //Engine will never suggest any recipe that does not contain at least one of these tags.
}
