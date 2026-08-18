namespace KitchenPC.Core.Menus;

public class GetMenuOptions
{
   public static GetMenuOptions None { get; } = new GetMenuOptions();
   public static GetMenuOptions WithRecipes { get; } = new GetMenuOptions { LoadRecipes = true };

   public bool LoadRecipes { get; set; }
}
