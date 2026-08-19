namespace KitchenPC.Core.Recipes;

public class ReadRecipeOptions
{
   public bool ReturnUserRating { get; set; }
   public bool ReturnMenuCount { get; set; }
   public bool ReturnMethod { get; set; }

   public static ReadRecipeOptions None { get; } = new ReadRecipeOptions();

   public static ReadRecipeOptions MethodOnly { get; } =
      new ReadRecipeOptions { ReturnMethod = true };
}
