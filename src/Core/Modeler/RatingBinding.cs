using System;

namespace KitchenPC.Core.Modeler;

public struct RatingBinding
{
   public Guid UserId { get; set; }
   public Guid RecipeId { get; set; }
   public Int16 Rating { get; set; }
}
