using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class Favorites
   {
      public virtual Guid FavoriteId { get; set; }
      public virtual Guid UserId { get; set; }
      public virtual Recipes Recipe { get; set; }
      public virtual Menus Menu { get; set; }
   }

   public class FavoritesMap : ClassMap<Favorites>
   {
      public FavoritesMap()
      {
         Id(x => x.FavoriteId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.UserId).Not.Nullable().Index("IDX_Favorites_UserId");

         References(x => x.Recipe).Not.Nullable().Index("IDX_Favorites_RecipeId");
         References(x => x.Menu);
      }
   }
}