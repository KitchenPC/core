using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Menus;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.NLP;
using KitchenPC.Core.Provisioning;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;
using KitchenPC.DB.Models;
using KitchenPC.DB.NLP;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transform;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using IngredientNode = KitchenPC.Core.NLP.IngredientNode;
using IngredientUsage = KitchenPC.Core.Ingredients.IngredientUsage;

namespace KitchenPC.DB;

/// <summary>A database adapter that uses NHibernate to connect to an underlying database.</summary>
public class DatabaseAdapter : IDBAdapter, IDisposable
{
   private ISessionFactory sessionFactory;
   private Configuration nhConfig;
   private readonly DatabaseAdapterBuilder builder;

   public IPersistenceConfigurer DatabaseConfiguration { get; set; }
   public List<IConvention> DatabaseConventions { get; set; }
   public ISearchProvider SearchProvider { get; set; }
   public Microsoft.Extensions.Logging.ILoggerFactory LoggerFactory { get; set; } =
      NullLoggerFactory.Instance;

   public static DatabaseAdapterBuilder Configure => new DatabaseAdapter().builder;

   private DatabaseAdapter()
   {
      builder = new DatabaseAdapterBuilder(this);
   }

   private ISessionFactory InitializeSessionFactory()
   {
      var conventions = new IConvention[]
      {
         Table.Is(x => x.EntityType.Name.ToLowerInvariant()), // All table names are lower case
         ForeignKey.EndsWith("Id"), // Foreign key references end with Id
         DefaultLazy.Always(), // Enable Lazy-Loading by default
      }
         .Concat(DatabaseConventions.NeverNull())
         .ToArray();

      var config = Fluently
         .Configure()
         .Database(DatabaseConfiguration)
         .Mappings(m =>
            m.FluentMappings.AddFromAssemblyOf<DatabaseAdapter>()
               .AddFromAssembly(System.Reflection.Assembly.GetEntryAssembly()) // TODO: Allow configuration for which assemblies are used for mappings
               .Conventions.Add(conventions)
         );

      nhConfig = config.BuildConfiguration();
      sessionFactory = config.BuildSessionFactory();

      return sessionFactory;
   }

   public void Initialize(IKPCContext context)
   {
      LoggerFactory = context.LoggerFactory;
      sessionFactory ??= InitializeSessionFactory();
   }

   // For now, this will create a new session - However, eventually we could re-use sessions within threads, HTTP request context, etc
   public ISession GetSession() => sessionFactory.OpenSession();

   public IStatelessSession GetStatelessSession() => sessionFactory.OpenStatelessSession();

   public void Dispose()
   {
      sessionFactory?.Dispose();
   }

   public IEnumerable<IngredientSource> LoadIngredientsForIndex()
   {
      using var session = GetStatelessSession();
      // Query for all ingredients, most used ingredients first
      // Note: Commenting out Linq query since there appears to be an NH bug with multiple ORDER BY clauses
      /*
         var ingredients = (from ing in session.Query<Ingredients>()
                            orderby ((from p in session.Query<RecipeIngredients>()
                                     where p.Ingredient == ing
                                     select p.RecipeIngredientId).Count()) descending,
                                     ing.DisplayName ascending
                            select new IngredientSource(ing.IngredientId, ing.DisplayName));
         */

      Ingredients ing = null;
      int? count = null;
      var popularity = QueryOver
         .Of<RecipeIngredients>()
         .Where(p => p.Ingredient.IngredientId == ing.IngredientId)
         .ToRowCountQuery();

      var ingredients = session
         .QueryOver<Ingredients>(() => ing)
         .SelectList(list =>
            list.Select(p => p.IngredientId)
               .Select(p => p.DisplayName)
               .SelectSubQuery(popularity)
               .WithAlias(() => count)
         )
         .OrderByAlias(() => count)
         .Desc()
         .ThenBy(p => p.DisplayName)
         .Asc()
         .List<Object[]>()
         .Select(i => new IngredientSource((Guid)i[0], (String)i[1]));

      return ingredients.ToList();
   }

   public IEnumerable<RecipeBinding> LoadRecipeGraph()
   {
      using var session = GetStatelessSession();
      RecipeMetadata metadata = null;
      var recipes = session
         .QueryOver<Recipes>()
         .JoinAlias(r => r.RecipeMetadata, () => metadata)
         .Select(
            p => p.RecipeId,
            p => p.Rating,
            p => metadata.DietGlutenFree,
            p => metadata.DietNoAnimals,
            p => metadata.DietNomeat,
            p => metadata.DietNoPork,
            p => metadata.DietNoRedMeat,
            p => metadata.MealBreakfast,
            p => metadata.MealDessert,
            p => metadata.MealDinner,
            p => metadata.MealLunch,
            p => metadata.NutritionLowCalorie,
            p => metadata.NutritionLowCarb,
            p => metadata.NutritionLowFat,
            p => metadata.NutritionLowSodium,
            p => metadata.NutritionLowSugar,
            p => metadata.SkillCommon,
            p => metadata.SkillEasy,
            p => metadata.SkillQuick
         )
         .List<Object[]>();

      return recipes
         .Select(r => new RecipeBinding
         {
            Id = (Guid)r[0],
            Rating = Convert.ToByte(r[1]),
            Tags =
               ((bool)r[2] ? 1 << 0 : 0)
               + ((bool)r[3] ? 1 << 1 : 0)
               + ((bool)r[4] ? 1 << 2 : 0)
               + ((bool)r[5] ? 1 << 3 : 0)
               + ((bool)r[6] ? 1 << 4 : 0)
               + ((bool)r[7] ? 1 << 5 : 0)
               + ((bool)r[8] ? 1 << 6 : 0)
               + ((bool)r[9] ? 1 << 7 : 0)
               + ((bool)r[10] ? 1 << 8 : 0)
               + ((bool)r[11] ? 1 << 9 : 0)
               + ((bool)r[12] ? 1 << 10 : 0)
               + ((bool)r[13] ? 1 << 11 : 0)
               + ((bool)r[14] ? 1 << 12 : 0)
               + ((bool)r[15] ? 1 << 13 : 0)
               + ((bool)r[16] ? 1 << 14 : 0)
               + ((bool)r[17] ? 1 << 15 : 0)
               + ((bool)r[18] ? 1 << 16 : 0),
         })
         .ToList();
   }

