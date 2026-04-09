using System;
using KitchenPC.Core.ShoppingLists;

namespace KitchenPC.Core.Provisioning.DTO;

public class ShoppingLists
{
   public Guid ShoppingListId { get; set; }
   public Guid UserId { get; set; }
   public String Title { get; set; }

   public static ShoppingList ToShoppingList(ShoppingLists dtoList)
   {
      return new ShoppingList { Id = dtoList.ShoppingListId, Title = dtoList.Title };
   }
}
