using System;
using FluentNHibernate.Mapping;
using KitchenPC.Core.Recipes;

namespace KitchenPC.DB.Models;

public class RecipeMetadata
{
   public virtual Guid RecipeMetadataId { get; set; }
   public virtual Recipes Recipe { get; set; }

   public virtual int PhotoRes { get; set; }
   public virtual float Commonality { get; set; }
   public virtual bool UsdaMatch { get; set; }

   public virtual bool MealBreakfast { get; set; }
   public virtual bool MealLunch { get; set; }
   public virtual bool MealDinner { get; set; }
   public virtual bool MealDessert { get; set; }

   public virtual bool DietNomeat { get; set; }
   public virtual bool DietGlutenFree { get; set; }
   public virtual bool DietNoRedMeat { get; set; }
   public virtual bool DietNoAnimals { get; set; }
   public virtual bool DietNoPork { get; set; }

   public virtual short NutritionTotalfat { get; set; }
   public virtual short NutritionTotalSodium { get; set; }
   public virtual bool NutritionLowSodium { get; set; }
   public virtual bool NutritionLowSugar { get; set; }
   public virtual bool NutritionLowCalorie { get; set; }
   public virtual short NutritionTotalSugar { get; set; }
   public virtual short NutritionTotalCalories { get; set; }
   public virtual bool NutritionLowFat { get; set; }
   public virtual bool NutritionLowCarb { get; set; }
   public virtual short NutritionTotalCarbs { get; set; }

   public virtual bool SkillQuick { get; set; }
   public virtual bool SkillEasy { get; set; }
   public virtual bool SkillCommon { get; set; }

   public virtual short TasteMildToSpicy { get; set; }
   public virtual short TasteSavoryToSweet { get; set; }

   public static RecipeMetadata FromId(Guid id) =>
      new()
      {
         RecipeMetadataId = id
      };

   public virtual RecipeTags Tags
   {
      get
      {
         var t = RecipeTags.None;

         if (DietGlutenFree) t |= RecipeTag.GlutenFree;
         if (DietNoAnimals) t |= RecipeTag.NoAnimals;
         if (DietNomeat) t |= RecipeTag.NoMeat;
         if (DietNoPork) t |= RecipeTag.NoPork;
         if (DietNoRedMeat) t |= RecipeTag.NoRedMeat;
         if (MealBreakfast) t |= RecipeTag.Breakfast;
         if (MealDessert) t |= RecipeTag.Dessert;
         if (MealDinner) t |= RecipeTag.Dinner;
         if (MealLunch) t |= RecipeTag.Lunch;
         if (NutritionLowCalorie) t |= RecipeTag.LowCalorie;
         if (NutritionLowCarb) t |= RecipeTag.LowCarb;
         if (NutritionLowFat) t |= RecipeTag.LowFat;
         if (NutritionLowSodium) t |= RecipeTag.LowSodium;
         if (NutritionLowSugar) t |= RecipeTag.LowSugar;
         if (SkillCommon) t |= RecipeTag.Common;
         if (SkillEasy) t |= RecipeTag.Easy;
         if (SkillQuick) t |= RecipeTag.Quick;

         return t;
      }
   }
}

public class RecipeMetadataMap : ClassMap<RecipeMetadata>
{
   public RecipeMetadataMap()
   {
      Id(x => x.RecipeMetadataId, "id")
         .GeneratedBy.GuidComb()
         .UnsavedValue(Guid.Empty);

      Map(x => x.Commonality).Not.Nullable().Index("IDX_RecipeMetadata_Commonality");
      Map(x => x.DietGlutenFree, "diet_glutenfree").Not.Nullable().Index("IDX_RecipeMetadata_DietGlutenFree");
      Map(x => x.DietNoAnimals, "diet_noanimals").Not.Nullable().Index("IDX_RecipeMetadata_DietNoAnimals");
      Map(x => x.DietNomeat, "diet_nomeat").Not.Nullable().Index("IDX_RecipeMetadata_DietNomeat");
      Map(x => x.DietNoPork, "diet_nopork").Not.Nullable().Index("IDX_RecipeMetadata_DietNoPork");
      Map(x => x.DietNoRedMeat, "diet_noredmeat").Not.Nullable().Index("IDX_RecipeMetadata_DietNoRedMeat");
      Map(x => x.MealBreakfast, "meal_breakfast").Not.Nullable().Index("IDX_RecipeMetadata_MealBreakfast");
      Map(x => x.MealDessert, "meal_dessert").Not.Nullable().Index("IDX_RecipeMetadata_MealDessert");
      Map(x => x.MealDinner, "meal_dinner").Not.Nullable().Index("IDX_RecipeMetadata_MealDinner");
      Map(x => x.MealLunch, "meal_lunch").Not.Nullable().Index("IDX_RecipeMetadata_MealLunch");
      Map(x => x.NutritionLowCalorie, "nutrition_lowcalorie").Not.Nullable().Index("IDX_RecipeMetadata_NutritionLowCalorie");
      Map(x => x.NutritionLowCarb, "nutrition_lowcarb").Not.Nullable().Index("IDX_RecipeMetadata_NutritionLowCarb");
      Map(x => x.NutritionLowFat, "nutrition_lowfat").Not.Nullable().Index("IDX_RecipeMetadata_NutritionLowFat");
      Map(x => x.NutritionLowSodium, "nutrition_lowsodium").Not.Nullable().Index("IDX_RecipeMetadata_NutritionLowSodium");
      Map(x => x.NutritionLowSugar, "nutrition_lowsugar").Not.Nullable().Index("IDX_RecipeMetadata_NutritionLowSugar");
      Map(x => x.NutritionTotalCalories, "nutrition_totalcalories").Not.Nullable().Index("IDX_RecipeMetadata_NutritionTotalCalories");
      Map(x => x.NutritionTotalCarbs, "nutrition_totalcarbs").Not.Nullable().Index("IDX_RecipeMetadata_NutritionTotalCarbs");
      Map(x => x.NutritionTotalfat, "nutrition_totalfat").Not.Nullable().Index("IDX_RecipeMetadata_NutritionTotalfat");
      Map(x => x.NutritionTotalSodium, "nutrition_totalsodium").Not.Nullable().Index("IDX_RecipeMetadata_NutritionTotalSodium");
      Map(x => x.NutritionTotalSugar, "nutrition_totalsugar").Not.Nullable().Index("IDX_RecipeMetadata_NutritionTotalSugar");
      Map(x => x.PhotoRes).Not.Nullable().Index("IDX_RecipeMetadata_PhotoRes");
      Map(x => x.SkillCommon, "skill_common").Not.Nullable().Index("IDX_RecipeMetadata_SkillCommon");
      Map(x => x.SkillEasy, "skill_easy").Not.Nullable().Index("IDX_RecipeMetadata_SkillEasy");
      Map(x => x.SkillQuick, "skill_quick").Not.Nullable().Index("IDX_RecipeMetadata_SkillQuick");
      Map(x => x.TasteMildToSpicy, "taste_mildtospicy").Not.Nullable().Index("IDX_RecipeMetadata_TasteMildToSpicy");
      Map(x => x.TasteSavoryToSweet, "taste_savorytosweet").Not.Nullable().Index("IDX_RecipeMetadata_TasteSavoryToSweet");
      Map(x => x.UsdaMatch).Not.Nullable();

      References(x => x.Recipe).Not.Nullable().Unique().Index("IDX_RecipeMetadata_RecipeId");
   }
}
