namespace KitchenPC.Recipes
{
   public class ReadRecipeOptions
   {
      public bool ReturnCommentCount;
      public bool ReturnUserRating;
      public bool ReturnCookbookStatus;
      public bool ReturnMethod;
      public bool ReturnPermalink;

      static readonly ReadRecipeOptions none = new ReadRecipeOptions();
      static readonly ReadRecipeOptions methodonly = new ReadRecipeOptions {ReturnMethod = true};

      public static ReadRecipeOptions None
      {
         get
         {
            return none;
         }
      }

      public static ReadRecipeOptions MethodOnly
      {
         get
         {
            return methodonly;
         }
      }
   }
}