using System;
using KitchenPC.Recipes;

namespace KitchenPC.Data.DTO
{
   public class Recipes
   {
      public Guid RecipeId { get; set; }
      public short? CookTime { get; set; }
      public string Steps { get; set; }
      public short? PrepTime { get; set; }
      public short Rating { get; set; }
      public string Description { get; set; }
      public string Title { get; set; }
      public bool Hidden { get; set; }
      public string Credit { get; set; }
      public string CreditUrl { get; set; }
      public DateTime DateEntered { get; set; }
      public short ServingSize { get; set; }
      public string ImageUrl { get; set; }

      public static RecipeBrief ToRecipeBrief(Recipes dtoRecipe)
      {
         return new RecipeBrief
         {
            Id = dtoRecipe.RecipeId,
            ImageUrl = dtoRecipe.ImageUrl,
            AvgRating = dtoRecipe.Rating,
            CookTime = dtoRecipe.CookTime,
            PrepTime = dtoRecipe.PrepTime,
            Description = dtoRecipe.Description,
            Title = dtoRecipe.Title
         };
      }
   }
}