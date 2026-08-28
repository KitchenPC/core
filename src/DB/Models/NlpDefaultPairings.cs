using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models;

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
   // This table stores the default weight, volume, and unit forms used by ingredient parsing.
   // The original KitchenPC website populated the same shape through a database view.
   public NlpDefaultPairingsMap()
   {
      Table("shoppingingredientsfornlp");
      Id(x => x.DefaultPairingId, "DefaultPairingId")
         .GeneratedBy.GuidComb()
         .UnsavedValue(Guid.Empty);

      References(x => x.Ingredient, "IngredientId").Unique().Not.Nullable();
      References(x => x.WeightForm, "WeightForm");
      References(x => x.VolumeForm, "VolumetricForm");
      References(x => x.UnitForm, "UnitForm");
   }
}
