using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FluentNHibernate.Cfg.Db;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Provisioning;
using KitchenPC.Core.Recipes;
using KitchenPC.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using Db = KitchenPC.DB.Models;
using Dto = KitchenPC.Core.Provisioning.DTO;

namespace KitchenPC.UnitTests;

[TestClass]
[DoNotParallelize]
public class OwnerSearchTests
{
   private static readonly Guid Owner = new("11111111-1111-1111-1111-111111111111");
   private static readonly Guid Other = new("22222222-2222-2222-2222-222222222222");
   private static readonly Guid Ingredient = new("33333333-3333-3333-3333-333333333333");
   private static readonly Guid OtherIngredient = new("44444444-4444-4444-4444-444444444444");
   private static DataStore store;
   private static DatabaseAdapter adapter;
   private static NpgsqlConnection connection;
   private static string schema;

   [ClassInitialize]
   public static void Initialize(TestContext _)
   {
      store = new DataStore
      {
         Recipes = [],
         RecipeIngredients = [],
         RecipeMetadata = [],
      };
      for (var i = 0; i < 205; i++)
      {
         var recipe = new Dto.Recipes
         {
            RecipeId = new Guid(i + 1, 0, 0, new byte[8]),
            OwnerId = Owner,
            OwnerAlias = "owner",
            Hidden = i % 2 == 0,
            Title = $"Recipe {i / 3:D3}",
            Description = i % 2 == 0 ? "match" : "other",
            PrepTime = (short)(i % 10 + 1),
            CookTime = (short)(10 - i % 10),
            Rating = (short)(i % 2 == 0 ? 5 : 1),
            ImageUrl = i % 2 == 0 ? "photo.jpg" : null,
            ServingSize = 4,
            DateEntered = DateTime.UtcNow,
         };
         store.Recipes.Add(recipe);
         store.RecipeIngredients.Add(
            new Dto.RecipeIngredients
            {
               RecipeIngredientId = Guid.NewGuid(),
               RecipeId = recipe.RecipeId,
               IngredientId = i % 2 == 0 ? Ingredient : OtherIngredient,
               Unit = Units.Unit,
               Qty = 2,
               QtyLow = 1,
               PrepNote = "chopped",
               Section = "Main",
               DisplayOrder = 1,
            }
         );
         store.RecipeMetadata.Add(
            new Dto.RecipeMetadata
            {
               RecipeMetadataId = Guid.NewGuid(),
               RecipeId = recipe.RecipeId,
               MealDinner = i % 2 == 0,
               DietGlutenFree = i % 2 == 0,
               NutritionLowFat = i % 2 == 0,
               SkillEasy = i % 2 == 0,
               TasteMildToSpicy = (short)(i % 2 == 0 ? 1 : 5),
               PhotoRes = i % 2 == 0 ? 1024 * 768 : 0,
            }
         );
      }
      foreach (
         var (owner, hidden) in new (Guid?, bool)[]
         {
            (Other, false),
            (Other, true),
            (null, false),
            (null, true),
            (Guid.Empty, true),
         }
      )
         store.Recipes.Add(
            new Dto.Recipes
            {
               RecipeId = Guid.NewGuid(),
               OwnerId = owner,
               OwnerAlias = owner == Other ? "other" : null,
               Hidden = hidden,
               Title = "Outside",
               ServingSize = 4,
               DateEntered = DateTime.UtcNow,
            }
         );

      var configured = Environment.GetEnvironmentVariable("KPC_TEST_POSTGRES");
      if (string.IsNullOrWhiteSpace(configured))
         return;
      connection = new NpgsqlConnection(configured);
      connection.Open();
      schema = "owner_search_" + Guid.NewGuid().ToString("N");
      using (var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", connection))
         command.ExecuteNonQuery();
      var scoped = new NpgsqlConnectionStringBuilder(configured) { SearchPath = schema };
      adapter = DatabaseAdapter
         .Configure.DatabaseConfiguration(
            PostgreSQLConfiguration.PostgreSQL82.ConnectionString(scoped.ConnectionString)
         )
         .Create();
      adapter.Initialize(DBContext.Configure.Capabilities(DBContextCapabilities.None).Create());
      adapter.InitializeStore();
      using var session = adapter.GetSession();
      using var transaction = session.BeginTransaction();
      foreach (var id in new[] { Owner, Other, Guid.Empty })
         session
            .CreateSQLQuery("INSERT INTO users (userid, alias) VALUES (:id, :alias)")
            .SetGuid("id", id)
            .SetString("alias", id == Owner ? "owner" : "other")
            .ExecuteUpdate();
      foreach (var id in new[] { Ingredient, OtherIngredient })
         session.Save(
            new Db.Ingredients
            {
               IngredientId = id,
               DisplayName = id.ToString(),
               UnitWeight = 1,
               ConversionType = UnitType.Unit,
            },
            id
         );
      transaction.Commit();
      using var importer = new DatabaseImporter(adapter.GetSession());
      importer.Import(store.Recipes);
      importer.Import(store.RecipeIngredients);
      importer.Import(store.RecipeMetadata);
   }

   [ClassCleanup]
   public static void Cleanup()
   {
      adapter?.Dispose();
      if (connection != null)
      {
         using var command = new NpgsqlCommand(
            $"DROP SCHEMA IF EXISTS {schema} CASCADE",
            connection
         );
         command.ExecuteNonQuery();
         connection.Dispose();
      }
   }

   private static ISearchProvider Provider(bool database)
   {
      if (!database)
         return new StaticSearch(store);
      if (adapter == null)
         Assert.Inconclusive("Set KPC_TEST_POSTGRES for PostgreSQL integration tests.");
      return NHSearch.Instance(adapter);
   }

   [TestMethod]
   [DataRow(false)]
   [DataRow(true)]
   public async Task VisibilityUsesIdentityEvenWhenOwnerFilterIsSupplied(bool database)
   {
      var provider = Provider(database);
      foreach (
         var identity in new[]
         {
            AuthIdentity.Anonymous,
            new AuthIdentity(Owner, "owner"),
            new AuthIdentity(Other, "other"),
            null,
         }
      )
      foreach (var filter in new Guid?[] { null, Owner, Other, Guid.Empty })
      {
         var query = new RecipeQuery { UserId = filter };
         var result = await provider.SearchAsync(identity, query);
         var expected = store
            .Recipes.Where(r =>
               !r.Hidden || (identity?.IsAuthenticated == true && r.OwnerId == identity.UserId)
            )
            .Where(r => !filter.HasValue || r.OwnerId == filter)
            .ToArray();
         Assert.AreEqual((long)expected.Length, result.TotalCount);
         Assert.IsTrue(result.Briefs.All(r => expected.Any(e => e.RecipeId == r.Id)));
         Assert.IsTrue(result.Briefs.All(r => !r.Hidden || identity?.UserId == r.OwnerId));
      }
   }

   [TestMethod]
   [DataRow(false)]
   [DataRow(true)]
   public async Task OwnerOnlySearchKeepsAllPagesCountsAndSorts(bool database)
   {
      var provider = Provider(database);
      foreach (var sort in Enum.GetValues<RecipeQuery.SortOrder>())
      foreach (var direction in Enum.GetValues<RecipeQuery.SortDirection>())
      {
         var query = new RecipeQuery
         {
            UserId = Owner,
            Sort = sort,
            Direction = direction,
         };
         var pages = new List<RecipeBrief>();
         foreach (var offset in new[] { 0, 100, 200, 300 })
         {
            query.Offset = offset;
            var result = await provider.SearchAsync(new AuthIdentity(Owner, "owner"), query);
            Assert.AreEqual(205L, result.TotalCount);
            Assert.AreEqual(
               offset < 200 ? 100
                  : offset == 200 ? 5
                  : 0,
               result.Briefs.Length
            );
            pages.AddRange(result.Briefs);
         }
         Assert.AreEqual(205, pages.Select(r => r.Id).Distinct().Count());
         Assert.IsTrue(pages.All(r => r.OwnerId == Owner && r.Author == "owner"));
         Func<Dto.Recipes, IComparable> key = sort switch
         {
            RecipeQuery.SortOrder.Title => r => r.Title,
            RecipeQuery.SortOrder.PrepTime => r => r.PrepTime,
            RecipeQuery.SortOrder.CookTime => r => r.CookTime,
            RecipeQuery.SortOrder.TotalTime => r => r.PrepTime + r.CookTime,
            RecipeQuery.SortOrder.Image => r => r.ImageUrl,
            _ => r => r.Rating,
         };
         var own = store.Recipes.Where(r => r.OwnerId == Owner);
         // PostgreSQL places nulls last ascending and first descending.
         var expected =
            direction == RecipeQuery.SortDirection.Ascending
               ? own.OrderBy(r => key(r) == null).ThenBy(key).ThenBy(r => r.RecipeId)
               : own.OrderByDescending(r => key(r) == null)
                  .ThenByDescending(key)
                  .ThenBy(r => r.RecipeId);
         CollectionAssert.AreEqual(
            expected.Select(r => r.RecipeId).ToArray(),
            pages.Select(r => r.Id).ToArray(),
            $"{sort} {direction}"
         );
      }
   }

   [TestMethod]
   [DataRow(false)]
   [DataRow(true)]
   public async Task OwnerScopeComposesWithEveryFilterFamily(bool database)
   {
      var provider = Provider(database);
      Action<RecipeQuery>[] filters =
      [
         q => q.Keywords = "match",
         q => q.Rating = (Rating)5,
         q => q.Include = [Ingredient],
         q => q.Exclude = [OtherIngredient],
         q => q.Photos = RecipeQuery.PhotoFilter.Photo,
         q => q.Photos = RecipeQuery.PhotoFilter.HighRes,
         q => q.Meal = MealFilter.Dinner,
         q => q.Diet.GlutenFree = true,
         q => q.Nutrition.LowFat = true,
         q => q.Skill.Easy = true,
         q => q.Taste.MildToSpicy = RecipeQuery.SpicinessLevel.MildMedium,
      ];
      foreach (var filter in filters)
      {
         var query = new RecipeQuery { UserId = Owner };
         filter(query);
         var result = await provider.SearchAsync(new AuthIdentity(Owner, "owner"), query);
         Assert.AreEqual(103L, result.TotalCount);
         Assert.IsTrue(result.Briefs.All(r => r.Hidden && r.OwnerId == Owner));
         query.Offset = 100;
         Assert.AreEqual(
            3,
            (await provider.SearchAsync(new AuthIdentity(Owner, "owner"), query)).Briefs.Length
         );
      }
      var combined = new RecipeQuery { UserId = Owner, Keywords = "match" };
      foreach (var filter in filters)
         filter(combined);
      combined.Time.MaxPrep = 3;
      combined.Time.MaxCook = 10;
      combined.Time.MaxTime = 11;
      var count = store.Recipes.Count(r => r.OwnerId == Owner && r.Hidden && r.PrepTime <= 3);
      Assert.AreEqual(
         (long)count,
         (await provider.SearchAsync(new AuthIdentity(Owner, "owner"), combined)).TotalCount
      );
      combined.Time.MaxTime = 10;
      Assert.AreEqual(
         0L,
         (await provider.SearchAsync(new AuthIdentity(Owner, "owner"), combined)).TotalCount
      );
   }

   [TestMethod]
   [DataRow(false)]
   [DataRow(true)]
   public async Task IngredientCreateReadPreservesOrderRangesNotesSectionsAndForms(bool database)
   {
      var formId = Guid.NewGuid();
      var local = new DataStore
      {
         Recipes = [],
         RecipeIngredients = [],
         RecipeMetadata = [],
         Ingredients =
         [
            new Dto.Ingredients
            {
               IngredientId = Ingredient,
               DisplayName = "ingredient",
               ConversionType = UnitType.Unit,
            },
         ],
         IngredientMetadata = [new Dto.IngredientMetadata { IngredientId = Ingredient }],
         IngredientForms =
         [
            new Dto.IngredientForms
            {
               IngredientFormId = formId,
               IngredientId = Ingredient,
               UnitType = Units.Unit,
               FormUnit = Units.Unit,
               FormAmount = 1,
               ConvMultiplier = 1,
            },
         ],
      };
      var context = StaticContext.Configure.Identity(() => AuthIdentity.Anonymous).Create();
      typeof(StaticContext)
         .GetField("store", BindingFlags.Instance | BindingFlags.NonPublic)
         .SetValue(context, local);
      var input = new Recipe
      {
         Title = "Round trip",
         DateEntered = DateTime.UtcNow,
         Tags = RecipeTags.From(RecipeTag.Dinner),
         Ingredients =
         [
            new IngredientUsage
            {
               Ingredient = new Ingredient(Ingredient, "ingredient"),
               Amount = new Amount(1, 3, Units.Unit),
               Form = new IngredientForm { FormId = formId },
               PrepNote = "diced",
               Section = "First",
            },
            new IngredientUsage
            {
               Ingredient = new Ingredient(Ingredient, "ingredient"),
               Amount = null,
               PrepNote = "to taste",
               Section = "Second",
            },
         ],
      };
      Guid? created = null;
      try
      {
         if (database)
         {
            _ = Provider(true);
            using var session = adapter.GetSession();
            using var transaction = session.BeginTransaction();
            session.Save(
               new Db.IngredientForms
               {
                  Ingredient = Db.Ingredients.FromId(Ingredient),
                  UnitType = Units.Unit,
                  FormUnit = Units.Unit,
                  FormAmount = 1,
                  ConvMultiplier = 1,
               },
               formId
            );
            transaction.Commit();
            created = adapter.CreateRecipe(AuthIdentity.Anonymous, input).NewRecipeId;
            // Reverse storage order explicitly to prove reads honor DisplayOrder.
            using var reorder = adapter.GetSession();
            reorder
               .CreateSQLQuery(
                  "UPDATE recipeingredients SET displayorder = 3 - displayorder WHERE recipeid = :id"
               )
               .SetGuid("id", created.Value)
               .ExecuteUpdate();
         }
         else
         {
            created = context.CreateRecipe(input).NewRecipeId;
            foreach (var usage in local.RecipeIngredients)
               usage.DisplayOrder = (short)(3 - usage.DisplayOrder);
         }
         var loaded = database
            ? (
               await adapter.ReadRecipesAsync(
                  AuthIdentity.Anonymous,
                  [created.Value],
                  ReadRecipeOptions.None
               )
            ).Single()
            : context.ReadRecipes([created.Value], ReadRecipeOptions.None).Single();
         Assert.AreEqual("Second", loaded.Ingredients[0].Section);
         Assert.AreEqual("to taste", loaded.Ingredients[0].PrepNote);
         Assert.IsNull(loaded.Ingredients[0].Amount);
         Assert.IsNull(loaded.Ingredients[0].Form);
         Assert.AreEqual("First", loaded.Ingredients[1].Section);
         Assert.AreEqual("diced", loaded.Ingredients[1].PrepNote);
         Assert.AreEqual(1f, loaded.Ingredients[1].Amount.SizeLow);
         Assert.AreEqual(3f, loaded.Ingredients[1].Amount.SizeHigh);
         Assert.AreEqual(formId, loaded.Ingredients[1].Form.FormId);
      }
      finally
      {
         if (database && adapter != null)
         {
            using var session = adapter.GetSession();
            if (created.HasValue)
               foreach (var table in new[] { "recipeingredients", "recipemetadata", "recipes" })
                  session
                     .CreateSQLQuery($"DELETE FROM {table} WHERE recipeid = :id")
                     .SetGuid("id", created.Value)
                     .ExecuteUpdate();
            session
               .CreateSQLQuery("DELETE FROM ingredientforms WHERE formid = :id")
               .SetGuid("id", formId)
               .ExecuteUpdate();
         }
      }
   }

   [TestMethod]
   public void XmlAndQueryCopyPreserveOwnership()
   {
      var serializer = new XmlSerializer(typeof(DataStore));
      using var text = new StringWriter();
      serializer.Serialize(text, store);
      using var reader = new StringReader(text.ToString());
      var copy = (DataStore)serializer.Deserialize(reader);
      var query = new RecipeQuery(new RecipeQuery { UserId = Owner });
      Assert.AreEqual(Owner, query.UserId);
      var result = new StaticSearch(copy).Search(new AuthIdentity(Owner, "owner"), query);
      Assert.AreEqual(205L, result.TotalCount);
      Assert.IsTrue(result.Briefs.All(r => r.Author == "owner" && r.OwnerId == Owner));
   }

   [TestMethod]
   public void DatabaseModelerLoadersExcludeHiddenRecipes()
   {
      _ = Provider(true);
      using (var session = adapter.GetSession())
      using (var transaction = session.BeginTransaction())
      {
         foreach (var recipe in store.Recipes.Take(2))
            session.Save(
               new Db.RecipeRatings
               {
                  Recipe = session.Load<Db.Recipes>(recipe.RecipeId),
                  UserId = Owner,
                  Rating = 5,
               }
            );
         transaction.Commit();
      }
      var hidden = store.Recipes.Where(r => r.Hidden).Select(r => r.RecipeId).ToHashSet();
      var recipes = adapter.LoadRecipeGraph().ToArray();
      var ingredients = adapter.LoadIngredientGraph().ToArray();
      var ratings = adapter.LoadRatingGraph().ToArray();
      Assert.IsTrue(recipes.Length > 0 && ingredients.Length > 0 && ratings.Length > 0);
      Assert.IsFalse(recipes.Any(r => hidden.Contains(r.Id)));
      Assert.IsFalse(ingredients.Any(r => hidden.Contains(r.RecipeId)));
      Assert.IsFalse(ratings.Any(r => hidden.Contains(r.RecipeId)));
   }

   [TestMethod]
   public async Task DatabaseReadAndExportPreserveOwnershipAndIngredientRanges()
   {
      _ = Provider(true);
      var id = store.Recipes[0].RecipeId;
      var recipes = await adapter.ReadRecipesAsync(
         new AuthIdentity(Owner, "owner"),
         [id],
         ReadRecipeOptions.None
      );
      var recipe = recipes.Single();
      Assert.AreEqual(Owner, recipe.OwnerId);
      Assert.AreEqual("owner", recipe.OwnerAlias);
      Assert.IsTrue(recipe.Hidden);
      Assert.AreEqual(1f, recipe.Ingredients.Single().Amount.SizeLow);
      Assert.AreEqual(2f, recipe.Ingredients.Single().Amount.SizeHigh);
      Assert.AreEqual("chopped", recipe.Ingredients.Single().PrepNote);
      Assert.AreEqual("Main", recipe.Ingredients.Single().Section);
      using var exporter = new DatabaseExporter(adapter.GetStatelessSession());
      var exported = exporter.Recipes().Single(r => r.RecipeId == id);
      Assert.AreEqual(Owner, exported.OwnerId);
      Assert.AreEqual("owner", exported.OwnerAlias);
      Assert.IsTrue(exported.Hidden);
   }
}