   public IEnumerable<IngredientBinding> LoadIngredientGraph()
   {
      using var session = GetStatelessSession();
      IngredientForms joinForm = null;
      Ingredients joinIng = null;

      var recIngs = session
         .QueryOver<RecipeIngredients>()
         .Left.JoinAlias(r => r.IngredientForm, () => joinForm)
         .JoinAlias(r => r.Ingredient, () => joinIng)
         .Where(p => joinIng.IngredientId != ShoppingList.GUID_WATER) // Ignore any usage for water
         .Select(
            p => joinIng.IngredientId,
            p => p.Recipe.RecipeId,
            p => p.Qty,
            p => p.Unit,
            p => joinIng.ConversionType,
            p => joinIng.UnitWeight,
            p => joinForm.UnitType,
            p => joinForm.FormAmount,
            p => joinForm.FormUnit,
            p => joinIng.DisplayName
         )
         .TransformUsing(IngredientGraphTransformer.Create())
         .List<IngredientBinding>();

      return recIngs;
   }

   public IEnumerable<RatingBinding> LoadRatingGraph()
   {
      using var session = GetStatelessSession();
      //Load ratings
      var ratings = session.QueryOver<RecipeRatings>().List();

      return new List<RatingBinding>(
         ratings.Select(s => new RatingBinding
         {
            RecipeId = s.Recipe.RecipeId,
            UserId = s.UserId,
            Rating = s.Rating,
         })
      );
   }

   public ISynonymLoader<IngredientNode> IngredientLoader => new IngredientLoader(this);

   public ISynonymLoader<UnitNode> UnitLoader => new UnitLoader(this);

   public ISynonymLoader<FormNode> FormLoader => new FormLoader(this);

   public ISynonymLoader<PrepNode> PrepLoader => new PrepLoader(this);

   public ISynonymLoader<AnomalousNode> AnomalyLoader => new AnomalyLoader(this);

   public Recipe[] ReadRecipes(AuthIdentity identity, Guid[] recipeIds, ReadRecipeOptions options)
   {
      return ReadRecipesAsync(identity, recipeIds, options).GetAwaiter().GetResult();
   }

   public async Task<Recipe[]> ReadRecipesAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      ReadRecipeOptions options,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      var dbRecipes = session
         .QueryOver<Recipes>()
         .Fetch(SelectMode.Fetch, prop => prop.RecipeMetadata)
         .Fetch(SelectMode.Fetch, prop => prop.Ingredients)
         .Fetch(SelectMode.Fetch, prop => prop.Ingredients[0].Ingredient)
         .Fetch(SelectMode.Fetch, prop => prop.Ingredients[0].IngredientForm)
         .AndRestrictionOn(p => p.RecipeId)
         .IsInG(recipeIds)
         .TransformUsing(Transformers.DistinctRootEntity)
         .ListAsync(cancellationToken);

      var loadedRecipes = await dbRecipes;

      if (!loadedRecipes.Any())
         throw new RecipeNotFoundException();

      var userRatings = new Dictionary<Guid, Rating>();
      var menuCounts = new Dictionary<Guid, int>();

      if (identity?.IsAuthenticated == true)
      {
         var recipeReferences = loadedRecipes
            .Select(item => new Recipes { RecipeId = item.RecipeId })
            .ToArray();

         if (options.ReturnUserRating)
         {
            var loadedRatings = await session
               .QueryOver<RecipeRatings>()
               .Where(item => item.UserId == identity.UserId)
               .AndRestrictionOn(item => item.Recipe)
               .IsInG(recipeReferences)
               .ListAsync(cancellationToken);

            userRatings = loadedRatings.ToDictionary(
               item => item.Recipe.RecipeId,
               item => (Rating)item.Rating
            );
         }

         if (options.ReturnMenuCount)
         {
            var loadedFavorites = await session
               .QueryOver<Favorites>()
               .Where(item => item.UserId == identity.UserId)
               .AndRestrictionOn(item => item.Recipe)
               .IsInG(recipeReferences)
               .ListAsync(cancellationToken);

            menuCounts = loadedFavorites
               .GroupBy(item => item.Recipe.RecipeId)
               .ToDictionary(group => group.Key, group => group.Count());
         }
      }

