using System;
using KitchenPC.ShoppingLists;

namespace KitchenPC.Ingredients
{
   public class IngredientAggregation : IShoppingListSource
   {
      public Ingredient Ingredient;
      public Amount Amount;

      public IngredientAggregation(Ingredient ingredient)
      {
         this.Ingredient = ingredient;

         if (ingredient != null)
         {
            this.Amount = new Amount();
            this.Amount.Unit = Unit.GetDefaultUnitType(ingredient.ConversionType);
         }
      }

      public IngredientAggregation(Ingredient ingredient, Amount baseAmount)
      {
         this.Ingredient = ingredient;
         this.Amount = baseAmount;
      }

      public override string ToString()
      {
         if (Ingredient != null && Amount != null)
            return String.Format("{0}: {1}", Ingredient.Name, Amount);

         if (Ingredient != null)
            return Ingredient.Name;

         return String.Empty;
      }

      public virtual IngredientAggregation AddUsage(IngredientUsage ingredient)
      {
         if (ingredient.Ingredient.Id != this.Ingredient.Id)
            throw new ArgumentException("Can only call IngredientAggregation::AddUsage() on original ingredient.");

         //Calculate new total
         if (this.Amount.Unit == ingredient.Amount.Unit || UnitConverter.CanConvert(this.Amount.Unit, ingredient.Amount.Unit)) //Just add
         {
            this.Amount += ingredient.Amount;
         }
         else //Find a conversion path between Ingredient and Form
         {
            var amount = FormConversion.GetNativeAmountForUsage(this.Ingredient, ingredient);
            this.Amount += amount;
         }

         return this; // Allows AddUsage calls to be chained together
      }

      public virtual ShoppingListItem GetItem()
      {
         return new ShoppingListItem(Ingredient)
         {
            Amount = Amount
         };
      }
   }
}