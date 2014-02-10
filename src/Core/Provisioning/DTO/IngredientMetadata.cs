using System;

namespace KitchenPC.Data.DTO
{
   public class IngredientMetadata
   {
      public Guid IngredientMetadataId { get; set; }
      public Guid IngredientId { get; set; }

      public bool? HasMeat { get; set; }
      public float? CarbsPerUnit { get; set; }
      public bool? HasRedMeat { get; set; }
      public float? SugarPerUnit { get; set; }
      public bool? HasPork { get; set; }
      public float? FatPerUnit { get; set; }
      public float? SodiumPerUnit { get; set; }
      public float? CaloriesPerUnit { get; set; }
      public short Spicy { get; set; }
      public short Sweet { get; set; }
      public bool? HasGluten { get; set; }
      public bool? HasAnimal { get; set; }

      public static KitchenPC.Ingredients.IngredientMetadata ToIngredientMetadata(IngredientMetadata metadata)
      {
         return new KitchenPC.Ingredients.IngredientMetadata
         {
            HasMeat = metadata.HasMeat,
            CarbsPerUnit = metadata.CarbsPerUnit,
            HasRedMeat = metadata.HasRedMeat,
            SugarPerUnit = metadata.SugarPerUnit,
            HasPork = metadata.HasPork,
            FatPerUnit = metadata.FatPerUnit,
            SodiumPerUnit = metadata.SodiumPerUnit,
            CaloriesPerUnit = metadata.CaloriesPerUnit,
            Spicy = metadata.Spicy,
            Sweet = metadata.Sweet,
            HasGluten = metadata.HasGluten,
            HasAnimal = metadata.HasAnimal
         };
      }
   }
}