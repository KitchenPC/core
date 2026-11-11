using System;
using System.Threading;
using System.Threading.Tasks;
using KitchenPC.Core.Recipes;

namespace KitchenPC.Core.Context;

/// <summary>Resolves aggregation cache misses without adding recipes to the public modeler.</summary>
public interface IBatchedRecipeResolver
{
   /// <summary>
   /// Returns every requested recipe with its current ingredients and forms. Implementations must
   /// authorize access using identity and throw if any recipe is missing or inaccessible.
   /// Instances are shared across users and must support concurrent calls without retaining identity.
   /// </summary>
   Task<Recipe[]> ResolveAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   );
}
