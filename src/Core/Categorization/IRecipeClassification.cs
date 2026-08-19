using KitchenPC.Core.Recipes;

namespace KitchenPC.Core.Categorization;

public interface IRecipeClassification
{
   Recipe Recipe { get; }

   bool IsBreakfast { get; }
   bool IsLunch { get; }
   bool IsDinner { get; }
   bool IsDessert { get; }
}
