using System;

namespace KitchenPC.Data.DTO
{
   public class RecipeRatings
   {
      public Guid RatingId { get; set; }
      public Guid UserId { get; set; }
      public Guid RecipeId { get; set; }
      public Int16 Rating { get; set; }
   }
}