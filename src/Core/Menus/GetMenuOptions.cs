namespace KitchenPC.Menus
{
   public class GetMenuOptions
   {
      public bool LoadRecipes;

      static readonly GetMenuOptions none = new GetMenuOptions();
      static readonly GetMenuOptions loaded = new GetMenuOptions {LoadRecipes = true};

      public static GetMenuOptions None
      {
         get
         {
            return none;
         }
      }

      public static GetMenuOptions WithRecipes
      {
         get
         {
            return loaded;
         }
      }
   }
}