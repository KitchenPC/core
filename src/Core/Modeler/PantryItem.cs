using System;

namespace KitchenPC.Modeler
{
   public struct PantryItem
   {
      public Guid IngredientId; //KPC Shopping Ingredient ID
      public Single? Amt; //Optional amount of ingredient, expressed in default units for ingredient

      public PantryItem(Ingredients.IngredientUsage usage)
      {
         IngredientId = usage.Ingredient.Id;

         //Need to convert IngredientUsage into proper Pantry form
         if (usage.Amount != null)
         {
            var toUnit = Unit.GetDefaultUnitType(usage.Ingredient.ConversionType);
            if (UnitConverter.CanConvert(usage.Form.FormUnitType, toUnit))
            {
               Amt = UnitConverter.Convert(usage.Amount, toUnit).SizeHigh; //Always take high amount for pantry items
            }
            else //Find conversion path
            {
               var amount = FormConversion.GetNativeAmountForUsage(usage.Ingredient, usage);
               Amt = UnitConverter.Convert(amount, toUnit).SizeHigh; //Always take high amount for pantry items
            }
         }
         else
         {
            Amt = null;
         }
      }
   }
}