      var ret = new List<Recipe>();
      foreach (var dbRecipe in loadedRecipes)
      {
         var recipe = new Recipe
         {
            Id = dbRecipe.RecipeId,
            Title = dbRecipe.Title,
            Description = dbRecipe.Description,
            DateEntered = dbRecipe.DateEntered,
            ImageUrl = dbRecipe.ImageUrl,
            ServingSize = dbRecipe.ServingSize,
            PrepTime = dbRecipe.PrepTime,
            CookTime = dbRecipe.CookTime,
            Credit = dbRecipe.Credit,
            CreditUrl = dbRecipe.CreditUrl,
            AvgRating = dbRecipe.Rating,
         };

         if (options.ReturnMethod)
            recipe.Method = dbRecipe.Steps;

         if (userRatings.TryGetValue(dbRecipe.RecipeId, out var userRating))
            recipe.UserRating = userRating;

         if (menuCounts.TryGetValue(dbRecipe.RecipeId, out var menuCount))
            recipe.InMenus = menuCount;

         recipe.Ingredients = dbRecipe
            .Ingredients.Select(i => new IngredientUsage
            {
               Amount = i.Qty.HasValue ? new Amount(i.Qty.Value, i.Unit) : null,
               PrepNote = i.PrepNote,
               Section = i.Section,
               Form = i.IngredientForm?.AsIngredientForm(), // Note: Form will be null when usage has no amount
               Ingredient = i.Ingredient.AsIngredient(),
            })
            .ToArray();

         recipe.Tags = dbRecipe.RecipeMetadata.Tags;
         ret.Add(recipe);
      }

