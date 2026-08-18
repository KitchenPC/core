namespace KitchenPC.Core.ShoppingLists;

public class GetShoppingListOptions
{
   public bool LoadItems;

   private static readonly GetShoppingListOptions none = new GetShoppingListOptions();
   private static readonly GetShoppingListOptions loaded = new GetShoppingListOptions
   {
      LoadItems = true,
   };

   public static GetShoppingListOptions None
   {
      get { return none; }
   }

   public static GetShoppingListOptions WithItems
   {
      get { return loaded; }
   }
}
