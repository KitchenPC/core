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

Building and Testing
====

Install the .NET 10 SDK, then restore, build, and test from the repository root:

```bash
dotnet restore src/core.slnx
dotnet build src/core.slnx --configuration Release --no-restore
dotnet test src/UnitTests/UnitTests.csproj --configuration Release --no-build --no-restore
```

The build includes `KitchenPC.Core`, `KitchenPC.DB`, and the unit tests.

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

Every push and pull request builds and tests the solution, then creates matching prerelease packages for CI validation. Version tags publish `KitchenPC.Core` and `KitchenPC.DB` to NuGet with the same version. For example:

```bash
git tag -a v1.0.0 -m "KitchenPC 1.0.0"
git push origin v1.0.0
```

NuGet package versions are immutable. Always increment the version for a subsequent release.
