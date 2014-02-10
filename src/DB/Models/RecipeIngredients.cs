using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
   public class RecipeIngredients
   {
      public virtual Guid RecipeIngredientId { get; set; }
      public virtual Recipes Recipe { get; set; }
      public virtual Ingredients Ingredient { get; set; }
      public virtual IngredientForms IngredientForm { get; set; }

      public virtual Units Unit { get; set; }
      public virtual float? QtyLow { get; set; }
      public virtual short DisplayOrder { get; set; }
      public virtual string PrepNote { get; set; }
      public virtual float? Qty { get; set; }
      public virtual string Section { get; set; }
   }

   public class RecipeIngredientsMap : ClassMap<RecipeIngredients>
   {
      public RecipeIngredientsMap()
      {
         Id(x => x.RecipeIngredientId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.Unit).Not.Nullable();
         Map(x => x.QtyLow);
         Map(x => x.DisplayOrder).Not.Nullable();
         Map(x => x.PrepNote).Length(50);
         Map(x => x.Qty);
         Map(x => x.Section).Length(50);

         References(x => x.Recipe).Column("RecipeId").Not.Nullable().Index("IDX_RecipeIngredients_RecipeId");
         References(x => x.Ingredient).Column("IngredientId").Not.Nullable().Index("IDX_RecipeIngredients_IngredientId");
         References(x => x.IngredientForm).Column("IngredientFormId");
      }
   }
}