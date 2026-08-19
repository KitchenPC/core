using System;

namespace KitchenPC.Core.Provisioning.DTO;

public class Favorites
{
   public Guid FavoriteId { get; set; }
   public Guid UserId { get; set; }
   public Guid RecipeId { get; set; }
   public Guid? MenuId { get; set; }
}
