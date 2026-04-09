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
   // TODO: KitchenPC doesn't have this data in a normalized manner, so we use the shoppingingredientsfornlp view to create it on the fly
   // Website could create a new adapter that can load this view, or the base adapter can be configurable so we can map to a certain view and columns
   public NlpDefaultPairingsMap()
   {
      Table("shoppingingredientsfornlp"); // TODO: Make this configurable and less KitchenPC database specific
      Id(x => x.DefaultPairingId, "DefaultPairingId")
         .GeneratedBy.GuidComb()
         .UnsavedValue(Guid.Empty);

      References(x => x.Ingredient, "IngredientId").Unique().Not.Nullable();
      References(x => x.WeightForm, "WeightForm");
      References(x => x.VolumeForm, "VolumetricForm");
      References(x => x.UnitForm, "UnitForm");
   }
}
