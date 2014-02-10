using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class NlpAnomalousIngredients
   {
      public virtual Guid AnomalousIngredientId { get; set; }
      public virtual String Name { get; set; }
      public virtual Ingredients Ingredient { get; set; }
      public virtual IngredientForms WeightForm { get; set; }
      public virtual IngredientForms VolumeForm { get; set; }
      public virtual IngredientForms UnitForm { get; set; }
   }

   public class NlpAnomalousIngredientsMap : ClassMap<NlpAnomalousIngredients>
   {
      public NlpAnomalousIngredientsMap()
      {
         Id(x => x.AnomalousIngredientId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.Name).Not.Nullable().Length(100).Unique();

         References(x => x.Ingredient).Not.Nullable();
         References(x => x.WeightForm);
         References(x => x.VolumeForm);
         References(x => x.UnitForm);
      }
   }
}