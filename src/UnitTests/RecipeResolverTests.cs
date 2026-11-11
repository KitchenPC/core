using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;
using KitchenPC.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dto = KitchenPC.Core.Provisioning.DTO;
using IngredientUsage = KitchenPC.Core.Ingredients.IngredientUsage;

namespace KitchenPC.UnitTests;

[TestClass]
[DoNotParallelize]
public class RecipeResolverTests
{
   private static readonly Guid Public = Guid.NewGuid();
   private static readonly Guid Personal = Guid.NewGuid();
   private static readonly Guid Owner = Guid.NewGuid();
   private static readonly Guid IngredientId = Guid.NewGuid();

   private static (DBContext Context, TrackingAdapter Adapter) Setup()
   {
      var (context, adapter) = DBContextCapabilitiesTest.CreateContext(DBContextCapabilities.All);
      adapter.RecipeBindings =
      [
         new RecipeBinding { Id = Public, Tags = RecipeTags.None },
         new RecipeBinding
         {
            Id = Personal,
            Hidden = true,
            Tags = RecipeTags.None,
         },
      ];
      adapter.IngredientBindings = new[] { Public, Personal }
         .Select(id =>
            IngredientBinding.Create(
               IngredientId,
               id,
               2,
               Units.Ounce,
               UnitType.Weight,
               0,
               null,
               null,
               null,
               "flour"
            )
         )
         .ToArray();
      adapter.RatingBindings = new[] { Public, Personal }
         .Select(id => new RatingBinding
         {
            RecipeId = id,
            UserId = Owner,
            Rating = 5,
         })
         .ToArray();
      context.Initialize();
      return ((DBContext)context.AsUserContext(new AuthIdentity(Owner, "owner")), adapter);
   }

   private static Recipe Recipe(float amount = 1) =>
      new(Personal, "Personal bread", null, null)
      {
         OwnerId = Owner,
         Hidden = true,
         Ingredients =
         [
            new IngredientUsage
            {
               Ingredient = new Ingredient(IngredientId, "flour")
               {
                  ConversionType = UnitType.Weight,
               },
               Amount = new Amount(amount, Units.Ounce),
            },
         ],
      };

   [TestMethod]
   public async Task PublicOnlyNeverCallsResolverOrDatabaseAndHiddenNeverEntersModeler()
   {
      var (context, adapter) = Setup();
      adapter.Resolver = new Resolver(
         (_, _, _) => throw new AssertFailedException("Unexpected resolution")
      );
      Assert.IsNull(context.ModelerProxy.FindRecipe(Personal));
      Assert.IsFalse(
         context.ModelerProxy.FindRecipe(Public).Suggestions.Any(r => r.RecipeId == Personal)
      );
      var result = (await context.AggregateRecipesAsync([Public])).Single();
      Assert.AreEqual(new Amount(2, Units.Ounce), result.Amount);
      Assert.AreEqual(0, adapter.CallCount("ReadRecipesAsync"));
      Assert.AreEqual(0, adapter.CallCount("ReadIngredient"));
   }

   [TestMethod]
   public async Task BatchesDistinctMissesPreservesMultiplicityAndDoesNotCacheEdits()
   {
      var (context, adapter) = Setup();
      var recipe = Recipe();
      var calls = 0;
      adapter.Resolver = new Resolver(
         async (identity, ids, token) =>
         {
            await Task.Yield();
            Assert.AreEqual(Owner, identity.UserId);
            CollectionAssert.AreEqual(new[] { Personal }, ids);
            calls++;
            return [recipe];
         }
      );
      Assert.AreEqual(
         new Amount(4, Units.Ounce),
         (await context.AggregateRecipesAsync([Public, Personal, Personal])).Single().Amount
      );
      recipe.Ingredients[0].Amount = new Amount(5, Units.Ounce);
      Assert.AreEqual(
         new Amount(7, Units.Ounce),
         context.AggregateRecipes(Public, Personal).Single().Amount
      );
      Assert.AreEqual(2, calls);
      Assert.IsNull(context.ModelerProxy.FindRecipe(Personal));
   }

   [TestMethod]
   public void UnconfiguredResolverPreservesSkippingMisses()
   {
      var (context, _) = Setup();
      Assert.AreEqual(0, context.AggregateRecipes(Personal, Guid.NewGuid()).Count);
      Assert.AreEqual(
         new Amount(2, Units.Ounce),
         context.AggregateRecipes(Public, Personal).Single().Amount
      );
   }

