namespace KitchenPC.Core.Recipes;

public class ReadRecipeOptions
{
    public bool ReturnCommentCount { get; set; }
    public bool ReturnUserRating { get; set; }
    public bool ReturnCookbookStatus { get; set; }
    public bool ReturnMethod { get; set; }
    public bool ReturnPermalink { get; set; }

    public static ReadRecipeOptions None { get; } = new ReadRecipeOptions();

    public static ReadRecipeOptions MethodOnly { get; } = new ReadRecipeOptions { ReturnMethod = true };
}
