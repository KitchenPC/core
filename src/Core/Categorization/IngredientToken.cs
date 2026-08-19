using KitchenPC.Core.Ingredients;

namespace KitchenPC.Core.Categorization
{
   internal class IngredientToken : IToken
   {
      private readonly Ingredient ing;

      public IngredientToken(Ingredient ing)
      {
         this.ing = ing;
      }

      public override bool Equals(object obj)
      {
         var t1 = obj as IngredientToken;
         return (t1 != null && t1.ing.Id.Equals(ing.Id));
      }

      public override int GetHashCode()
      {
         return ing.Id.GetHashCode();
      }

      public override string ToString()
      {
         return "[ING] - " + ing;
      }
   }
}
