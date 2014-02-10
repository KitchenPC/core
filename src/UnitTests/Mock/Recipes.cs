using System;
using KitchenPC.Ingredients;
using KitchenPC.Recipes;

namespace KitchenPC.UnitTests.Mock
{
   internal static class Recipes
   {
      public static Recipe MockRecipe(string title, string desc, RecipeTags tags = null)
      {
         var ret = new Recipe(Guid.NewGuid(), title, desc, null);

         ret.Method = "This is a mock recipe.";
         ret.OwnerAlias = "Fake Owner";
         ret.OwnerId = Guid.NewGuid();
         ret.Permalink = "http://www.kitchenpc.com/123";
         ret.ServingSize = 5;
         ret.Tags = tags;

         return ret;
      }

      public static Recipe BEST_BROWNIES
      {
         get
         {
            var r = new Recipe(new Guid("b11a64a9-95b3-402f-8b82-312bad539d4e"), "Best Brownies", "from scratch!", "");

            r.Tags = RecipeTag.NoMeat | RecipeTag.NoPork | RecipeTag.NoRedMeat | RecipeTag.Dessert;
            r.AvgRating = 5;
            r.CookTime = 40;
            r.PrepTime = 15;
            r.ServingSize = 24;
            r.AddIngredients(new IngredientUsage[]
            {
               new IngredientUsage(Ingredients.MARGARINE, Forms.MARGARINE_VOLUME, new Amount(1, Units.Cup), "in chunks"),
               new IngredientUsage(Ingredients.UNSWEETENED_BAKING_CHOCOLATE_SQUARES, Forms.UNSWEETENED_BAKING_CHOCOLATE_SQUARES_WEIGHT, new Amount(1, Units.Ounce), ""),
               new IngredientUsage(Ingredients.GRANULATED_SUGAR, Forms.GRANULATED_SUGAR_VOLUME, new Amount(2.66667f, Units.Cup), ""),
               new IngredientUsage(Ingredients.EGGS, Forms.EGGS_UNIT, new Amount(4, Units.Unit), "large"),
               new IngredientUsage(Ingredients.VANILLA_EXTRACT, Forms.VANILLA_EXTRACT_VOLUME, new Amount(2, Units.Teaspoon), ""),
               new IngredientUsage(Ingredients.ALL_PURPOSE_FLOUR, Forms.ALL_PURPOSE_FLOUR_SIFTED, new Amount(1, Units.Cup), "")
            });

            return r;
         }
      }
   }
}