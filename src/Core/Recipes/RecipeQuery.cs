using System;

namespace KitchenPC.Core.Recipes;

public class RecipeQuery
{
   public const int PageSize = 100;

   public enum PhotoFilter
   {
      All = 0,
      Photo = 1,
      HighRes = 2,
   }

   public enum SortOrder
   {
      None = 0,
      Title = 1,
      PrepTime = 2,
      CookTime = 3,
      TotalTime = 4,
      Rating = 5,
      Image = 6,
   }

   public enum SortDirection
   {
      Ascending = 0,
      Descending = 1,
   }

   public enum SpicinessLevel
   {
      Mild = 0,
      MildMedium = 1,
      Medium = 2,
      MediumSpicy = 3,
      Spicy = 4,
   }

   public enum SweetnessLevel
   {
      Savory = 0,
      SavoryMedium = 1,
      Medium = 2,
      MediumSweet = 3,
      Sweet = 4,
   }

   public class TimeFilter
   {
      public short? MaxPrep { get; set; }
      public short? MaxCook { get; set; }
      public short? MaxTime { get; set; }

      public static implicit operator bool(TimeFilter f)
      {
         return f.MaxPrep.HasValue || f.MaxCook.HasValue || f.MaxTime.HasValue;
      }
   }

   public class DietFilter
   {
      public bool GlutenFree { get; set; }
      public bool NoAnimals { get; set; }
      public bool NoMeat { get; set; }
      public bool NoPork { get; set; }
      public bool NoRedMeat { get; set; }

      public static implicit operator bool(DietFilter f)
      {
         return f.GlutenFree || f.NoAnimals || f.NoMeat || f.NoPork || f.NoRedMeat;
      }
   }

   public class NutritionFilter
   {
      public bool LowCalorie { get; set; }
      public bool LowCarb { get; set; }
      public bool LowFat { get; set; }
      public bool LowSodium { get; set; }
      public bool LowSugar { get; set; }

      public static implicit operator bool(NutritionFilter f)
      {
         return f.LowCalorie || f.LowCarb || f.LowFat || f.LowSodium || f.LowSugar;
      }
   }

   public class SkillFilter
   {
      public bool Common { get; set; }
      public bool Easy { get; set; }
      public bool Quick { get; set; }

      public static implicit operator bool(SkillFilter f)
      {
         return f.Common || f.Easy || f.Quick;
      }
   }

   public class TasteFilter
   {
      public SpicinessLevel MildToSpicy { get; set; }
      public SweetnessLevel SavoryToSweet { get; set; }

      private static readonly byte[] SpicyOffsets = { 0, 2, 0, 3, 10 };
      private static readonly byte[] SweetOffsets = { 3, 10, 0, 20, 30 };

      public static implicit operator bool(TasteFilter f)
      {
         return f.MildToSpicy != SpicinessLevel.Medium || f.SavoryToSweet != SweetnessLevel.Medium;
      }

      public byte Spiciness
      {
         get { return SpicyOffsets[(int)MildToSpicy]; }
      }

      public byte Sweetness
      {
         get { return SweetOffsets[(int)SavoryToSweet]; }
      }
   }

   public string Keywords { get; set; }
   public MealFilter Meal { get; set; }
   public Rating? Rating { get; set; }
   public Guid[] Include { get; set; }
   public Guid[] Exclude { get; set; }
   public Int32 Offset { get; set; } //Used for paging
   public TimeFilter Time { get; set; } = new TimeFilter();
   public DietFilter Diet { get; set; } = new DietFilter();
   public NutritionFilter Nutrition { get; set; } = new NutritionFilter();
   public SkillFilter Skill { get; set; } = new SkillFilter();
   public TasteFilter Taste { get; set; } = new TasteFilter();
   public PhotoFilter Photos { get; set; }
   public SortOrder Sort { get; set; }
   public SortDirection Direction { get; set; } //True if sort order is descending

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
      if (query.Include != null)
         this.Include = (Guid[])query.Include.Clone();
      if (query.Exclude != null)
         this.Exclude = (Guid[])query.Exclude.Clone();
      this.Time = query.Time;
      this.Photos = query.Photos;
      this.Sort = query.Sort;
      this.Direction = query.Direction;
   }
}
