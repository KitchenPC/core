using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class IngredientMetadata
   {
      public virtual Guid IngredientMetadataId { get; set; }
      public virtual Ingredients Ingredient { get; set; }

      public virtual bool? HasMeat { get; set; }
      public virtual float? CarbsPerUnit { get; set; }
      public virtual bool? HasRedMeat { get; set; }
      public virtual float? SugarPerUnit { get; set; }
      public virtual bool? HasPork { get; set; }
      public virtual float? FatPerUnit { get; set; }
      public virtual float? SodiumPerUnit { get; set; }
      public virtual float? CaloriesPerUnit { get; set; }
      public virtual short Spicy { get; set; }
      public virtual short Sweet { get; set; }
      public virtual bool? HasGluten { get; set; }
      public virtual bool? HasAnimal { get; set; }

      public virtual KitchenPC.Ingredients.IngredientMetadata AsIngredientMetadata()
      {
         return new KitchenPC.Ingredients.IngredientMetadata
         {
            HasMeat = HasMeat,
            CarbsPerUnit = CarbsPerUnit,
            HasRedMeat = HasRedMeat,
            SugarPerUnit = SugarPerUnit,
            HasPork = HasPork,
            FatPerUnit = FatPerUnit,
            SodiumPerUnit = SodiumPerUnit,
            CaloriesPerUnit = CaloriesPerUnit,
            Spicy = Spicy,
            Sweet = Sweet,
            HasGluten = HasGluten,
            HasAnimal = HasAnimal
         };
      }
   }

   public class IngredientMetadataMap : ClassMap<IngredientMetadata>
   {
      public IngredientMetadataMap()
      {
         Id(x => x.IngredientMetadataId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.HasMeat);
         Map(x => x.CarbsPerUnit);
         Map(x => x.HasRedMeat);
         Map(x => x.SugarPerUnit);
         Map(x => x.HasPork);
         Map(x => x.FatPerUnit);
         Map(x => x.SodiumPerUnit);
         Map(x => x.CaloriesPerUnit);
         Map(x => x.Spicy).Not.Nullable();
         Map(x => x.Sweet).Not.Nullable();
         Map(x => x.HasGluten);
         Map(x => x.HasAnimal);

         References(x => x.Ingredient).Not.Nullable().Unique();
      }
   }
}