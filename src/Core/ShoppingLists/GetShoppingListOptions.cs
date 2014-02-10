namespace KitchenPC.ShoppingLists
{
   public class GetShoppingListOptions
   {
      public bool LoadItems;

      static readonly GetShoppingListOptions none = new GetShoppingListOptions();
      static readonly GetShoppingListOptions loaded = new GetShoppingListOptions {LoadItems = true};

      public static GetShoppingListOptions None
      {
         get
         {
            return none;
         }
      }

      public static GetShoppingListOptions WithItems
      {
         get
         {
            return loaded;
         }
      }
   }
}