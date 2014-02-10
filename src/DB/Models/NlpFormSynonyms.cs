using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class NlpFormSynonyms
   {
      public virtual Guid FormSynonymId { get; set; }
      public virtual Ingredients Ingredient { get; set; }
      public virtual IngredientForms Form { get; set; }
      public virtual string Name { get; set; }
   }

   public class NlpFormSynonymsMap : ClassMap<NlpFormSynonyms>
   {
      public NlpFormSynonymsMap()
      {
         Id(x => x.FormSynonymId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.Name).Length(50).UniqueKey("FormName");

         References(x => x.Ingredient).Not.Nullable().UniqueKey("FormName");
         References(x => x.Form).Not.Nullable();
      }
   }
}