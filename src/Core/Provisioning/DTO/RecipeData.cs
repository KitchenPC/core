namespace KitchenPC.Data.DTO
{
   public class RecipeData
   {
      public Recipes Recipe { get; set; }
      public RecipeIngredients[] Ingredients { get; set; }
      public RecipeMetadata Metadata { get; set; }
   }
}