using System;

namespace KitchenPC.Core.Provisioning.DTO;

public class NlpIngredientSynonyms
{
   public Guid IngredientSynonymId { get; set; }
   public Guid IngredientId { get; set; }
   public string Alias { get; set; }
   public string Prepnote { get; set; }
}
