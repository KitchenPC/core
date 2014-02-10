using System;

namespace KitchenPC.Recipes
{
   public class RecipeQuery
   {
      public enum PhotoFilter
      {
         All,
         Photo,
         HighRes
      }

      public enum SortOrder
      {
         None,
         Title,
         PrepTime,
         CookTime,
         Rating,
         Image
      }

      public enum SortDirection
      {
         Ascending,
         Descending
      }

      public enum SpicinessLevel
      {
         Mild,
         MildMedium,
         Medium,
         MediumSpicy,
         Spicy
      }

      public enum SweetnessLevel
      {
         Savory,
         SavoryMedium,
         Medium,
         MediumSweet,
         Sweet
      }

      public struct TimeFilter
      {
         public short? MaxPrep;
         public short? MaxCook;

         public static implicit operator bool(TimeFilter f)
         {
            return f.MaxPrep.HasValue || f.MaxCook.HasValue;
         }
      }

      public struct DietFilter
      {
         public bool GlutenFree;
         public bool NoAnimals;
         public bool NoMeat;
         public bool NoPork;
         public bool NoRedMeat;

         public static implicit operator bool(DietFilter f)
         {
            return f.GlutenFree || f.NoAnimals || f.NoMeat || f.NoPork || f.NoRedMeat;
         }
      }

      public struct NutritionFilter
      {
         public bool LowCalorie;
         public bool LowCarb;
         public bool LowFat;
         public bool LowSodium;
         public bool LowSugar;

         public static implicit operator bool(NutritionFilter f)
         {
            return f.LowCalorie || f.LowCarb || f.LowFat || f.LowSodium || f.LowSugar;
         }
      }

      public struct SkillFilter
      {
         public bool Common;
         public bool Easy;
         public bool Quick;

         public static implicit operator bool(SkillFilter f)
         {
            return f.Common || f.Easy || f.Quick;
         }
      }

      public struct TasteFilter
      {
         public SpicinessLevel MildToSpicy;
         public SweetnessLevel SavoryToSweet;

         static readonly byte[] SpicyOffsets = {0, 2, 0, 3, 10};
         static readonly byte[] SweetOffsets = {3, 10, 0, 20, 30};

         public static implicit operator bool(TasteFilter f)
         {
            return f.MildToSpicy != SpicinessLevel.Medium || f.SavoryToSweet != SweetnessLevel.Medium;
         }

         public byte Spiciness
         {
            get
            {
               return SpicyOffsets[(int) MildToSpicy];
            }
         }

         public byte Sweetness
         {
            get
            {
               return SweetOffsets[(int) SavoryToSweet];
            }
         }
      }

      public string Keywords;
      public MealFilter Meal;
      public Rating? Rating;
      public Guid[] Include;
      public Guid[] Exclude;
      public Int32 Offset; //Used for paging
      public TimeFilter Time;
      public DietFilter Diet;
      public NutritionFilter Nutrition;
      public SkillFilter Skill;
      public TasteFilter Taste;
      public PhotoFilter Photos;
      public SortOrder Sort;
      public SortDirection Direction; //True if sort order is descending

      public RecipeQuery()
      {
         Taste.MildToSpicy = SpicinessLevel.Medium;
         Taste.SavoryToSweet = SweetnessLevel.Medium;

         Sort = SortOrder.Rating;
         Direction = SortDirection.Descending;
      }

      public RecipeQuery(RecipeQuery query)
      {
         this.Keywords = query.Keywords;
         this.Rating = query.Rating;
         if (query.Include != null) this.Include = (Guid[]) query.Include.Clone();
         if (query.Exclude != null) this.Exclude = (Guid[]) query.Exclude.Clone();
         this.Time = query.Time;
         this.Photos = query.Photos;
         this.Sort = query.Sort;
         this.Direction = query.Direction;
      }
   }
}