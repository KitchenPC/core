using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using KitchenPC.Core.Menus;

namespace KitchenPC.DB.Models;

public class Menus
{
   public virtual Guid MenuId { get; set; }
   public virtual Guid UserId { get; set; }
   public virtual String Title { get; set; }
   public virtual DateTime CreatedDate { get; set; }

   public virtual IList<Favorites> Recipes { get; set; }

   public static Menus FromId(Guid id) => new() { MenuId = id };

   public virtual Menu AsMenu() => new() { Id = MenuId, Title = Title };

   public static bool operator !=(Menu menu, Menus dbMenu) => !(menu == dbMenu);

   public static bool operator ==(Menu menu, Menus dbMenu)
   {
      if (dbMenu == null)
      {
         return !menu.Id.HasValue;
      }

      return menu.Id == dbMenu.MenuId;
   }

   public override bool Equals(object obj)
   {
      if (false == (obj is Menus))
         return false;

      var menu = (Menus)obj;
      return MenuId.Equals(menu.MenuId);
   }

   public override int GetHashCode() => MenuId.GetHashCode();
}

public class MenusMap : ClassMap<Menus>
{
   public MenusMap()
   {
      Id(x => x.MenuId).GeneratedBy.GuidComb().UnsavedValue(Guid.Empty);

      Map(x => x.UserId).Not.Nullable().Index("IDX_Menus_UserId").UniqueKey("UserTitle");
      Map(x => x.Title).Not.Nullable().UniqueKey("UserTitle");
      Map(x => x.CreatedDate).Not.Nullable();

      HasMany(x => x.Recipes).KeyColumn("MenuId").Cascade.Delete(); // If Menu is deleted, delete all the Favorites that reference this menu
   }
}
