using System;

namespace KitchenPC.Core.Recipes;

public class RecipeAggregation
{
    public Guid RecipeId { get; set; }
    public float? ServingOverride { get; set; }
}
