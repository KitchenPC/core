using System;

namespace KitchenPC.ShoppingLists
{
   public class ShoppingListModification
   {
      public Guid ModifiedItemId { get; private set; }
      public Amount NewAmount { get; private set; }
      public Boolean? CrossOut { get; private set; }

      public ShoppingListModification(Guid itemId, Amount newAmount, Boolean? crossout)
      {
         ModifiedItemId = itemId;
         NewAmount = newAmount;
         CrossOut = crossout;
      }
   }
}