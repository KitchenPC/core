using System;
using KitchenPC.Recipes;

namespace KitchenPC.Menus
{
   public struct Menu
   {
      public Guid? Id;
      public String Title;
      public RecipeBrief[] Recipes; //Can be null

      public static Menu FromId(Guid menuId)
      {
         return new Menu(menuId, null);
      }

      static readonly Menu favorites = new Menu(null, "Favorites");

      public static Menu Favorites
      {
         get
         {
            return favorites;
         }
      }

      public Menu(Guid? id, String title)
      {
         Id = id;
         Title = title;
         Recipes = null;
      }

      public Menu(Menu menu)
      {
         Id = menu.Id;
         Title = menu.Title;
         Recipes = null;

         if (menu.Recipes != null)
         {
            Recipes = new RecipeBrief[menu.Recipes.Length];
            menu.Recipes.CopyTo(Recipes, 0);
         }
      }

      public override string ToString()
      {
         var count = (Recipes != null ? Recipes.Length : 0);

         return String.Format("{0} ({1} {2}",
            Title,
            count,
            count != 1 ? "recipes" : "recipe");
      }

      public override bool Equals(object obj)
      {
         if (false == (obj is Menu))
            return false;

         var menu = (Menu) obj;
         if (this.Id.HasValue || menu.Id.HasValue)
            return this.Id.Equals(menu.Id);

         return this.Title.Equals(menu.Title);
      }

      public override int GetHashCode()
      {
         return this.Id.HasValue ? this.Id.Value.GetHashCode() : this.Title.GetHashCode();
      }
   }
}