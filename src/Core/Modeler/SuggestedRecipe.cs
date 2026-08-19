using System;
using KitchenPC.Core.Ingredients;

namespace KitchenPC.Core.Modeler;

/// <summary>
/// A recipe suggested by the modeler.
/// </summary>
public class SuggestedRecipe
{
   public Guid Id { get; set; }
   public IngredientAggregation[] Ingredients { get; set; }
}
