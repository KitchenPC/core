using System;

namespace KitchenPC.Data.DTO
{
   public class NlpUnitSynonyms
   {
      public Guid UnitSynonymId { get; set; }
      public Guid IngredientId { get; set; }
      public Guid FormId { get; set; }
      public string Name { get; set; }
   }
}