using System;
using FluentNHibernate.Mapping;
using KitchenPC.Ingredients;

namespace KitchenPC.DB.Models
{
   public class IngredientForms
   {
      public virtual Guid IngredientFormId { get; set; }
      public virtual Ingredients Ingredient { get; set; }

      public virtual short ConvMultiplier { get; set; }
      public virtual float FormAmount { get; set; }
      public virtual Units UnitType { get; set; }
      public virtual string UnitName { get; set; }
      public virtual Units FormUnit { get; set; }
      public virtual string FormDisplayName { get; set; }

      public static IngredientForms FromId(Guid id)
      {
         return new IngredientForms
         {
            IngredientFormId = id
         };
      }

      public virtual IngredientForm AsIngredientForm()
      {
         return new IngredientForm
         {
            FormId = IngredientFormId,
            FormUnitType = UnitType,
            ConversionMultiplier = ConvMultiplier,
            FormDisplayName = FormDisplayName,
            FormUnitName = UnitName,
            IngredientId = Ingredient.IngredientId,
            FormAmount = new Amount(FormAmount, FormUnit)
         };
      }
   }

   public class IngredientFormsMap : ClassMap<IngredientForms>
   {
      public IngredientFormsMap()
      {
         Id(x => x.IngredientFormId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.ConvMultiplier).Not.Nullable();
         Map(x => x.FormAmount).Not.Nullable();
         Map(x => x.UnitType).Not.Nullable();
         Map(x => x.UnitName).Length(50);
         Map(x => x.FormUnit).Not.Nullable();
         Map(x => x.FormDisplayName).Length(200).UniqueKey("UniqueIngredientForm");

         References(x => x.Ingredient).Not.Nullable().UniqueKey("UniqueIngredientForm");
      }
   }
}