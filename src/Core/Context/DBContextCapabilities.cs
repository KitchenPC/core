using System;

namespace KitchenPC.Core.Context;

/// <summary>
/// Controls which optional in-memory indexes a <see cref="DBContext"/> initializes.
/// Database-backed recipe, ingredient, menu, queue, and shopping-list operations remain available
/// regardless of the selected capabilities.
/// </summary>
[Flags]
public enum DBContextCapabilities
{
   /// <summary>Initialize only the configured database adapter.</summary>
   None = 0,

   /// <summary>Build the substring index used by ingredient autocomplete.</summary>
   IngredientAutocomplete = 1,

   /// <summary>Build the grammar and synonym indexes used to parse ingredient text.</summary>
   IngredientParsing = 2,

   /// <summary>Load the in-memory recipe graph used by the recipe modeler.</summary>
   RecipeModeler = 4,

   /// <summary>Initialize every optional capability. This is the default for compatibility.</summary>
   All = IngredientAutocomplete | IngredientParsing | RecipeModeler,
}
