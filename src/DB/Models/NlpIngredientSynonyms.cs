using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class NlpIngredientSynonyms
   {
      public virtual Guid IngredientSynonymId { get; set; }
      public virtual Ingredients Ingredient { get; set; }
      public virtual string Alias { get; set; }
      public virtual string Prepnote { get; set; }
   }

   public class NlpIngredientSynonymsMap : ClassMap<NlpIngredientSynonyms>
   {
      public NlpIngredientSynonymsMap()
      {
         Id(x => x.IngredientSynonymId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.Alias).Length(200).Unique();
         Map(x => x.Prepnote).Length(50);

         References(x => x.Ingredient).Not.Nullable();
      }
   }
}