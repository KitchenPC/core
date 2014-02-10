using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using KitchenPC.ShoppingLists;

namespace KitchenPC.DB.Models
{
   public class ShoppingLists
   {
      public virtual Guid ShoppingListId { get; set; }
      public virtual Guid UserId { get; set; }
      public virtual String Title { get; set; }

      public virtual IList<ShoppingListItems> Items { get; set; }

      public static ShoppingLists FromId(Guid id)
      {
         return new ShoppingLists
         {
            ShoppingListId = id
         };
      }

      public virtual ShoppingList AsShoppingList()
      {
         return new ShoppingList
         {
            Id = ShoppingListId,
            Title = Title
         };
      }
   }

   public class ShoppingListsMap : ClassMap<ShoppingLists>
   {
      public ShoppingListsMap()
      {
         Id(x => x.ShoppingListId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.UserId).Not.Nullable().Index("IDX_ShoppingLists_UserId").UniqueKey("UniqueTitle");
         Map(x => x.Title).Not.Nullable().UniqueKey("UniqueTitle");

         HasMany(x => x.Items)
            .KeyColumn("ShoppingListId")
            .Inverse()
            .Cascade.All();
      }
   }
}