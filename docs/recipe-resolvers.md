# Batched recipe resolution (2.1.1)

Recipe aggregation uses the public modeler snapshot first. An optional
`IBatchedRecipeResolver` supplies recipes absent from that snapshot. This lets an
application aggregate fresh personal recipes alongside immutable public recipes
without querying the database for cached public recipes.

## Configuration

Configure the resolver on the database adapter, alongside the search provider:

```csharp
var context = DBContext.Configure
   .Adapter(DatabaseAdapter.Configure
      .DatabaseConfiguration(databaseConfiguration)
      .SearchProvider(NHSearch.Instance)
      .RecipeResolver(adapter => new PersonalRecipeResolver(adapter)))
   .Identity(() => AuthIdentity.Anonymous)
   .Create();
context.Initialize();

var userContext = context.AsUserContext(currentIdentity);
var ingredients = await userContext.AggregateRecipesAsync(recipeIds, cancellationToken);
```

The factory runs during configuration, before adapter initialization. Store the
adapter in the constructor; open sessions only inside `ResolveAsync`. The resolver
is shared by user contexts, so it must be thread-safe and must not retain a current
user or an NHibernate session. ASP.NET DI activation is a separate enhancement
tracked by [Core #26](https://github.com/KitchenPC/core/issues/26).

A database-backed implementation can use the adapter's asynchronous batch reader:

```csharp
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Recipes;
using KitchenPC.DB;

public sealed class PersonalRecipeResolver(DatabaseAdapter adapter) : IBatchedRecipeResolver
{
   public async Task<Recipe[]> ResolveAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default)
   {
      if (!identity.IsAuthenticated)
         throw new UnauthorizedAccessException();

      // ReadRecipesAsync loads ingredients, forms, and ingredient conversion metadata.
      // It does NOT enforce recipe visibility: the resolver must authorize the results.
      var recipes = await adapter.ReadRecipesAsync(
         identity, recipeIds, ReadRecipeOptions.None, cancellationToken)
         .ConfigureAwait(false);

      // This example accepts only recipes owned by the caller, including hidden ones.
      // Do not return a partial list or distinguish missing from inaccessible recipes.
      if (recipes.Length != recipeIds.Length || recipes.Any(r => r.OwnerId != identity.UserId))
         throw new UnauthorizedAccessException("One or more recipes are unavailable.");

      return recipes;
   }
}
```

This is application code, not a built-in authorization policy. A broader resolver
may accept `!r.Hidden || r.OwnerId == identity.UserId`, if public cache misses should
also fall back to storage. A cache miss alone does not prove a recipe is personal.
For stricter data isolation, use `adapter.GetSession()` to issue an owner-filtered
batch query and populate the same complete recipe/ingredient/form data.

`StaticContext.Configure.RecipeResolver(resolver)` also accepts a resolver instance.
Custom `IDBAdapter` implementations can expose the optional `RecipeResolver`
property; its default implementation returns null.

## Contract and behavior

- No resolver is configured by default. Missing recipes are skipped as before.
- Cached public recipes cause **no resolver calls or database reads**, including
  ingredient-name lookups. Custom modeler loaders should supply ingredient names.
- All distinct misses are passed to the resolver once per aggregation. Repeated
  recipe IDs are resolved once but counted once per occurrence in the input.
- The current `AuthIdentity` and cancellation token are passed on each call.
- Return exactly one complete recipe per requested ID. Null, duplicate, unexpected,
  or missing results fail the whole aggregation. Exceptions propagate; partial
  shopping lists are not saved.
- Resolved recipes are never inserted into the modeler. Subsequent aggregations
  resolve them again, so edits are immediately reflected. Applications can later
  add their own authorization-aware caching and invalidation inside the resolver.
- Ingredient forms are normalized before merging with cached public usages.
  Resolved quantity ranges are preserved; any unknown amount makes that ingredient's
  total unknown. Water is omitted. Public graph quantities retain the existing
  upper-bound representation.
- Database and XML graph loaders exclude hidden recipes, their ingredient links,
  and their ratings. The snapshot also rejects hidden recipes from custom loaders.
  Modeler suggestions never call the resolver and remain public-only.
- With `DBContextCapabilities.RecipeModeler` disabled, aggregation continues to use
  the adapter's batch reader directly; the resolver is not used. That existing
  mode has no public-cache performance guarantee or additional authorization policy.

`AggregateRecipesAsync` is the asynchronous entry point. Existing synchronous
`AggregateRecipes`, shopping-list creation, and shopping-list updates use the same
logic and wait for resolution. Use the async entry point in async request handlers;
resolver implementations should avoid capturing a synchronization context when
awaiting I/O, since synchronous callers can otherwise block that context.

For printing, aggregate recipe IDs through the same API rather than reading recipe
nodes directly. Saved shopping lists remain materialized ingredient lists: printing
an existing list does not re-resolve recipes or retroactively change amounts after
an edit. Rebuilding a list resolves current personal usages.

## Trying a resolver in a console application

Create a temporary .NET 10 console app, add a project reference to
`src/DB/DB.csproj` (or install KitchenPC.DB 2.1.1 after publication), and add the
resolver above. Add the `FluentNHibernate.Cfg.Db` namespace and construct
`databaseConfiguration` using
`PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString)`.
Use a development database with public and owned hidden recipes, then:

1. Initialize a modeler-enabled context and create a user context for the owner.
2. Aggregate public recipe IDs and verify the resolver is never called.
3. Aggregate a public ID and a hidden owned ID; verify the summed ingredients.
4. Edit the personal recipe's usages in storage and repeat aggregation without
   reinitializing the context; the new quantities should appear.
5. Repeat as another user and verify resolution fails.
6. Verify `context.ModelerProxy.FindRecipe(hiddenId)` remains null throughout.

The implementation was also exercised in a temporary console app against an
isolated PostgreSQL schema; that sample is intentionally not included in the package.
Automated coverage lives in `RecipeResolverTests`, `DBContextCapabilitiesTest`, and
the modeler-loader test in `OwnerSearchTests`. Set `KPC_TEST_POSTGRES` to run the
optional database tests; each run creates and removes its own schema.
