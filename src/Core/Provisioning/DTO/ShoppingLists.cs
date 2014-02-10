using System;
using KitchenPC.ShoppingLists;

namespace KitchenPC.Data.DTO
{
   public class ShoppingLists
   {
      public Guid ShoppingListId { get; set; }
      public Guid UserId { get; set; }
      public String Title { get; set; }

      public static ShoppingList ToShoppingList(ShoppingLists dtoList)
      {
         return new ShoppingList
         {
            Id = dtoList.ShoppingListId,
            Title = dtoList.Title
         };
      }
   }
}