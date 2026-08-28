using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Menus;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.NLP;
using KitchenPC.Core.Provisioning;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoreIngredientUsage = KitchenPC.Core.Ingredients.IngredientUsage;
using NlpIngredientNode = KitchenPC.Core.NLP.IngredientNode;

namespace KitchenPC.UnitTests;

[TestClass]
[DoNotParallelize]
public class DBContextCapabilitiesTest
{
   [TestMethod]
   public void DefaultsToAllCapabilities()
   {
      var context = DBContext.Configure.Create();

      Assert.AreEqual(DBContextCapabilities.All, context.Capabilities);
   }

   [TestMethod]
   public void CapabilitiesArePreservedByUserContext()
   {
      var context = DBContext
         .Configure.Capabilities(DBContextCapabilities.IngredientParsing)
         .Identity(() => AuthIdentity.Anonymous)
         .Create();

      var userContext = (DBContext)
         context.AsUserContext(new AuthIdentity(Guid.NewGuid(), "Sample user"));

      Assert.AreEqual(DBContextCapabilities.IngredientParsing, userContext.Capabilities);
   }

   [DataTestMethod]
   [DataRow(DBContextCapabilities.None, false, false, false)]
   [DataRow(DBContextCapabilities.IngredientAutocomplete, true, false, false)]
   [DataRow(DBContextCapabilities.IngredientParsing, false, true, false)]
   [DataRow(DBContextCapabilities.RecipeModeler, false, false, true)]
   [DataRow(
      DBContextCapabilities.IngredientAutocomplete | DBContextCapabilities.IngredientParsing,
      true,
      true,
      false
   )]
   [DataRow(
      DBContextCapabilities.IngredientAutocomplete | DBContextCapabilities.RecipeModeler,
      true,
      false,
      true
   )]
   [DataRow(
      DBContextCapabilities.IngredientParsing | DBContextCapabilities.RecipeModeler,
      false,
      true,
      true
   )]
   [DataRow(DBContextCapabilities.All, true, true, true)]
   public void InitializesOnlySelectedCapabilities(
      DBContextCapabilities capabilities,
      bool autocomplete,
      bool parsing,
      bool modeler
   )
   {
      var (context, adapter) = CreateContext(capabilities);

      context.Initialize();

      Assert.AreEqual(1, adapter.CallCount("Initialize"));
      Assert.AreEqual(autocomplete ? 1 : 0, adapter.CallCount("LoadIngredientsForIndex"));
      Assert.AreEqual(modeler ? 1 : 0, adapter.CallCount("LoadRecipeGraph"));
      Assert.AreEqual(modeler ? 1 : 0, adapter.CallCount("LoadIngredientGraph"));
      Assert.AreEqual(modeler ? 1 : 0, adapter.CallCount("LoadRatingGraph"));
      Assert.AreEqual(parsing ? 1 : 0, adapter.CallCount("get_IngredientLoader"));
      Assert.AreEqual(parsing ? 1 : 0, adapter.CallCount("get_UnitLoader"));
      Assert.AreEqual(parsing ? 1 : 0, adapter.CallCount("get_FormLoader"));
      Assert.AreEqual(parsing ? 1 : 0, adapter.CallCount("get_PrepLoader"));
      Assert.AreEqual(parsing ? 1 : 0, adapter.CallCount("get_AnomalyLoader"));
   }

   [TestMethod]
   public void DisabledCapabilitiesThrowClearErrors()
   {
      var (context, _) = CreateContext(DBContextCapabilities.None);
      context.Initialize();

      AssertCapabilityError(
         DBContextCapabilities.IngredientAutocomplete,
         () => context.AutocompleteIngredient("egg")
      );
      AssertCapabilityError(
         DBContextCapabilities.IngredientParsing,
         () => context.ParseIngredientUsage("12 eggs")
      );
      AssertCapabilityError(
         DBContextCapabilities.RecipeModeler,
         () =>
         {
            _ = context.ModelerProxy;
         }
      );
      AssertCapabilityError(
         DBContextCapabilities.RecipeModeler,
         () =>
         {
            _ = context.Modeler;
         }
      );
   }

   [TestMethod]
   public void AggregatesRecipesFromDatabaseWhenModelerIsDisabled()
   {
      var ingredient = new Ingredient(Guid.NewGuid(), "eggs") { ConversionType = UnitType.Unit };
      var recipe = new Recipe(Guid.NewGuid(), "Eggs", null, null)
      {
         Ingredients =
         [
            new CoreIngredientUsage
            {
               Ingredient = ingredient,
               Amount = new Amount(12, Units.Unit),
            },
         ],
      };
      var (context, adapter) = CreateContext(DBContextCapabilities.IngredientParsing);
      adapter.Recipes = [recipe];
      context.Initialize();

      var result = context.AggregateRecipes(recipe.Id).Single();

      Assert.AreEqual("eggs", result.Ingredient.Name);
      Assert.AreEqual(new Amount(12, Units.Unit), result.Amount);
      Assert.AreEqual(1, adapter.CallCount("ReadRecipes"));
      Assert.AreEqual(0, adapter.CallCount("LoadRecipeGraph"));
   }

