using System;
using KitchenPC.Ingredients;

namespace KitchenPC.UnitTests.Mock
{
   internal static class Ingredients
   {
      public static Ingredient SALT = new Ingredient(new Guid("a6b0179f-6bf5-4ec4-b12b-a95f2f94fe91"), "salt", new IngredientMetadata(false, false, false, false, false, 0, 0, 0f, 0f, 0f, 38758f, 0f));
      public static Ingredient GRANULATED_SUGAR = new Ingredient(new Guid("4de12110-87b1-4198-a9a9-2b32e45df0f0"), "granulated sugar", new IngredientMetadata(false, false, false, false, true, 0, 4, 0f, 99.8f, 387f, 1f, 99.98f));
      public static Ingredient EGGS = new Ingredient(new Guid("948aeda5-ffff-41bd-af4e-71d1c740db76"), "eggs", new IngredientMetadata(false, false, false, false, true, 0, 0, 9.51f, 0.37f, 143f, 142f, 0.72f));
      public static Ingredient ALL_PURPOSE_FLOUR = new Ingredient(new Guid("daa8fbf6-3347-41b9-826d-078cd321402e"), "all-purpose flour", new IngredientMetadata(true, false, false, false, false, 0, 0, 0.98f, 0.27f, 364f, 2f, 76.31f));
      public static Ingredient SALTED_BUTTER = new Ingredient(new Guid("ac48a403-b5d7-42f1-bafb-aed0c597f139"), "salted butter", new IngredientMetadata(false, false, false, false, true, 0, 0, 81.11f, 0.06f, 717f, 714f, 0.06f));
      public static Ingredient VANILLA_EXTRACT = new Ingredient(new Guid("6a51bee2-8a40-4c7e-bf91-c7a29e980ac6"), "vanilla extract", new IngredientMetadata(false, false, false, false, false, 0, 0, 0.06f, 12.65f, 288f, 9f, 12.65f));
      public static Ingredient WATER = new Ingredient(new Guid("cb44df2d-f27c-442a-bd6e-fd7bdd501f10"), "water", new IngredientMetadata(false, false, false, false, false, 0, 0, 0f, 0f, 0f, 4f, 0f));
      public static Ingredient BLACK_PEPPER = new Ingredient(new Guid("514c3e35-1c23-44ed-9438-7f41244b852f"), "black pepper", new IngredientMetadata(false, false, false, false, false, 0, 0, 3.26f, 0.64f, 251f, 20f, 63.95f));
      public static Ingredient BAKING_POWDER = new Ingredient(new Guid("ab207e00-907c-44d9-8afe-a8931b899b0c"), "baking powder", new IngredientMetadata(false, false, false, false, false, 0, 0, 0f, 0f, 53f, 10600f, 27.7f));
      public static Ingredient LOWFAT_MILK = new Ingredient(new Guid("5a698842-54a9-4ed2-b6c3-aea1bcd157cd"), "2% milk", new IngredientMetadata(false, false, false, false, true, 0, 0, 1.98f, 5.06f, 50f, 47f, 4.8f));
      public static Ingredient LIGHT_BROWN_SUGAR = new Ingredient(new Guid("2d1a807c-5102-4ef4-9620-5e951c8365f9"), "light brown sugar", new IngredientMetadata(false, false, false, false, true, 0, 4, 0f, 97.02f, 380f, 28f, 98.09f));
      public static Ingredient BAKING_SODA = new Ingredient(new Guid("63e799e8-8c12-4302-931e-f058923d97d1"), "baking soda", new IngredientMetadata(false, false, false, false, false, 0, 0, 0f, 0f, 0f, 27360f, 0f));
      public static Ingredient UNSWEETENED_BAKING_CHOCOLATE_SQUARES = new Ingredient(new Guid("bef2bde5-d6b3-45d9-95e9-eb75e05721d4"), "unsweetened baking chocolate squares", new IngredientMetadata(false, false, false, false, true, 0, 1, 52.31f, 0.91f, 501f, 24f, 29.84f));
      public static Ingredient MARGARINE = new Ingredient(new Guid("5b42f415-8012-48b6-86e6-576a2f1dac83"), "margarine (oleo)", new IngredientMetadata(false, false, false, false, true, 0, 0, 60.39f, 0f, 537f, 785f, 0.69f));
   }
}