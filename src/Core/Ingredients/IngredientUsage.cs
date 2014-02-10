using System;
using KitchenPC.Context.Fluent;

namespace KitchenPC.Ingredients
{
   public class IngredientUsage
   {
      public Ingredient Ingredient;
      public IngredientForm Form;
      public Amount Amount;
      public String PrepNote;
      public String Section;

      public static IngredientUsageCreator Create
      {
         get
         {
            return new IngredientUsageCreator(new IngredientUsage());
         }
      }

      public IngredientUsage(Ingredient ingredient, IngredientForm form, Amount amount, String prepnote)
      {
         Ingredient = ingredient;
         Form = form;
         Amount = amount;
         PrepNote = prepnote;
      }

      public IngredientUsage()
      {
      }

      /// <summary>Renders Ingredient Usage, using ingredientTemplate for the ingredient name.</summary>
      /// <param name="ingredientTemplate">A string template for the ingredient name, {0} will be the Ingredient Id and {1} will be the ingredient name.</param>
      /// <param name="amountTemplate">Optional string template for displaying amounts.  {0} will be numeric value, {1} will be unit.</param>
      /// <param name="multiplier">Number to multiply amount by, used to adjust recipe servings.</param>
      /// <returns>Ingredient Name (form): Amount (prep note)</returns>
      public string ToString(string ingredientTemplate, string amountTemplate, float multiplier)
      {
         var ingname = String.IsNullOrEmpty(ingredientTemplate) ? Ingredient.Name : String.Format(ingredientTemplate, Ingredient.Id, Ingredient.Name);
         var prep = String.Empty;
         string amount;

         if (!String.IsNullOrEmpty(PrepNote))
         {
            prep = String.Format(" ({0})", PrepNote);
         }

         if (Amount == null) //Just display ingredient and prep
         {
            return String.Format("{0}{1}", ingname, prep);
         }

         //Normalize amount and form
         var normalizedAmt = (Amount == null ? null : Amount.Normalize(Amount, multiplier));
         if (Form.FormUnitType != Units.Unit && !String.IsNullOrEmpty(Form.FormDisplayName))
         {
            ingname += String.Format(" ({0})", Form.FormDisplayName);
         }

         var unitType = Unit.GetConvType(Form.FormUnitType);

         if (unitType == UnitType.Unit && !String.IsNullOrEmpty(Form.FormUnitName))
         {
            var names = Form.FormUnitName.Split('/');
            var unitName = (normalizedAmt.SizeLow.HasValue || normalizedAmt.SizeHigh > 1) ? names[1] : names[0];
            amount = normalizedAmt.ToString(unitName);
         }
         else
         {
            amount = normalizedAmt.ToString();
         }

         var amt = String.Format(String.IsNullOrEmpty(amountTemplate) ? "{0}{1}" : amountTemplate, amount, prep);
         return String.Format("{0}: {1}", ingname, amt);
      }

      public string ToString(float multiplier)
      {
         return ToString(null, null, multiplier);
      }

      public override string ToString()
      {
         return ToString(null, null, 1);
      }
   }
}