      return ret.ToArray();
   }

   public SearchResults RecipeSearch(AuthIdentity identity, RecipeQuery query)
   {
      if (SearchProvider == null)
         throw new NoConfiguredSearchProvidersException();

      return SearchProvider.Search(identity, query);
   }

   public Task<SearchResults> RecipeSearchAsync(
      AuthIdentity identity,
      RecipeQuery query,
      CancellationToken cancellationToken = default
   )
   {
      if (SearchProvider == null)
         throw new NoConfiguredSearchProvidersException();

      return SearchProvider.SearchAsync(identity, query, cancellationToken);
   }

   public void RateRecipe(AuthIdentity identity, Guid recipeId, Rating rating)
   {
      RateRecipeAsync(identity, recipeId, rating).GetAwaiter().GetResult();
   }

   public async Task RateRecipeAsync(
      AuthIdentity identity,
      Guid recipeId,
      Rating rating,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      using var transaction = session.BeginTransaction();
      var existingRate = session
         .QueryOver<RecipeRatings>()
         .Where(p => p.UserId == identity.UserId)
         .Where(p => p.Recipe.RecipeId == recipeId)
         .SingleOrDefaultAsync(cancellationToken);

      var loadedRate = await existingRate;

      if (loadedRate != null) // Update existing
      {
         loadedRate.Rating = (byte)rating;
         await session.UpdateAsync(loadedRate, cancellationToken);
      }
      else // Create rating
      {
         await session.SaveAsync(
            new RecipeRatings
            {
               UserId = identity.UserId,
               Recipe = new Recipes { RecipeId = recipeId },
               Rating = (byte)rating,
            },
            cancellationToken
         );
      }

      await transaction.CommitAsync(cancellationToken);
   }

   public RecipeResult CreateRecipe(AuthIdentity identity, Recipe recipe)
   {
      using var session = GetSession();
      using var transaction = session.BeginTransaction();
      // Create Recipe
      var dbRecipe = new Recipes
      {
         Title = recipe.Title,
         Description = recipe.Description,
         CookTime = recipe.CookTime,
         PrepTime = recipe.PrepTime,
         Credit = recipe.Credit,
         CreditUrl = recipe.CreditUrl,
         DateEntered = recipe.DateEntered,
         ImageUrl = recipe.ImageUrl,
         Rating = recipe.AvgRating,
         ServingSize = recipe.ServingSize,
         Steps = recipe.Method,
      };

      session.Save(dbRecipe);

      // Create Ingredients
      short displayOrder = 0;
      recipe.Ingredients.ForEach(i =>
      {
         var dbIngredient = new RecipeIngredients
         {
            Recipe = dbRecipe,
            Ingredient = Ingredients.FromId(i.Ingredient.Id),
            IngredientForm = (i.Form != null ? IngredientForms.FromId(i.Form.FormId) : null),
            Qty = (i.Amount != null ? (float?)i.Amount.SizeHigh : null),
            QtyLow = (i.Amount != null ? (float?)i.Amount.SizeLow : null),
            Unit = (i.Amount != null ? i.Amount.Unit : Units.Unit),
            Section = i.Section,
            DisplayOrder = ++displayOrder,
         };

         session.Save(dbIngredient);
      });

      // Create RecipeMetadata
      var dbMetadata = new RecipeMetadata
      {
         Recipe = dbRecipe,
         DietGlutenFree = recipe.Tags.HasTag(RecipeTag.GlutenFree),
         DietNoAnimals = recipe.Tags.HasTag(RecipeTag.NoAnimals),
         DietNomeat = recipe.Tags.HasTag(RecipeTag.NoMeat),
         DietNoPork = recipe.Tags.HasTag(RecipeTag.NoPork),
         DietNoRedMeat = recipe.Tags.HasTag(RecipeTag.NoRedMeat),
         MealBreakfast = recipe.Tags.HasTag(RecipeTag.Breakfast),
         MealDessert = recipe.Tags.HasTag(RecipeTag.Dessert),
         MealDinner = recipe.Tags.HasTag(RecipeTag.Dinner),
         MealLunch = recipe.Tags.HasTag(RecipeTag.Lunch),
         NutritionLowCalorie = recipe.Tags.HasTag(RecipeTag.LowCalorie),
         NutritionLowCarb = recipe.Tags.HasTag(RecipeTag.LowCarb),
         NutritionLowFat = recipe.Tags.HasTag(RecipeTag.LowFat),
         NutritionLowSodium = recipe.Tags.HasTag(RecipeTag.LowSodium),
         NutritionLowSugar = recipe.Tags.HasTag(RecipeTag.LowSugar),
         SkillCommon = recipe.Tags.HasTag(RecipeTag.Common),
         SkillEasy = recipe.Tags.HasTag(RecipeTag.Easy),
         SkillQuick = recipe.Tags.HasTag(RecipeTag.Quick),
      };

      session.Save(dbMetadata);
      transaction.Commit();

      return new RecipeResult { RecipeCreated = true, NewRecipeId = dbRecipe.RecipeId };
   }

   public IngredientFormsCollection ReadFormsForIngredient(Guid ingredientId)
   {
      using var session = GetSession();
      var dbIng = session
         .QueryOver<Ingredients>()
         .Fetch(SelectMode.Fetch, prop => prop.Forms)
         .Where(p => p.IngredientId == ingredientId)
         .SingleOrDefault();

      if (dbIng == null)
         throw new IngredientNotFoundException();

      return new IngredientFormsCollection(from f in dbIng.Forms select f.AsIngredientForm());
   }

   public Ingredient ReadIngredient(string ingredient)
   {
      return ReadIngredientAsync(ingredient).GetAwaiter().GetResult();
   }

   public async Task<Ingredient> ReadIngredientAsync(
      string ingredient,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      var dbIng = session
         .QueryOver<Ingredients>()
         .Fetch(SelectMode.Fetch, prop => prop.Metadata)
         .Where(p => p.DisplayName == ingredient.Trim())
         .SingleOrDefaultAsync(cancellationToken);

      var loadedIngredient = await dbIng;

      if (loadedIngredient == null)
         throw new IngredientNotFoundException();

      return loadedIngredient.AsIngredient();
   }

   public Ingredient ReadIngredient(Guid ingid)
   {
      return ReadIngredientAsync(ingid).GetAwaiter().GetResult();
   }

   public async Task<Ingredient> ReadIngredientAsync(
      Guid ingredientId,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      var dbIng = session
         .QueryOver<Ingredients>()
         .Fetch(SelectMode.Fetch, prop => prop.Metadata)
         .Where(p => p.IngredientId == ingredientId)
         .SingleOrDefaultAsync(cancellationToken);

      var loadedIngredient = await dbIng;

      if (loadedIngredient == null)
         throw new IngredientNotFoundException();

      return loadedIngredient.AsIngredient();
   }

   public void DequeueRecipe(AuthIdentity identity, params Guid[] recipeIds)
   {
      DequeueRecipeAsync(identity, recipeIds).GetAwaiter().GetResult();
   }

   public async Task DequeueRecipeAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      var recipes = (from r in recipeIds select new Recipes { RecipeId = r }).ToArray();

      var dbRecipes = session.QueryOver<QueuedRecipes>().Where(p => p.UserId == identity.UserId);

      if (recipeIds.Any())
      {
         dbRecipes = dbRecipes.AndRestrictionOn(p => p.Recipe).IsInG(recipes);
      }

      using var transaction = session.BeginTransaction();
      var queuedRecipes = await dbRecipes.ListAsync(cancellationToken);
      foreach (var queuedRecipe in queuedRecipes)
      {
         await session.DeleteAsync(queuedRecipe, cancellationToken);
      }
      await transaction.CommitAsync(cancellationToken);
   }

   public void EnqueueRecipes(AuthIdentity identity, params Guid[] recipeIds)
   {
      EnqueueRecipesAsync(identity, recipeIds).GetAwaiter().GetResult();
   }

   public async Task EnqueueRecipesAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      // Check for dupes
      var recipes = (from r in recipeIds select new Recipes { RecipeId = r }).ToArray();

      var dupes = session
         .QueryOver<QueuedRecipes>()
         .Where(p => p.UserId == identity.UserId)
         .AndRestrictionOn(p => p.Recipe)
         .IsInG(recipes)
         .ListAsync<QueuedRecipes>(cancellationToken);

      var loadedDupes = await dupes;

      var existing = (from r in loadedDupes select r.Recipe.RecipeId).ToList();

      // Enqueue each recipe
      using var transaction = session.BeginTransaction();
      var now = DateTime.Now;
      foreach (var rid in recipeIds.Where(rid => !existing.Contains(rid)))
      {
         await session.SaveAsync(
            new QueuedRecipes
            {
               Recipe = new Recipes { RecipeId = rid },
               UserId = identity.UserId,
               QueuedDate = now,
            },
            cancellationToken
         );
      }

      await transaction.CommitAsync(cancellationToken);
   }

   public RecipeBrief[] GetRecipeQueue(AuthIdentity identity)
   {
      return GetRecipeQueueAsync(identity).GetAwaiter().GetResult();
   }

   public async Task<RecipeBrief[]> GetRecipeQueueAsync(
      AuthIdentity identity,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      var dbRecipes = session
         .QueryOver<QueuedRecipes>()
         .Fetch(SelectMode.Fetch, prop => prop.Recipe)
         .Where(p => p.UserId == identity.UserId)
         .ListAsync(cancellationToken);

      return (from r in await dbRecipes select r.Recipe.AsRecipeBrief()).ToArray();
   }

   public Menu[] GetMenus(AuthIdentity identity, IList<Menu> menus, GetMenuOptions options)
   {
      return GetMenusAsync(identity, menus, options).GetAwaiter().GetResult();
   }

   public async Task<Menu[]> GetMenusAsync(
      AuthIdentity identity,
      IList<Menu> menus,
      GetMenuOptions options,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      // menus will be null if all menus should be loaded, or a list of Menu objects to specify individual menus to load
      if (options == null)
         throw new ArgumentNullException("options");
      if (identity == null)
         throw new ArgumentNullException("identity");

      var loadFav = true;
      var query = session.QueryOver<Menus>().Where(p => p.UserId == identity.UserId);

      if (menus != null) // Load individual menus
      {
         loadFav = menus.Contains(Menu.Favorites);
         var ids = menus.Where(m => m.Id.HasValue).Select(m => m.Id.Value).ToArray();
         query = query.AndRestrictionOn(p => p.MenuId).IsInG(ids);
      }

      var dbMenus = await query.ListAsync(cancellationToken);
      var ret = new List<Menu>();

      if (loadFav)
         ret.Add(Menu.Favorites);

      ret.AddRange(dbMenus.Select(m => m.AsMenu()));

      if (!options.LoadRecipes) // We're done!
         return ret.ToArray();

      // Load recipes into each menu
      ICriterion filter = (
         loadFav
            ? Restrictions.Or(Restrictions.IsNull("Menu"), Restrictions.InG("Menu", dbMenus)) // Menu can be null, or in loaded menu list
            : Restrictions.InG("Menu", dbMenus)
      ); // Menu must be in loaded menu list

      var dbFavorites = session
         .QueryOver<Favorites>()
         .Fetch(SelectMode.Fetch, prop => prop.Recipe)
         .Where(p => p.UserId == identity.UserId)
         .Where(filter)
         .ListAsync(cancellationToken);

      var loadedFavorites = await dbFavorites;

      return ret.Select(m => new Menu(m)
         {
            Recipes = (
               m.Id.HasValue
                  ? loadedFavorites.Where(f => f.Menu != null && f.Menu.MenuId == m.Id)
                  : loadedFavorites.Where(f => f.Menu == null)
            )
               .Select(r => r.Recipe.AsRecipeBrief())
               .ToArray(),
         })
         .ToArray();
   }

   public MenuResult CreateMenu(AuthIdentity identity, Menu menu, params Guid[] recipeIds)
   {
      return CreateMenuAsync(identity, menu, recipeIds).GetAwaiter().GetResult();
   }

   public async Task<MenuResult> CreateMenuAsync(
      AuthIdentity identity,
      Menu menu,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      menu.Title = menu.Title.Trim();
      var ret = new MenuResult();

      using var transaction = session.BeginTransaction();
      Menus dbMenu;
      var dupes = session
         .QueryOver<Menus>()
         .Where(p => p.UserId == identity.UserId)
         .Where(p => p.Title == menu.Title)
         .ToRowCountQuery()
         .RowCountAsync(cancellationToken);

      if (await dupes > 0)
      {
         throw new MenuAlreadyExistsException();
      }

      await session.SaveAsync(
         dbMenu = new Menus
         {
            UserId = identity.UserId,
            Title = menu.Title,
            CreatedDate = DateTime.Now,
         },
         cancellationToken
      );

      foreach (var rid in recipeIds.NeverNull().Distinct())
      {
         var fav = new Favorites
         {
            UserId = identity.UserId,
            Recipe = new Recipes() { RecipeId = rid },
            Menu = dbMenu,
         };

         await session.SaveAsync(fav, cancellationToken);
      }

      await transaction.CommitAsync(cancellationToken);

      ret.MenuCreated = true;
      ret.NewMenuId = dbMenu.MenuId;

      return ret;
   }

   public void DeleteMenus(AuthIdentity identity, params Guid[] menuIds)
   {
      DeleteMenusAsync(identity, menuIds).GetAwaiter().GetResult();
   }

   public async Task DeleteMenusAsync(
      AuthIdentity identity,
      Guid[] menuIds,
      CancellationToken cancellationToken = default
   )
   {
      using var session = GetSession();
      using var transaction = session.BeginTransaction();
      var dbMenu = session
         .QueryOver<Menus>()
         .AndRestrictionOn(p => p.MenuId)
         .IsInG(menuIds)
         .Where(p => p.UserId == identity.UserId)
         .Fetch(SelectMode.Fetch, prop => prop.Recipes)
         .ListAsync(cancellationToken);

      foreach (var menu in await dbMenu)
      {
         await session.DeleteAsync(menu, cancellationToken);
      }
      await transaction.CommitAsync(cancellationToken);
   }

   public MenuResult UpdateMenu(
      AuthIdentity identity,
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null
   )
   {
      return UpdateMenuAsync(
            identity,
            menuId,
            recipesAdd,
            recipesRemove,
            recipesMove,
            clear,
            newName
         )
         .GetAwaiter()
         .GetResult();
   }

   public async Task<MenuResult> UpdateMenuAsync(
      AuthIdentity identity,
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null,
      CancellationToken cancellationToken = default
   )
   {
      var ret = new MenuResult();
      ret.MenuUpdated = true; // TODO: Verify actual changes were made before setting MenuUpdated to true

      using var session = GetSession();
      using var transaction = session.BeginTransaction();
      Menus dbMenu = null;
      IList<Favorites> dbRecipes;
      if (menuId.HasValue)
      {
         var loadedMenu = session
            .QueryOver<Menus>()
            .Fetch(SelectMode.Fetch, prop => prop.Recipes)
            .Where(p => p.MenuId == menuId)
            .SingleOrDefaultAsync(cancellationToken);

         dbMenu = await loadedMenu;

         if (dbMenu == null)
            throw new MenuNotFoundException();

         if (dbMenu.UserId != identity.UserId) // User does not have access to modify this menu
            throw new UserDoesNotOwnMenuException();

         if (!String.IsNullOrWhiteSpace(newName)) // Rename menu
            dbMenu.Title = newName.Trim();

         dbRecipes = dbMenu.Recipes;
      }
      else
      {
         var loadedRecipes = session
            .QueryOver<Favorites>()
            .Where(p => p.UserId == identity.UserId)
            .Where(p => p.Menu == null)
            .ListAsync(cancellationToken);

         dbRecipes = await loadedRecipes;
      }

      if (recipesAdd.Any()) // Add recipes to menu
      {
         var existing = (from r in dbRecipes select r.Recipe.RecipeId);
         recipesAdd = recipesAdd.Except(existing).ToArray(); //Remove dupes

         foreach (var rid in recipesAdd)
         {
            var fav = new Favorites
            {
               UserId = identity.UserId,
               Recipe = new Recipes() { RecipeId = rid },
               Menu = dbMenu,
            };

            await session.SaveAsync(fav, cancellationToken);
         }
      }

      if (recipesRemove.Any()) // Remove recipes from menu
      {
         var toDelete = (
            from r in dbRecipes
            where recipesRemove.Contains(r.Recipe.RecipeId)
            select r
         );
         foreach (var recipe in toDelete)
         {
            await session.DeleteAsync(recipe, cancellationToken);
         }
      }

      if (clear) // Remove every recipe from menu
      {
         foreach (var recipe in dbRecipes)
         {
            await session.DeleteAsync(recipe, cancellationToken);
         }
      }

      if (recipesMove.Any()) // Move items to another menu
      {
         foreach (var moveAction in recipesMove)
         {
            Menus dbTarget = null;
            if (moveAction.TargetMenu.HasValue)
            {
               var loadedTarget = session
                  .QueryOver<Menus>()
                  .Where(p => p.MenuId == moveAction.TargetMenu.Value)
                  .Where(p => p.UserId == identity.UserId)
                  .SingleOrDefaultAsync(cancellationToken);

               dbTarget = await loadedTarget;

               if (dbTarget == null)
                  throw new MenuNotFoundException(moveAction.TargetMenu.Value);
            }

            var rToMove = (
               moveAction.MoveAll
                  ? dbRecipes
                  : dbRecipes.Where(r => moveAction.RecipesToMove.Contains(r.Recipe.RecipeId))
            ).ToArray();

            if (
               !moveAction.MoveAll
               && rToMove.Select(r => r.Recipe.RecipeId).Distinct().Count()
                  != moveAction.RecipesToMove.Distinct().Count()
            )
            {
               throw new MenuItemNotFoundException();
            }

            var recipeIds = rToMove.Select(r => r.Recipe.RecipeId).ToArray();
            var destinationQuery = session
               .QueryOver<Favorites>()
               .Fetch(SelectMode.Fetch, p => p.Recipe)
               .Where(p => p.UserId == identity.UserId);
            destinationQuery = moveAction.TargetMenu.HasValue
               ? destinationQuery.Where(p => p.Menu.MenuId == moveAction.TargetMenu.Value)
               : destinationQuery.Where(p => p.Menu == null);

            if (
               recipeIds.Length > 0
               && (await destinationQuery.ListAsync(cancellationToken)).Any(f =>
                  recipeIds.Contains(f.Recipe.RecipeId)
               )
            )
            {
               throw new DuplicateCookbookException();
            }

            rToMove.ForEach(a => a.Menu = dbTarget);
         }
      }

      await transaction.CommitAsync(cancellationToken);

      return ret;
   }

   public void MoveMenuItem(AuthIdentity identity, Guid recipeId, Menu fromMenu, Menu toMenu)
   {
      using var session = GetSession();
      using var transaction = session.BeginTransaction();
      if (!fromMenu.Id.HasValue || !toMenu.Id.HasValue)
         throw new MenuIdRequiredException();

      var dbFavorite = session
         .QueryOver<Favorites>()
         .Where(p => p.Menu.MenuId == fromMenu.Id.Value)
         .Where(p => p.Recipe.RecipeId == recipeId)
         .SingleOrDefault();

      if (dbFavorite == null)
         throw new RecipeNotFoundException();

      var dbToMenu = session
         .QueryOver<Menus>()
         .Where(p => p.MenuId == toMenu.Id.Value)
         .SingleOrDefault();

      if (dbToMenu == null)
         throw new MenuNotFoundException();

      dbFavorite.Menu = dbToMenu;
      session.Update(dbFavorite);
      transaction.Commit();
   }

   public ShoppingList[] GetShoppingLists(
      AuthIdentity identity,
      IList<ShoppingList> lists,
      GetShoppingListOptions options
   )
   {
      using var session = GetSession();
      var loadDef = true;
      var query = session.QueryOver<ShoppingLists>().Where(p => p.UserId == identity.UserId);

      if (lists != null) // Load individual lists
      {
         loadDef = lists.Contains(ShoppingList.Default);
         var ids = lists.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToArray();
         query = query.AndRestrictionOn(x => x.ShoppingListId).IsInG(ids);
      }

      var dbLists = query.List();
      var ret = new List<ShoppingList>();

      if (loadDef)
         ret.Add(ShoppingList.Default);

      ret.AddRange(dbLists.Select(l => l.AsShoppingList()));

      if (!options.LoadItems) // We're done!
         return ret.ToArray();

      // Load items into each list
      ICriterion filter = (
         loadDef
            ? Restrictions.Or(
               Restrictions.IsNull("ShoppingList"),
               Restrictions.InG("ShoppingList", dbLists)
            ) // Menu can be null, or in loaded menu list
            : Restrictions.InG("ShoppingList", dbLists)
      ); // Menu must be in loaded menu list

      var dbItems = session
         .QueryOver<ShoppingListItems>()
         .Fetch(SelectMode.Fetch, prop => prop.Ingredient)
         .Fetch(SelectMode.Fetch, prop => prop.Recipe)
         .Where(p => p.UserId == identity.UserId)
         .Where(filter)
         .List();

      return ret.Select(m => new ShoppingList(
            m.Id,
            m.Title,
            (
               m.Id.HasValue
                  ? dbItems.Where(f =>
                     f.ShoppingList != null && f.ShoppingList.ShoppingListId == m.Id
                  )
                  : dbItems.Where(f => f.ShoppingList == null)
            ).Select(r => r.AsShoppingListItem())
         ))
         .ToArray();
   }

   public ShoppingListResult CreateShoppingList(AuthIdentity identity, ShoppingList list)
   {
      using var session = GetSession();
      var ret = new ShoppingListResult();

      using (var transaction = session.BeginTransaction())
      {
         var dbList = new ShoppingLists();
         dbList.Title = list.Title.Trim();
         dbList.UserId = identity.UserId;
         session.Save(dbList);

         if (list.Any()) // Create ShoppingListItems
         {
            list.ToList()
               .ForEach(i =>
               {
                  var dbItem = ShoppingListItems.FromShoppingListItem(i);
                  dbItem.ShoppingList = dbList;
                  dbItem.UserId = dbList.UserId;
                  session.Save(dbItem);
               });
         }

         transaction.Commit();

         ret.NewShoppingListId = dbList.ShoppingListId;
      }

      ret.List = list;
      return ret;
   }

   public void DeleteShoppingLists(AuthIdentity identity, ShoppingList[] lists)
   {
      if (!lists.Any())
         throw new ArgumentException("DeleteShoppingLists requires at least one list to delete.");

      using var session = GetSession();
      using var transaction = session.BeginTransaction();
      var dbLists = session
         .QueryOver<ShoppingLists>()
         .AndRestrictionOn(p => p.ShoppingListId)
         .IsInG(lists.Where(l => l.Id.HasValue).Select(l => l.Id.Value))
         .Where(p => p.UserId == identity.UserId)
         .List();

      dbLists.ForEach(session.Delete);
      transaction.Commit();
   }

   public ShoppingListResult UpdateShoppingList(
      AuthIdentity identity,
      Guid? listId,
      Guid[] toRemove,
      ShoppingListModification[] toModify,
      IShoppingListSource[] toAdd,
      string newName = null
   )
   {
      using var session = GetSession();
      using var transaction = session.BeginTransaction();
      // Deletes
      if (toRemove.Any())
      {
         var dbDeletes = session
            .QueryOver<ShoppingListItems>()
            .Where(p => p.UserId == identity.UserId)
            .Where(
               listId.HasValue
                  ? Restrictions.Eq("ShoppingList", listId.Value)
                  : Restrictions.IsNull("ShoppingList")
            )
            .AndRestrictionOn(p => p.ItemId)
            .IsInG(toRemove)
            .List();

         dbDeletes.ForEach(session.Delete);
      }

      // Updates
      ShoppingLists dbList = null;
      IList<ShoppingListItems> dbItems;
      if (listId.HasValue)
      {
         dbList = session
            .QueryOver<ShoppingLists>()
            .Fetch(SelectMode.Fetch, prop => prop.Items)
            .Where(p => p.UserId == identity.UserId)
            .Where(p => p.ShoppingListId == listId.Value)
            .SingleOrDefault();

         if (dbList == null)
            throw new ShoppingListNotFoundException();

         if (!String.IsNullOrWhiteSpace(newName))
            dbList.Title = newName;

         dbItems = dbList.Items;
      }
      else
      {
         dbItems = session
            .QueryOver<ShoppingListItems>()
            .Where(p => p.UserId == identity.UserId)
            .Where(p => p.ShoppingList == null)
            .List();
      }

      toModify.ForEach(item =>
      {
         var dbItem = dbItems.FirstOrDefault(i => i.ItemId == item.ModifiedItemId);
         if (dbItem == null)
            return;

         if (item.CrossOut.HasValue)
            dbItem.CrossedOut = item.CrossOut.Value;
         if (item.NewAmount != null)
            dbItem.Amount = item.NewAmount;
      });

      toAdd.ForEach(item =>
      {
         var source = item.GetItem();

         if (source.Ingredient == null && !String.IsNullOrWhiteSpace(source.Raw)) // Raw shopping list item
         {
            if (!dbItems.Any(i => source.Raw.Equals(i.Raw, StringComparison.OrdinalIgnoreCase))) // Add it
            {
               var newItem = new ShoppingListItems
               {
                  ShoppingList = dbList,
                  UserId = identity.UserId,
                  Raw = source.Raw,
               };

               session.Save(newItem);
               dbItems.Add(newItem);
            }

            return;
         }

         if (source.Ingredient != null && source.Amount == null) // Raw ingredient without any amount
         {
            var existingItem = dbItems.FirstOrDefault(i =>
               i.Ingredient != null && i.Ingredient.IngredientId == source.Ingredient.Id
            );

            if (existingItem == null) // Add it
            {
               var newItem = new ShoppingListItems
               {
                  ShoppingList = dbList,
                  UserId = identity.UserId,
                  Ingredient = Ingredients.FromId(source.Ingredient.Id),
               };

               session.Save(newItem);
               dbItems.Add(newItem);
            }
            else // Clear out existing amount
            {
               existingItem.Amount = null;
            }
         }

         if (source.Ingredient != null && source.Amount != null) // Ingredient with amount, aggregate if necessary
         {
            var existingItem = dbItems.FirstOrDefault(i =>
               i.Ingredient != null && i.Ingredient.IngredientId == source.Ingredient.Id
            );

            if (existingItem == null) // Add it
            {
               var newItem = new ShoppingListItems
               {
                  ShoppingList = dbList,
                  UserId = identity.UserId,
                  Ingredient = Ingredients.FromId(source.Ingredient.Id),
                  Amount = source.Amount,
               };

               session.Save(newItem);
               dbItems.Add(newItem);
            }
            else if (existingItem.Amount != null) // Add to total
            {
               existingItem.Amount += source.Amount;
            }
         }
      });

      transaction.Commit();

      return new ShoppingListResult
      {
         List = new ShoppingList(
            dbList?.ShoppingListId,
            dbList?.Title,
            dbItems.Select(i => i.AsShoppingListItem())
         ),
      };
   }

   public DataStore Export()
   {
      var store = new DataStore();
      using var exporter = new DatabaseExporter(GetStatelessSession(), LoggerFactory);
      store.IngredientForms = exporter.IngredientForms();
      store.IngredientMetadata = exporter.IngredientMetadata();
      store.Ingredients = exporter.Ingredients();
      store.NlpAnomalousIngredients = exporter.NlpAnomalousIngredients();
      store.NlpDefaultPairings = exporter.NlpDefaultPairings();
      store.NlpFormSynonyms = exporter.NlpFormSynonyms();
      store.NlpIngredientSynonyms = exporter.NlpIngredientSynonyms();
      store.NlpPrepNotes = exporter.NlpPrepNotes();
      store.NlpUnitSynonyms = exporter.NlpUnitSynonyms();
      store.Recipes = exporter.Recipes();
      store.RecipeMetadata = exporter.RecipeMetadata();
      store.RecipeIngredients = exporter.RecipeIngredients();
      store.Favorites = exporter.Favorites();
      store.Menus = exporter.Menus();
      store.QueuedRecipes = exporter.QueuedRecipes();
      store.RecipeRatings = exporter.RecipeRatings();
      store.ShoppingLists = exporter.ShoppingLists();
      store.ShoppingListItems = exporter.ShoppingListItems();

      return store;
   }

   public void Import(IProvisionSource source)
   {
      //KPCContext.Log.DebugFormat("Importing data from {0} into DBContext.", source.GetType().Name);

      var store = source.Export();
      if (store == null)
         throw new DataStoreException("Given data source contains no data to import.");

      if (sessionFactory == null)
         InitializeSessionFactory();

      using (var importer = new DatabaseImporter(GetSession(), LoggerFactory))
      {
         // Note: Import order is important to maintain referential integrity of database
         importer.Import(store.Ingredients);
         importer.Import(store.IngredientForms);
         importer.Import(store.IngredientMetadata);
         importer.Import(store.NlpAnomalousIngredients);
         importer.Import(store.NlpDefaultPairings);
         importer.Import(store.NlpFormSynonyms);
         importer.Import(store.NlpIngredientSynonyms);
         importer.Import(store.NlpPrepNotes);
         importer.Import(store.NlpUnitSynonyms);
         importer.Import(store.Recipes);
         importer.Import(store.RecipeIngredients);
         importer.Import(store.RecipeMetadata);
         importer.Import(store.Menus);
         importer.Import(store.Favorites);
         importer.Import(store.QueuedRecipes);
         importer.Import(store.RecipeRatings);
         importer.Import(store.ShoppingLists);
         importer.Import(store.ShoppingListItems);
      }

      // TODO Fix logging
      //KPCContext.Log.DebugFormat("Done importing data from into DBContext.");
   }

   public void InitializeStore()
   {
      if (sessionFactory == null)
         sessionFactory = InitializeSessionFactory();

      //KPCContext.Log.DebugFormat("Creating database schema on configured database.");
      var export = new SchemaExport(nhConfig);
      export.Create(false, true);
      //KPCContext.Log.DebugFormat("Done creating database schema.");
   }
}
