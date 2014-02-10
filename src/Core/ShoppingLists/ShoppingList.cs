using System;
using System.Collections;
using System.Collections.Generic;

namespace KitchenPC.ShoppingLists
{
   public class ShoppingList : IEnumerable<ShoppingListItem>
   {
      public static Guid GUID_WATER = new Guid("cb44df2d-f27c-442a-bd6e-fd7bdd501f10");
      public Guid? Id { get; set; }
      public string Title { get; set; }
      readonly List<ShoppingListItem> list;

      static readonly ShoppingList defaultList = new ShoppingList(null, "");

      public static ShoppingList Default
      {
         get
         {
            return defaultList;
         }
      }

      public static ShoppingList FromId(Guid menuId)
      {
         return new ShoppingList(menuId, null);
      }

      public ShoppingList()
      {
         list = new List<ShoppingListItem>();
      }

      public ShoppingList(Guid? id, String title) : this()
      {
         this.Id = id;
         this.Title = title;
      }

      public ShoppingList(Guid? id, String title, IEnumerable<IShoppingListSource> items) : this(id, title)
      {
         AddItems(items);
      }

      public void AddItems(IEnumerable<IShoppingListSource> items)
      {
         foreach (var item in items)
         {
            AddItem(item.GetItem());
         }
      }

      void AddItem(ShoppingListItem item)
      {
         var existingItem = list.Find(i => i.Equals(item));
         if (existingItem == null)
         {
            list.Add(item);
            return;
         }

         existingItem.CrossedOut = item.CrossedOut; // If new item is crossed out, cross out existing item

         if (existingItem.Ingredient == null || existingItem.Amount == null) // Adding same ingredient twice, but nothing to aggregate.  Skip.
            return;

         if (item.Amount == null) // Clear out existing amount
         {
            existingItem.Amount = null;
            return;
         }

         //increment existing amount
         existingItem.Amount += item.Amount;
      }

      public override string ToString()
      {
         var title = (!String.IsNullOrEmpty(Title) ? Title : "Default List");
         var count = list.Count;
         return String.Format("{0} ({1} Item{2})", title, count, (count != 1 ? "s" : ""));
      }

      public IEnumerator<ShoppingListItem> GetEnumerator()
      {
         return list.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return list.GetEnumerator();
      }
   }
}