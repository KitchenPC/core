using System;
using KitchenPC.Core.Ingredients;

namespace KitchenPC.Core.Modeler;

public struct IngredientBinding
{
   public Guid RecipeId { get; set; }
   public Guid IngredientId { get; set; }
   public Single? Qty { get; set; }
   public Units Unit { get; set; }

   public static IngredientBinding Create(
      Guid ingId,
      Guid recipeId,
      Single? qty,
      Units usageUnit,
      UnitType convType,
      Int32 unitWeight,
      Units? formUnit,
      Single? equivAmount,
      Units? equivUnit
   )
   {
      var rawUnit = Core.Unit.GetDefaultUnitType(convType);

      if (qty.HasValue && rawUnit != usageUnit)
      {
         if (UnitConverter.CanConvert(usageUnit, rawUnit))
         {
            qty = UnitConverter.Convert(qty.Value, usageUnit, rawUnit);
         }
         else
         {
            if (!formUnit.HasValue || !equivAmount.HasValue || !equivUnit.HasValue)
            {
               qty = null;
               return CreateBinding(ingId, recipeId, qty, rawUnit);
            }

            var ing = new Ingredient
            {
               Id = ingId,
               ConversionType = convType,
               UnitWeight = unitWeight,
            };

            var form = new IngredientForm
            {
               FormUnitType = formUnit.Value,
               FormAmount = new Amount(equivAmount.Value, equivUnit.Value),
               IngredientId = ingId,
            };

            var usage = new Ingredients.IngredientUsage
            {
               Form = form,
               Ingredient = ing,
               Amount = new Amount(qty.Value, usageUnit),
            };

            try
            {
               var newAmt = FormConversion.GetNativeAmountForUsage(ing, usage);
               qty = UnitConverter.Convert(newAmt.SizeHigh, newAmt.Unit, rawUnit); //Ingredient graph only stores high amounts
            }
            catch (Exception e)
            {
               throw new DataLoadException(e);
            }
         }
      }

      return CreateBinding(ingId, recipeId, qty, rawUnit);
   }

   private static IngredientBinding CreateBinding(
      Guid ingredientId,
      Guid recipeId,
      Single? quantity,
      Units unit
   ) =>
      new IngredientBinding
      {
         RecipeId = recipeId,
         IngredientId = ingredientId,
         Qty = quantity.HasValue ? (float?)Math.Round(quantity.Value, 3) : null,
         Unit = unit,
      };
}
