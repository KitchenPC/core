using System;

namespace KitchenPC.Modeler
{
   public struct IngredientUsage
   {
      public IngredientNode Ingredient; //Reference to IngredientNode describing this ingredient
      public Single? Amt; //Amount of ingredient, expressed in default units for ingredient
      public Units Unit; //Unit for this amount (will always be compatible with Ingredient.ConvType)
   }
}