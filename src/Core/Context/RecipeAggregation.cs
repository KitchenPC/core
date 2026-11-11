using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;
using IngredientUsage = KitchenPC.Core.Ingredients.IngredientUsage;

namespace KitchenPC.Core.Context;

internal static class RecipeAggregation
{
   internal static async Task<IList<IngredientAggregation>> AggregateAsync(
      Guid[] recipeIds,
      IReadOnlyDictionary<Guid, RecipeNode> cached,
      IBatchedRecipeResolver resolver,
      Func<AuthIdentity> getIdentity,
      CancellationToken cancellationToken
   )
   {
      cancellationToken.ThrowIfCancellationRequested();
      var missing = cached.Where(pair => pair.Value == null).Select(pair => pair.Key).ToArray();
      var resolved = new Dictionary<Guid, Recipe>();
      if (missing.Length > 0 && resolver != null)
      {
         var expected = missing.ToHashSet();
         var recipes = await resolver
            .ResolveAsync(getIdentity(), missing, cancellationToken)
            .ConfigureAwait(false);
         cancellationToken.ThrowIfCancellationRequested();
         if (recipes == null)
            throw new InvalidOperationException("The recipe resolver returned null.");
         foreach (var recipe in recipes)
         {
            if (
               recipe == null
               || !expected.Contains(recipe.Id)
               || !resolved.TryAdd(recipe.Id, recipe)
            )
               throw new InvalidOperationException(
                  "The recipe resolver returned an unexpected or duplicate recipe."
               );
         }
         if (resolved.Count != expected.Count)
            throw new InvalidOperationException(
               "The recipe resolver did not return every requested recipe."
            );
      }

      var usages = new List<IngredientUsage>();
      foreach (var id in recipeIds)
      {
         cancellationToken.ThrowIfCancellationRequested();
         var node = cached[id];
         if (node != null)
         {
            usages.AddRange(
               node.Ingredients.Select(usage => new IngredientUsage
               {
                  Ingredient = new Ingredient(
                     usage.Ingredient.IngredientId,
                     usage.Ingredient.DisplayName
                  )
                  {
                     ConversionType = usage.Ingredient.ConvType,
                  },
                  Amount = usage.Amt.HasValue ? new Amount(usage.Amt.Value, usage.Unit) : null,
               })
            );
         }
         else if (resolved.TryGetValue(id, out var recipe))
            usages.AddRange(recipe.Ingredients);
      }
      return AggregateUsages(usages);
   }

   internal static IList<IngredientAggregation> AggregateUsages(
      IEnumerable<IngredientUsage> usages
   ) =>
      usages
         .Where(usage => usage.Ingredient.Id != ShoppingList.GUID_WATER)
         .GroupBy(usage => usage.Ingredient.Id)
         .Select(group =>
         {
            var aggregation = new IngredientAggregation(group.First().Ingredient);
            foreach (var usage in group)
            {
               if (usage.Amount == null)
               {
                  aggregation.Amount = null;
                  break;
               }
               aggregation.AddUsage(usage);
            }
            return aggregation;
         })
         .ToArray();
}
