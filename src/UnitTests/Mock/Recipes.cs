using System;
using KitchenPC.Core;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Recipes;

namespace KitchenPC.UnitTests.Mock;

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
         var r = new Recipe(
            new Guid("b11a64a9-95b3-402f-8b82-312bad539d4e"),
            "Best Brownies",
            "from scratch!",
            ""
         )
         {
            Tags = RecipeTag.NoMeat | RecipeTag.NoPork | RecipeTag.NoRedMeat | RecipeTag.Dessert,
            AvgRating = 5,
            CookTime = 40,
            PrepTime = 15,
            ServingSize = 24,
         };

         r.AddIngredients(
            new IngredientUsage[]
            {
               new(
                  Ingredients.MARGARINE,
                  Forms.MARGARINE_VOLUME,
                  new Amount(1, Units.Cup),
                  "in chunks"
               ),
               new(
                  Ingredients.UNSWEETENED_BAKING_CHOCOLATE_SQUARES,
                  Forms.UNSWEETENED_BAKING_CHOCOLATE_SQUARES_WEIGHT,
                  new Amount(1, Units.Ounce),
                  ""
               ),
               new(
                  Ingredients.GRANULATED_SUGAR,
                  Forms.GRANULATED_SUGAR_VOLUME,
                  new Amount(2.66667f, Units.Cup),
                  ""
               ),
               new(Ingredients.EGGS, Forms.EGGS_UNIT, new Amount(4, Units.Unit), "large"),
               new(
                  Ingredients.VANILLA_EXTRACT,
                  Forms.VANILLA_EXTRACT_VOLUME,
                  new Amount(2, Units.Teaspoon),
                  ""
               ),
               new(
                  Ingredients.ALL_PURPOSE_FLOUR,
                  Forms.ALL_PURPOSE_FLOUR_SIFTED,
                  new Amount(1, Units.Cup),
                  ""
               ),
            }
         );

         return r;
      }
   }
}
