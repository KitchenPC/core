using System;

namespace KitchenPC.Data.DTO
{
   public class NlpFormSynonyms
   {
      public Guid FormSynonymId { get; set; }
      public Guid IngredientId { get; set; }
      public Guid FormId { get; set; }
      public string Name { get; set; }
   }
}