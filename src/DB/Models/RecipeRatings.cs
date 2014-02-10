using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class RecipeRatings
   {
      public virtual Guid RatingId { get; set; }
      public virtual Guid UserId { get; set; }
      public virtual Recipes Recipe { get; set; }
      public virtual Int16 Rating { get; set; }
   }

   public class RecipeRatingsMap : ClassMap<RecipeRatings>
   {
      public RecipeRatingsMap()
      {
         Id(x => x.RatingId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.UserId).Not.Nullable().Index("IDX_RecipeRatings_UserId").UniqueKey("UserRating");
         Map(x => x.Rating).Not.Nullable();

         References(x => x.Recipe).Not.Nullable().Index("IDX_RecipeRatings_RecipeId").UniqueKey("UserRating");
      }
   }
}