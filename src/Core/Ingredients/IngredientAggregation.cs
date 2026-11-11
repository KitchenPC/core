using System;
using KitchenPC.Core.ShoppingLists;

namespace KitchenPC.Core.Ingredients;

public class IngredientAggregation : IShoppingListSource
{
   public Ingredient Ingredient { get; set; }
   public Amount Amount { get; set; }

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
         throw new ArgumentException(
            "Can only call IngredientAggregation::AddUsage() on original ingredient."
         );

      Amount amount;
      if (UnitConverter.CanConvert(this.Amount.Unit, ingredient.Amount.Unit))
         amount = UnitConverter.Convert(ingredient.Amount, this.Amount.Unit);
      else
      {
         // The usage carries full ingredient metadata; a cached public usage may not have unit weight.
         amount = FormConversion.GetNativeAmountForUsage(ingredient.Ingredient, ingredient);
         if (ingredient.Amount.SizeLow.HasValue)
         {
            var lowUsage = new IngredientUsage
            {
               Ingredient = ingredient.Ingredient,
               Form = ingredient.Form,
               Amount = new Amount(ingredient.Amount.SizeLow.Value, ingredient.Amount.Unit),
            };
            amount.SizeLow = FormConversion
               .GetNativeAmountForUsage(ingredient.Ingredient, lowUsage)
               .SizeHigh;
         }
         amount = UnitConverter.Convert(amount, this.Amount.Unit);
      }
      this.Amount += amount;

      return this; // Allows AddUsage calls to be chained together
   }

   public virtual ShoppingListItem GetItem()
   {
      return new ShoppingListItem(Ingredient) { Amount = Amount };
   }
}
