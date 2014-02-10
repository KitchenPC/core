using System;

namespace KitchenPC.Data.DTO
{
   public class NlpAnomalousIngredients
   {
      public Guid AnomalousIngredientId { get; set; }
      public String Name { get; set; }
      public Guid IngredientId { get; set; }
      public Guid? WeightFormId { get; set; }
      public Guid? VolumeFormId { get; set; }
      public Guid? UnitFormId { get; set; }
   }
}