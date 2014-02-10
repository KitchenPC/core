using System;
using KitchenPC.Recipes;

namespace KitchenPC.Modeler
{
   public class UserProfile : IUserProfile
   {
      static IUserProfile anonymous;

      /// <summary>
      /// Represents a modeling profile that has no user context, such as a saved pantry, favorite ingredients, blacklists, etc.
      /// </summary>
      public static IUserProfile Anonymous
      {
         get
         {
            if (anonymous == null)
            {
               anonymous = new UserProfile
               {
                  UserId = Guid.Empty,
                  Ratings = new RecipeRating[0],
                  FavoriteIngredients = new Guid[0],
                  FavoriteTags = RecipeTags.None,
                  BlacklistedIngredients = new Guid[0]
               };
            }

            return anonymous;
         }
      }

      public Guid UserId { get; set; }
      public RecipeRating[] Ratings { get; set; }
      public PantryItem[] Pantry { get; set; }
      public Guid[] FavoriteIngredients { get; set; }
      public RecipeTags FavoriteTags { get; set; }
      public Guid[] BlacklistedIngredients { get; set; }
      public Guid? AvoidRecipe { get; set; }
      public RecipeTags AllowedTags { get; set; }
   }
}