   [TestMethod]
   [DataRow(0)]
   [DataRow(1)]
   [DataRow(2)]
   [DataRow(3)]
   [DataRow(4)]
   public async Task InvalidResolverResponseFailsEntireAggregation(int invalid)
   {
      var (context, adapter) = Setup();
      adapter.Resolver = new Resolver(
         (_, _, _) =>
            Task.FromResult<Recipe[]>(
               invalid switch
               {
                  0 => null,
                  1 => [],
                  2 => [Recipe(), Recipe()],
                  3 => [new Recipe(Guid.NewGuid(), "unexpected", null, null)],
                  _ => [null],
               }
            )
      );
      await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
         context.AggregateRecipesAsync([Public, Personal])
      );
   }

   [TestMethod]
   public async Task IdentityIsPerCallAndAuthorizationFailuresPropagate()
   {
      var (context, adapter) = Setup();
      adapter.Resolver = new Resolver(
         (identity, _, _) =>
         {
            if (identity.UserId != Owner)
               throw new UnauthorizedAccessException();
            return Task.FromResult(new[] { Recipe() });
         }
      );
      Assert.AreEqual(1, (await context.AggregateRecipesAsync([Personal])).Count);
      var other = context.AsUserContext(new AuthIdentity(Guid.NewGuid(), "other"));
      await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() =>
         other.AggregateRecipesAsync([Personal])
      );
      await Assert.ThrowsExactlyAsync<UnauthorizedAccessException>(() =>
         context.AsUserContext(AuthIdentity.Anonymous).AggregateRecipesAsync([Personal])
      );
   }

   [TestMethod]
   public async Task CancellationIsForwardedAndEmptyInputDoesNotResolve()
   {
      var (context, adapter) = Setup();
      using var cancellation = new CancellationTokenSource();
      adapter.Resolver = new Resolver(
         (_, _, token) =>
         {
            Assert.AreEqual(cancellation.Token, token);
            cancellation.Cancel();
            return Task.FromResult(new[] { Recipe() });
         }
      );
      Assert.AreEqual(0, (await context.AggregateRecipesAsync([])).Count);
      await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
         context.AggregateRecipesAsync([Personal], cancellation.Token)
      );
      await Assert.ThrowsExactlyAsync<OperationCanceledException>(() =>
         context.AggregateRecipesAsync([Public], cancellation.Token)
      );
   }

   [TestMethod]
   public void MixedRecipesConvertFormsAndRangesAndExcludeWater()
   {
      var (context, adapter) = Setup();
      var recipe = Recipe();
      var usage = recipe.Ingredients.Single();
      usage.Amount = new Amount(1, 2, Units.Cup);
      usage.Form = new IngredientForm
      {
         FormUnitType = Units.Cup,
         FormAmount = new Amount(100, Units.Gram),
      };
      recipe.AddIngredient(
         new IngredientUsage
         {
            Ingredient = new Ingredient(ShoppingList.GUID_WATER, "water")
            {
               ConversionType = UnitType.Volume,
            },
            Amount = new Amount(1, Units.Cup),
         }
      );
      adapter.Resolver = new Resolver((_, _, _) => Task.FromResult(new[] { recipe }));
      var result = context.AggregateRecipes(Public, Personal).Single();
      Assert.AreEqual(
         2 + UnitConverter.Convert(100, Units.Gram, Units.Ounce),
         result.Amount.SizeLow.Value,
         0.0001f
      );
      Assert.AreEqual(
         2 + UnitConverter.Convert(200, Units.Gram, Units.Ounce),
         result.Amount.SizeHigh,
         0.0001f
      );
      usage.Amount = null;
      Assert.IsNull(context.AggregateRecipes(Public, Personal).Single().Amount);
   }

   [TestMethod]
   public void AdapterFactoryReceivesTheConfiguredAdapter()
   {
      DatabaseAdapter supplied = null;
      var resolver = new Resolver((_, _, _) => Task.FromResult(Array.Empty<Recipe>()));
      var adapter = DatabaseAdapter
         .Configure.RecipeResolver(value =>
         {
            supplied = value;
            return resolver;
         })
         .Create();
      Assert.AreSame(adapter, supplied);
      Assert.AreSame(resolver, adapter.RecipeResolver);
   }

   [TestMethod]
   public void ShoppingListCreationAndUpdateResolveFreshRecipesBeforePersistence()
   {
      var (context, adapter) = Setup();
      var recipe = Recipe();
      adapter.Resolver = new Resolver((_, _, _) => Task.FromResult(new[] { recipe }));
      var created = context.CreateShoppingList(
         "test",
         [KitchenPC.Core.Recipes.Recipe.FromId(Public), recipe],
         [],
         [],
         []
      );
      Assert.AreEqual(new Amount(3, Units.Ounce), created.List.Single().Amount);
      recipe.Ingredients[0].Amount = new Amount(4, Units.Ounce);
      var updated = context.UpdateShoppingList(
         created.List,
         [new ShoppingListUpdateCommand { NewRecipe = recipe }]
      );
      Assert.AreEqual(new Amount(4, Units.Ounce), updated.List.Single().Amount);
      adapter.Resolver = new Resolver((_, _, _) => throw new UnauthorizedAccessException());
      Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
         context.CreateShoppingList("denied", [recipe], [], [], [])
      );
      Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
         context.UpdateShoppingList(
            created.List,
            [new ShoppingListUpdateCommand { NewRecipe = recipe }]
         )
      );
      Assert.AreEqual(1, adapter.CallCount("CreateShoppingList"));
      Assert.AreEqual(1, adapter.CallCount("UpdateShoppingList"));
   }

   [TestMethod]
   public async Task StaticContextExcludesHiddenRecipesAndSupportsResolution()
   {
      var directory = System.IO.Path.Combine(
         System.IO.Path.GetTempPath(),
         "kpc-resolver-" + Guid.NewGuid()
      );
      System.IO.Directory.CreateDirectory(directory);
      try
      {
         var serializer = new System.Xml.Serialization.XmlSerializer(
            typeof(KitchenPC.Core.Provisioning.DataStore)
         );
         var form = Guid.NewGuid();
         var store = new KitchenPC.Core.Provisioning.DataStore
         {
            Recipes =
            [
               new Dto.Recipes { RecipeId = Personal, Hidden = true },
               new Dto.Recipes { RecipeId = Public },
            ],
            RecipeMetadata = new[] { Personal, Public }
               .Select(id => new Dto.RecipeMetadata { RecipeId = id })
               .ToList(),
            RecipeIngredients = new[] { Personal, Public }
               .Select(id => new Dto.RecipeIngredients
               {
                  RecipeId = id,
                  IngredientId = IngredientId,
                  IngredientFormId = form,
                  Unit = Units.Ounce,
                  Qty = 2,
               })
               .ToList(),
            RecipeRatings = new[] { Personal, Public }
               .Select(id => new Dto.RecipeRatings
               {
                  RecipeId = id,
                  UserId = Owner,
                  Rating = 5,
               })
               .ToList(),
            Ingredients =
            [
               new Dto.Ingredients
               {
                  IngredientId = IngredientId,
                  DisplayName = "flour",
                  ConversionType = UnitType.Weight,
               },
            ],
            IngredientForms =
            [
               new Dto.IngredientForms
               {
                  IngredientFormId = form,
                  IngredientId = IngredientId,
                  UnitType = Units.Ounce,
                  FormUnit = Units.Gram,
                  FormAmount = 28.3495f,
               },
            ],
            IngredientMetadata = [],
            NlpAnomalousIngredients = [],
            NlpDefaultPairings = [],
            NlpFormSynonyms = [],
            NlpIngredientSynonyms = [],
            NlpPrepNotes = [],
            NlpUnitSynonyms = [],
         };
         var hidden = store.Recipes.First();
         var loader = new StaticModelerLoader(store);
         Assert.AreEqual(1, loader.LoadRecipeGraph().Count());
         Assert.AreEqual(1, loader.LoadIngredientGraph().Count());
         Assert.AreEqual(1, loader.LoadRatingGraph().Count());
         Assert.IsFalse(loader.LoadRecipeGraph().Any(r => r.Id == hidden.RecipeId));
         Assert.IsFalse(loader.LoadIngredientGraph().Any(r => r.RecipeId == hidden.RecipeId));
         Assert.IsFalse(loader.LoadRatingGraph().Any(r => r.RecipeId == hidden.RecipeId));
         using (
            var writer = System.IO.File.Create(System.IO.Path.Combine(directory, "KPCData.xml"))
         )
            serializer.Serialize(writer, store);
         var recipe = Recipe();
         recipe.Id = hidden.RecipeId;
         var context = StaticContext
            .Configure.DataDirectory(directory)
            .Identity(() => new AuthIdentity(Owner, "owner"))
            .RecipeResolver(
               new Resolver(
                  (_, ids, _) =>
                  {
                     CollectionAssert.AreEqual(new[] { hidden.RecipeId }, ids);
                     return Task.FromResult(new[] { recipe });
                  }
               )
            )
            .Create();
         context.Initialize();
         Assert.IsNull(context.ModelerProxy.FindRecipe(hidden.RecipeId));
         Assert.AreEqual(
            new Amount(1, Units.Ounce),
            (await context.AggregateRecipesAsync([hidden.RecipeId])).Single().Amount
         );
      }
      finally
      {
         System.IO.Directory.Delete(directory, true);
      }
   }

   [TestMethod]
   public async Task MultipleMissesAreResolvedInOneBatch()
   {
      var (context, adapter) = Setup();
      var other = Recipe(4);
      other.Id = Guid.NewGuid();
      var calls = 0;
      adapter.Resolver = new Resolver(
         (_, ids, _) =>
         {
            calls++;
            CollectionAssert.AreEquivalent(new[] { Personal, other.Id }, ids);
            return Task.FromResult(new[] { other, Recipe() });
         }
      );
      Assert.AreEqual(
         new Amount(11, Units.Ounce),
         (await context.AggregateRecipesAsync([Public, Personal, other.Id, other.Id]))
            .Single()
            .Amount
      );
      Assert.AreEqual(1, calls);
   }

   [TestMethod]
   public void MixedRecipesUseResolvedUnitWeightForWeightToCountConversion()
   {
      var (context, adapter) = DBContextCapabilitiesTest.CreateContext(
         DBContextCapabilities.RecipeModeler
      );
      adapter.RecipeBindings = [new RecipeBinding { Id = Public, Tags = RecipeTags.None }];
      adapter.IngredientBindings =
      [
         IngredientBinding.Create(
            IngredientId,
            Public,
            2,
            Units.Unit,
            UnitType.Unit,
            50,
            null,
            null,
            null,
            "eggs"
         ),
      ];
      var recipe = Recipe();
      var usage = recipe.Ingredients.Single();
      usage.Ingredient.ConversionType = UnitType.Unit;
      usage.Ingredient.UnitWeight = 50;
      usage.Form = new IngredientForm
      {
         FormUnitType = Units.Gram,
         FormAmount = new Amount(1, Units.Gram),
      };
      usage.Amount = new Amount(50, 100, Units.Gram);
      adapter.Resolver = new Resolver((_, _, _) => Task.FromResult(new[] { recipe }));
      context.Initialize();
      Assert.AreEqual(
         new Amount(3, 4, Units.Unit),
         context.AggregateRecipes(Public, Personal).Single().Amount
      );
   }

   [TestMethod]
   public void PublicAggregationDoesNotRequireAnIdentityProvider()
   {
      var (context, adapter) = DBContextCapabilitiesTest.CreateContext(
         DBContextCapabilities.RecipeModeler
      );
      context.GetIdentity = null;
      adapter.RecipeBindings = [new RecipeBinding { Id = Public, Tags = RecipeTags.None }];
      context.Initialize();
      Assert.AreEqual(0, context.AggregateRecipes(Public, Personal).Count);
      adapter.Resolver = new Resolver(
         (_, _, _) => throw new AssertFailedException("Unexpected resolution")
      );
      Assert.AreEqual(0, context.AggregateRecipes(Public).Count);
   }

   private sealed class Resolver(
      Func<AuthIdentity, Guid[], CancellationToken, Task<Recipe[]>> resolve
   ) : IBatchedRecipeResolver
   {
      public Task<Recipe[]> ResolveAsync(
         AuthIdentity identity,
         Guid[] recipeIds,
         CancellationToken cancellationToken = default
      ) => resolve(identity, recipeIds, cancellationToken);
   }
}
