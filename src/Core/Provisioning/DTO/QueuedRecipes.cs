using System;

namespace KitchenPC.Data.DTO
{
   public class QueuedRecipes
   {
      public Guid QueueId { get; set; }
      public Guid UserId { get; set; }
      public Guid RecipeId { get; set; }
      public DateTime QueuedDate { get; set; }
   }
}