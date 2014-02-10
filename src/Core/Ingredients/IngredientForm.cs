using System;

namespace KitchenPC.Ingredients
{
   public class IngredientForm
   {
      public Guid FormId;
      public Guid IngredientId;
      public Units FormUnitType;
      public string FormDisplayName;
      public string FormUnitName;
      public int ConversionMultiplier;
      public Amount FormAmount;

      public static IngredientForm FromId(Guid id)
      {
         return new IngredientForm
         {
            FormId = id
         };
      }

      public IngredientForm()
      {
      }

      public IngredientForm(Guid formid, Guid ingredientid, Units unittype, string displayname, string unitname, int convmultiplier, Amount amount)
      {
         FormId = formid;
         IngredientId = ingredientid;
         FormUnitType = unittype;
         FormDisplayName = displayname;
         FormUnitName = unitname;
         ConversionMultiplier = convmultiplier;
         FormAmount = amount;
      }

      public override string ToString()
      {
         return FormId.ToString();
      }
   }
}