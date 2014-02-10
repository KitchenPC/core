using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class NlpUnitSynonyms
   {
      public virtual Guid UnitSynonymId { get; set; }
      public virtual Ingredients Ingredient { get; set; }
      public virtual IngredientForms Form { get; set; }
      public virtual string Name { get; set; }
   }

   public class NlpUnitSynonymsMap : ClassMap<NlpUnitSynonyms>
   {
      public NlpUnitSynonymsMap()
      {
         Id(x => x.UnitSynonymId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.Name).Length(50).UniqueKey("UniquePair");

         References(x => x.Ingredient).Not.Nullable().UniqueKey("UniquePair");
         References(x => x.Form).Not.Nullable();
      }
   }
}