using System;
using KitchenPC.Ingredients;

namespace KitchenPC.Modeler
{
   public struct IngredientBinding
   {
      public Guid RecipeId { get; set; }
      public Guid IngredientId { get; set; }
      public Single? Qty { get; set; }
      public Units Unit { get; set; }

      public static IngredientBinding Create(Guid ingId, Guid recipeId, Single? qty, Units usageUnit, UnitType convType, Int32 unitWeight,
         Units formUnit, Single equivAmount, Units equivUnit)
      {
         var rawUnit = KitchenPC.Unit.GetDefaultUnitType(convType);

         if (qty.HasValue && rawUnit != usageUnit)
         {
            if (UnitConverter.CanConvert(usageUnit, rawUnit))
            {
               qty = UnitConverter.Convert(qty.Value, usageUnit, rawUnit);
            }
            else
            {
               var ing = new Ingredient
               {
                  Id = ingId,
                  ConversionType = convType,
                  UnitWeight = unitWeight
               };

               var form = new IngredientForm
               {
                  FormUnitType = formUnit,
                  FormAmount = new Amount(equivAmount, equivUnit),
                  IngredientId = ingId
               };

               var usage = new Ingredients.IngredientUsage
               {
                  Form = form,
                  Ingredient = ing,
                  Amount = new Amount(qty.Value, usageUnit)
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

         return new IngredientBinding
         {
            RecipeId = recipeId,
            IngredientId = ingId,
            Qty = qty.HasValue ? (float?) Math.Round(qty.Value, 3) : null,
            Unit = rawUnit
         };
      }
   }
}