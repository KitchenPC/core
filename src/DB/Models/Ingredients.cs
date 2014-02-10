using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using KitchenPC.Ingredients;

namespace KitchenPC.DB.Models
{
   public class Ingredients
   {
      public virtual Guid IngredientId { get; set; }
      public virtual string FoodGroup { get; set; }
      public virtual string UsdaId { get; set; }
      public virtual string UnitName { get; set; }
      public virtual string ManufacturerName { get; set; }
      public virtual UnitType ConversionType { get; set; }
      public virtual int UnitWeight { get; set; }
      public virtual string DisplayName { get; set; }
      public virtual string UsdaDesc { get; set; }

      public virtual IList<IngredientForms> Forms { get; set; }
      public virtual IngredientMetadata Metadata { get; set; }

      public virtual Ingredient AsIngredient()
      {
         return new Ingredient
         {
            Id = IngredientId,
            ConversionType = ConversionType,
            Name = DisplayName,
            UnitName = UnitName,
            UnitWeight = UnitWeight,
            Metadata = (Metadata != null ? Metadata.AsIngredientMetadata() : null)
         };
      }

      public static Ingredients FromId(Guid id)
      {
         return new Ingredients
         {
            IngredientId = id
         };
      }
   }

   public class IngredientsMap : ClassMap<Ingredients>
   {
      public IngredientsMap()
      {
         Id(x => x.IngredientId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.FoodGroup).Length(4);
         Map(x => x.UsdaId).Length(5);
         Map(x => x.UnitName).Length(50);
         Map(x => x.ManufacturerName).Length(65);
         Map(x => x.ConversionType).Not.Nullable();
         Map(x => x.UnitWeight).Not.Nullable().Default("0");
         Map(x => x.DisplayName).Not.Nullable().Length(200).Unique().Index("IDX_Ingredients_DisplayName");
         Map(x => x.UsdaDesc).Length(200);

         HasMany(x => x.Forms).KeyColumn("IngredientId");
         HasOne(x => x.Metadata).PropertyRef(x => x.Ingredient).Cascade.All();
      }
   }
}