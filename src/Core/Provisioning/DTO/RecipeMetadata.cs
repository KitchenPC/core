using System;
using KitchenPC.Recipes;

namespace KitchenPC.Data.DTO
{
   public class RecipeMetadata
   {
      public Guid RecipeMetadataId { get; set; }
      public Guid RecipeId { get; set; }

      public int PhotoRes { get; set; }
      public float Commonality { get; set; }
      public bool UsdaMatch { get; set; }

      public bool MealBreakfast { get; set; }
      public bool MealLunch { get; set; }
      public bool MealDinner { get; set; }
      public bool MealDessert { get; set; }

      public bool DietNomeat { get; set; }
      public bool DietGlutenFree { get; set; }
      public bool DietNoRedMeat { get; set; }
      public bool DietNoAnimals { get; set; }
      public bool DietNoPork { get; set; }

      public short NutritionTotalfat { get; set; }
      public short NutritionTotalSodium { get; set; }
      public bool NutritionLowSodium { get; set; }
      public bool NutritionLowSugar { get; set; }
      public bool NutritionLowCalorie { get; set; }
      public short NutritionTotalSugar { get; set; }
      public short NutritionTotalCalories { get; set; }
      public bool NutritionLowFat { get; set; }
      public bool NutritionLowCarb { get; set; }
      public short NutritionTotalCarbs { get; set; }

      public bool SkillQuick { get; set; }
      public bool SkillEasy { get; set; }
      public bool SkillCommon { get; set; }

      public short TasteMildToSpicy { get; set; }
      public short TasteSavoryToSweet { get; set; }

      public static RecipeTags ToRecipeTags(RecipeMetadata metadata)
      {
         return
            (metadata.DietGlutenFree ? 1 << 0 : 0) +
            (metadata.DietNoAnimals ? 1 << 1 : 0) +
            (metadata.DietNomeat ? 1 << 2 : 0) +
            (metadata.DietNoPork ? 1 << 3 : 0) +
            (metadata.DietNoRedMeat ? 1 << 4 : 0) +
            (metadata.MealBreakfast ? 1 << 5 : 0) +
            (metadata.MealDessert ? 1 << 6 : 0) +
            (metadata.MealDinner ? 1 << 7 : 0) +
            (metadata.MealLunch ? 1 << 8 : 0) +
            (metadata.NutritionLowCalorie ? 1 << 9 : 0) +
            (metadata.NutritionLowCarb ? 1 << 10 : 0) +
            (metadata.NutritionLowFat ? 1 << 11 : 0) +
            (metadata.NutritionLowSodium ? 1 << 12 : 0) +
            (metadata.NutritionLowSugar ? 1 << 13 : 0) +
            (metadata.SkillCommon ? 1 << 14 : 0) +
            (metadata.SkillEasy ? 1 << 15 : 0) +
            (metadata.SkillQuick ? 1 << 16 : 0);
      }
   }
}