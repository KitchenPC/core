using System;
using KitchenPC.Ingredients;

namespace KitchenPC.Data.DTO
{
   public class IngredientForms
   {
      public Guid IngredientFormId { get; set; }
      public Guid IngredientId { get; set; }
      public short ConvMultiplier { get; set; }
      public float FormAmount { get; set; }
      public Units UnitType { get; set; }
      public string UnitName { get; set; }
      public Units FormUnit { get; set; }
      public string FormDisplayName { get; set; }

      public static IngredientForm ToIngredientForm(IngredientForms dtoForm)
      {
         return new IngredientForm
         {
            FormId = dtoForm.IngredientFormId,
            FormUnitType = dtoForm.UnitType,
            ConversionMultiplier = dtoForm.ConvMultiplier,
            FormDisplayName = dtoForm.FormDisplayName,
            FormUnitName = dtoForm.UnitName,
            IngredientId = dtoForm.IngredientId,
            FormAmount = new Amount(dtoForm.FormAmount, dtoForm.FormUnit)
         };
      }
   }
}