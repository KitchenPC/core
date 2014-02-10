using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class QueuedRecipes
   {
      public virtual Guid QueueId { get; set; }
      public virtual Guid UserId { get; set; }
      public virtual Recipes Recipe { get; set; }
      public virtual DateTime QueuedDate { get; set; }
   }

   public class QueuedRecipesMap : ClassMap<QueuedRecipes>
   {
      public QueuedRecipesMap()
      {
         Id(x => x.QueueId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.UserId).Not.Nullable().Index("IDX_QueuedRecipes_UserId").UniqueKey("UniqueRecipe");
         Map(x => x.QueuedDate).Not.Nullable();

         References(x => x.Recipe).Not.Nullable().UniqueKey("UniqueRecipe");
      }
   }
}