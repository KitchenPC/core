using System;
using KitchenPC.Ingredients;

namespace KitchenPC.Data.DTO
{
   public class Ingredients
   {
      public Guid IngredientId { get; set; }
      public string FoodGroup { get; set; }
      public string UsdaId { get; set; }
      public string UnitName { get; set; }
      public string ManufacturerName { get; set; }
      public UnitType ConversionType { get; set; }
      public int UnitWeight { get; set; }
      public string DisplayName { get; set; }
      public string UsdaDesc { get; set; }

      public static Ingredient ToIngredient(Ingredients dtoIngredient, IngredientMetadata metadata = null)
      {
         return new Ingredient
         {
            Id = dtoIngredient.IngredientId,
            ConversionType = dtoIngredient.ConversionType,
            Name = dtoIngredient.DisplayName,
            UnitName = dtoIngredient.UnitName,
            UnitWeight = dtoIngredient.UnitWeight,
            Metadata = (metadata != null ? IngredientMetadata.ToIngredientMetadata(metadata) : null)
         };
      }
   }
}