   [TestMethod]
   public void DatabaseRecipeAggregationNormalizesIngredientForms()
   {
      var ingredient = new Ingredient(Guid.NewGuid(), "flour") { ConversionType = UnitType.Weight };
      var cupForm = new IngredientForm
      {
         IngredientId = ingredient.Id,
         FormUnitType = Units.Cup,
         FormAmount = new Amount(4, Units.Gram),
      };
      var recipe = new Recipe(Guid.NewGuid(), "Bread", null, null)
      {
         Ingredients =
         [
            new CoreIngredientUsage
            {
               Ingredient = ingredient,
               Form = cupForm,
               Amount = new Amount(2, Units.Cup),
            },
         ],
      };
      var (context, adapter) = CreateContext(DBContextCapabilities.None);
      adapter.Recipes = [recipe];
      context.Initialize();

      var result = context.AggregateRecipes(recipe.Id).Single();

      Assert.AreEqual(UnitConverter.Convert(new Amount(8, Units.Gram), Units.Ounce), result.Amount);
   }

   [TestMethod]
   public void DatabaseRecipeAggregationExcludesWater()
   {
      var water = new Ingredient(ShoppingList.GUID_WATER, "water")
      {
         ConversionType = UnitType.Volume,
      };
      var recipe = new Recipe(Guid.NewGuid(), "Tea", null, null)
      {
         Ingredients =
         [
            new CoreIngredientUsage { Ingredient = water, Amount = new Amount(1, Units.Cup) },
         ],
      };
      var (context, adapter) = CreateContext(DBContextCapabilities.None);
      adapter.Recipes = [recipe];
      context.Initialize();

      var result = context.AggregateRecipes(recipe.Id);

      Assert.AreEqual(0, result.Count);
   }

   [TestMethod]
   public void ModelerAggregationDoesNotRequireAutocompleteIndex()
   {
      var recipeId = Guid.NewGuid();
      var ingredientId = Guid.NewGuid();
      var (context, adapter) = CreateContext(DBContextCapabilities.RecipeModeler);
      adapter.RecipeBindings = [new RecipeBinding { Id = recipeId, Tags = RecipeTags.None }];
      adapter.IngredientBindings =
      [
         IngredientBinding.Create(
            ingredientId,
            recipeId,
            2,
            Units.Unit,
            UnitType.Unit,
            0,
            null,
            null,
            null,
            "eggs"
         ),
      ];
      context.Initialize();

      var result = context.AggregateRecipes(recipeId).Single();

      Assert.AreEqual("eggs", result.Ingredient.Name);
      Assert.AreEqual(new Amount(2, Units.Unit), result.Amount);
      Assert.AreEqual(0, adapter.CallCount("LoadIngredientsForIndex"));
      Assert.AreEqual(0, adapter.CallCount("ReadRecipes"));
   }

   [TestMethod]
   public void RejectsUnknownCapabilities()
   {
      Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
         DBContext.Configure.Capabilities((DBContextCapabilities)8)
      );
   }

   private static void AssertCapabilityError(DBContextCapabilities expected, Action action)
   {
      var exception = Assert.ThrowsException<ContextCapabilityNotEnabledException>(action);
      Assert.AreEqual(expected, exception.Capability);
   }

   private static (DBContext Context, TrackingAdapter Adapter) CreateContext(
      DBContextCapabilities capabilities
   )
   {
      var adapter = DispatchProxy.Create<IDBAdapter, TrackingAdapter>();
      var tracker = (TrackingAdapter)(object)adapter;
      var context = DBContext
         .Configure.Adapter(new AdapterBuilder(adapter))
         .Capabilities(capabilities)
         .Identity(() => AuthIdentity.Anonymous)
         .Create();

      return (context, tracker);
   }

   private sealed class AdapterBuilder : IConfigurationBuilder<IDBAdapter>
   {
      private readonly IDBAdapter adapter;

      public AdapterBuilder(IDBAdapter adapter)
      {
         this.adapter = adapter;
      }

      public IDBAdapter Create() => adapter;
   }
}

public class TrackingAdapter : DispatchProxy
{
   private readonly List<string> calls = [];

   public Recipe[] Recipes { get; set; } = [];
   public RecipeBinding[] RecipeBindings { get; set; } = [];
   public IngredientBinding[] IngredientBindings { get; set; } = [];

   public int CallCount(string name) => calls.Count(call => call == name);

   protected override object Invoke(MethodInfo targetMethod, object[] args)
   {
      calls.Add(targetMethod.Name);

      return targetMethod.Name switch
      {
         "Initialize" => null,
         "LoadIngredientsForIndex" => Array.Empty<IngredientSource>(),
         "LoadRecipeGraph" => RecipeBindings,
         "LoadIngredientGraph" => IngredientBindings,
         "LoadRatingGraph" => Array.Empty<RatingBinding>(),
         "get_IngredientLoader" => new EmptySynonymLoader<NlpIngredientNode>(),
         "get_UnitLoader" => new EmptySynonymLoader<UnitNode>(),
         "get_FormLoader" => new EmptySynonymLoader<FormNode>(),
         "get_PrepLoader" => new EmptySynonymLoader<PrepNode>(),
         "get_AnomalyLoader" => new EmptySynonymLoader<AnomalousNode>(),
         "ReadRecipes" => Recipes,
         "Export" => new DataStore(),
         "Import" or "InitializeStore" => null,
         _ => throw new NotSupportedException(targetMethod.Name),
      };
   }
}

internal sealed class EmptySynonymLoader<T> : ISynonymLoader<T>
{
   public IEnumerable<T> LoadSynonyms() => Array.Empty<T>();

   public Pairings LoadFormPairings() => new();
}
