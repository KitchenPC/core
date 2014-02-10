using System;

namespace KitchenPC.Data.DTO
{
   public class NlpDefaultPairings
   {
      public virtual Guid DefaultPairingId { get; set; }
      public virtual Guid IngredientId { get; set; }
      public virtual Guid? WeightFormId { get; set; }
      public virtual Guid? VolumeFormId { get; set; }
      public virtual Guid? UnitFormId { get; set; }
   }
}