What is KitchenPC?
====

KitchenPC is a free, open-source framework written in C# for working with recipes, shopping lists, and menus.  It provides a standardized data model for representing normalized ingredient and recipe information, and allows ingredient usage to be aggregated across recipes automatically.  The core KitchenPC framework includes:

1. Base classes for describing and working with core recipe-related concepts.
2. A **recipe modeling engine** capable of building sets of recipes that efficiently use a set of ingredients and amounts.
3. A **natural language parser** which can convert human input ("a dozen eggs") to a normalized ingredient usage structure (whole eggs: 12)
4. A **categorization engine** which can take recipe objects and catagorize them as breakfast, lunch, dinner or dessert.  This engine can also derive nutrional information based on USDA data, a taste profile (sweet, savory, spicy, mild) based on ingredients and amounts used, dietary flags (vegetarian, gluten-free, low-calorie, etc) and other aspects of the recipe.
5. An extensible framework to define how data is loaded and saved to a persistence mechanism, such as a SQL database or full-text search engine.

Documentation
====

The [KitchenPC developer Wiki](https://github.com/KitchenPC/core/wiki) documents the current API,
architecture, setup, and extension points. Start with these guides:

1. [Getting started](https://github.com/KitchenPC/core/wiki/Getting-Started)
2. [Contexts and configuration](https://github.com/KitchenPC/core/wiki/Contexts-and-Configuration)
3. [Recipes and search](https://github.com/KitchenPC/core/wiki/Recipes-and-Search)
4. [Ingredient parsing and units](https://github.com/KitchenPC/core/wiki/Ingredient-Parsing-and-Units)
5. [Shopping lists and aggregation](https://github.com/KitchenPC/core/wiki/Shopping-Lists-and-Aggregation)
6. [PostgreSQL and database provisioning](https://github.com/KitchenPC/core/wiki/PostgreSQL-and-Database-Provisioning)
7. [ASP.NET Core integration](https://github.com/KitchenPC/core/wiki/ASP.NET-Core-Integration)

Example applications and a small static data snapshot are available in the
[KitchenPC Samples repository](https://github.com/KitchenPC/Samples).

Batched recipe resolution (2.1.1)
====

An optional `.RecipeResolver(adapter => new PersonalRecipeResolver(adapter))` factory
resolves aggregation cache misses in one batch with the current user identity.
Public recipes stay in memory; personal recipes stay out of the modeler and can be
loaded fresh for shopping lists and printing. See the [configuration, authorization,
and sample guide](docs/recipe-resolvers.md).

Building and Testing
====

Install the .NET 8 and .NET 10 SDKs, then restore, build, and test from the repository root:

```bash
dotnet restore src/core.slnx
dotnet build src/core.slnx --configuration Release --no-restore
dotnet test src/UnitTests/UnitTests.csproj --configuration Release --no-build --no-restore
```

The build includes `KitchenPC.Core`, `KitchenPC.DB`, `KitchenPC.Core.AspNetCore`, and the unit tests.

`KitchenPC.Core` contains the engine and static context. Add `KitchenPC.DB` when using PostgreSQL,
and add `KitchenPC.Core.AspNetCore` only when registering a context with ASP.NET Core dependency
injection. Applications can connect KitchenPC to standard Microsoft logging through either context
builder:

```csharp
var context = DBContext.Configure
   .Logging(loggerFactory)
   .Adapter(/* database adapter configuration */)
   .Identity(() => AuthIdentity.Anonymous)
   .Create();
```

Database Schema Naming
====

The PostgreSQL persistence adapter uses `shoppingingredients` as the physical table name for the
ingredient catalog. This legacy name is retained for compatibility with the KitchenPC website.
Public domain types and provisioning data continue to use the simpler `Ingredient` and
`Ingredients` terminology; those names describe application data rather than database tables.

`DBContext.InitializeStore()` recreates the KitchenPC schema and deletes existing KitchenPC data.
Use it only with a new database or when replacing all existing data is intentional. See the
[KitchenPC Samples repository](https://github.com/KitchenPC/Samples) for a PostgreSQL initializer
and a small sample dataset.

Optional DBContext Capabilities
====

`DBContext` initializes its autocomplete index, ingredient-text parser, and recipe-modeler graph
by default. Applications that do not use every feature can select only the in-memory capabilities
they need while retaining ordinary database-backed operations:

```csharp
var context = DBContext.Configure
   .Adapter(/* database adapter configuration */)
   .Capabilities(DBContextCapabilities.IngredientParsing)
   .Identity(() => AuthIdentity.Anonymous)
   .Create();
```

The available flags are `IngredientAutocomplete`, `IngredientParsing`, and `RecipeModeler`.
`DBContextCapabilities.All` is the default for backward compatibility. Calling an API whose
capability was not enabled throws `ContextCapabilityNotEnabledException`. Recipe aggregation uses
the in-memory graph when the modeler is enabled and falls back to loading recipes from the database
when it is disabled.

See [DBContext capability profiles](https://github.com/KitchenPC/core/wiki/DBContext-Capability-Profiles)
for capability requirements and
sample-data startup and memory measurements.

Packages and Releases
====

Every push and pull request builds and tests the solution, then creates matching prerelease packages
for CI validation. Version tags publish `KitchenPC.Core`, `KitchenPC.DB`, and
`KitchenPC.Core.AspNetCore` to NuGet with the same version. For example:

```bash
git tag -a v2.0.0 -m "KitchenPC 2.0.0"
git push origin v2.0.0
```

NuGet package versions are immutable. Always increment the version for a subsequent release.


Owner-aware recipe search (2.1.0)
====

`RecipeQuery.UserId` is an optional owner filter. The existing search providers apply
visibility using the context's trusted `AuthIdentity`, independently of that filter:

- Anonymous callers see public (`Hidden == false`) recipes only.
- Authenticated callers see public recipes plus their own hidden recipes.
- `UserId` narrows that visible set to the specified owner. Set it to the authenticated
  user's ID for My Recipes; another user's ID never grants access to their hidden data.
- Empty keywords and `Photos = RecipeQuery.PhotoFilter.All` include image-less recipes.
  Filtering precedes counts, sorting, and paging. Recipe ID breaks sort ties.

Both PostgreSQL/NHSearch and XML/StaticSearch implement the same visibility rules.
The caller must obtain identity from its authentication system, not from the query.
`Recipe.OwnerId`/`OwnerAlias` and `RecipeBrief.OwnerId`/`Author` contain the owner ID
and public username (`users.alias`). `Hidden` is available on both responses.
Ownerless records retain `Guid.Empty` in the public DTOs; nullable ownership is
preserved in the database and provisioning DTO. The DB owner mapping is read-only
and loads only the public ID/alias, not credentials or profile data.

Provisioning includes owner IDs and the public alias snapshot for XML searches.
Database import preserves the ID and resolves the current alias from `users`;
accounts with those IDs must already exist in the destination. Accounts and their
credentials are not copied by the recipe exporter. Older XML without owner fields
continues to load as ownerless data. Ingredient reads and writes preserve display
order, amount ranges, forms, sections, and prep notes.

This release provides the core groundwork for KitchenPC/Website#73. Hosts still
own recipe creation, mutation authorization, sharing, and deletion; these search
rules are not a universal authorization layer for other recipe-ID APIs. A host
must not use the legacy CreateRecipe operation as a private-recipe write API.
For fresh shopping aggregation, the existing `DBContext` with
`DBContextCapabilities.RecipeModeler` disabled reads usages from the database and
performs form-aware conversions. A separately configured context can use that path
without changing the modeler-backed context. Website's freshness/access integration
remains a separate slice; no new core aggregation API is required.

PostgreSQL search and round-trip integration tests run when `KPC_TEST_POSTGRES`
contains an Npgsql connection string. They create and drop a uniquely named schema,
leaving existing tables alone. CI and release workflows supply disposable PostgreSQL.
The Website migration adds a leading `recipes.ownerid` btree only when no suitable
index exists; it is not created implicitly by library initialization.
