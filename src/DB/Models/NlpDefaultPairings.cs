using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class NlpDefaultPairings
   {
      public virtual Guid DefaultPairingId { get; set; }
      public virtual Ingredients Ingredient { get; set; }
      public virtual IngredientForms WeightForm { get; set; }
      public virtual IngredientForms VolumeForm { get; set; }
      public virtual IngredientForms UnitForm { get; set; }
   }

   public class NlpDefaultPairingsMap : ClassMap<NlpDefaultPairings>
   {
      public NlpDefaultPairingsMap()
      {
         Id(x => x.DefaultPairingId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         References(x => x.Ingredient).Unique().Not.Nullable();
         References(x => x.WeightForm);
         References(x => x.VolumeForm);
         References(x => x.UnitForm);
      }
   }
}