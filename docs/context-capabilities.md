# DBContext capability profiles

`DBContext` has three independently configurable in-memory capabilities:

| Capability | In-memory data | Required by |
| --- | --- | --- |
| `IngredientAutocomplete` | Ingredient-name substring index | `AutocompleteIngredient` |
| `IngredientParsing` | Ingredient, unit, form, prep-note, anomaly, and numeric grammar indexes | `ParseIngredient` and `ParseIngredientUsage` |
| `RecipeModeler` | Ratings, recipes, ingredients, tags, and suggestion graph | Recipe modeling APIs |

Database-backed recipe search, recipe details, ingredients, menus, queues, and shopping lists do
not require these flags. Recipe aggregation uses the modeler graph when available and loads the
requested recipes from the database when the modeler is disabled.

All capabilities are enabled by default for compatibility. Applications can select a smaller
profile during configuration:

```csharp
var context = DBContext.Configure
   .Adapter(/* database adapter configuration */)
   .Capabilities(DBContextCapabilities.IngredientParsing)
   .Identity(() => AuthIdentity.Anonymous)
   .Create();
```

Calling an API whose capability was not enabled throws
`ContextCapabilityNotEnabledException`, whose `Capability` property identifies the missing flag.

## Sample-data measurements

The following measurements compare the previous implementation with each capability profile. They
were collected on Linux with .NET 10 and PostgreSQL 17 using the KitchenPC sample snapshot (2,707
ingredients and 30 recipes). Each value is the median of three fresh processes after forced garbage
collection. Working set includes the runtime, NHibernate, and PostgreSQL client infrastructure.

| Profile | Startup | Managed memory | Working set |
| --- | ---: | ---: | ---: |
| Previous implementation | 1,572 ms | 132.9 MiB | 263.1 MiB |
| `All` | 1,567 ms | 133.0 MiB | 262.5 MiB |
| Parsing + modeler | 1,072 ms | 71.5 MiB | 185.7 MiB |
| Parsing only | 993 ms | 71.4 MiB | 180.3 MiB |
| Autocomplete only | 1,049 ms | 66.6 MiB | 183.4 MiB |
| Modeler only | 663 ms | 5.2 MiB | 114.0 MiB |
| Database only | 532 ms | 5.0 MiB | 111.8 MiB |

The parsing-only profile intended for the public sample website reduced startup by approximately
37%, managed memory by 46%, and process working set by 31% compared with the previous all-feature
initialization on this dataset. Production-sized recipe data will make the modeler profile more
expensive than this small snapshot suggests. The private KitchenPC website should be measured
separately with production-scale data when it adopts the parsing-plus-modeler profile.

An end-to-end PostgreSQL check also used the parsing-only profile to search and load a recipe,
aggregate its six ingredients through the database fallback, parse `12 eggs`, add the recipe to the
default shopping list, reload the persisted items, and remove them. The test used a freshly
provisioned database and removed it afterward.

## Legacy NLP database views

The Core and DB runtime loaders do not query the legacy `FormSynonymsForNLP`,
`UnitSynonymsForNLP`, `PrepNotesForNLP`, or `AnomaliesForNLP` views. The private Website repository
still defines those views and grants access to the `Website`, `IngredientCzar`, and `Indexer` roles
in its database installation script, so they should not be removed without separately auditing
those external consumers.

The similarly named `shoppingingredientsfornlp` object remains part of the Core persistence model:
the default schema creates it as a table containing the weight, volume, and unit form pairings used
by NLP. It is not removed or renamed by the capability work.
