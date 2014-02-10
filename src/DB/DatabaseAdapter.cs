using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Helpers;
using KitchenPC.Context;
using KitchenPC.Data;
using KitchenPC.DB.Models;
using KitchenPC.DB.Provisioning;
using KitchenPC.Ingredients;
using KitchenPC.Menus;
using KitchenPC.Modeler;
using KitchenPC.NLP;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Transform;
using IngredientNode = KitchenPC.NLP.IngredientNode;
using IngredientUsage = KitchenPC.Ingredients.IngredientUsage;

namespace KitchenPC.DB
{
   /// <summary>A database adapter that uses NHibernate to connect to an underlying database.</summary>
   public class DatabaseAdapter : IDBAdapter, IDisposable
   {
      ISessionFactory sessionFactory;
      Configuration nhConfig;
      readonly DatabaseAdapterBuilder builder;

      public IPersistenceConfigurer DatabaseConfiguration { get; set; }
      public List<IConvention> DatabaseConventions { get; set; }
      public ISearchProvider SearchProvider { get; set; }

      public static DatabaseAdapterBuilder Configure
      {
         get
         {
            return new DatabaseAdapter().builder;
         }
      }

      DatabaseAdapter()
      {
         builder = new DatabaseAdapterBuilder(this);
      }

      ISessionFactory InitializeSessionFactory()
      {
         var conventions = new IConvention[]
         {
            Table.Is(x => x.EntityType.Name.ToLowerInvariant()), // All table names are lower case
            ForeignKey.EndsWith("Id"), // Foreign key references end with Id
            DefaultLazy.Always() // Enable Lazy-Loading by default
         }
            .Concat(DatabaseConventions.NeverNull())
            .ToArray();

         var config = Fluently.Configure()
            .Database(DatabaseConfiguration)
            .Mappings(m => m.FluentMappings
               .AddFromAssemblyOf<DatabaseAdapter>()
               .Conventions.Add(conventions));

         nhConfig = config.BuildConfiguration();
         sessionFactory = config.BuildSessionFactory();

         return sessionFactory;
      }

      public void Initialize(IKPCContext context)
      {
         if (sessionFactory == null)
            sessionFactory = InitializeSessionFactory();
      }

      public ISession GetSession()
      {
         // For now, this will create a new session - However, eventually we could re-use sessions within threads, HTTP request context, etc

         return sessionFactory.OpenSession();
      }

      public IStatelessSession GetStatelessSession()
      {
         return sessionFactory.OpenStatelessSession();
      }

      public void Dispose()
      {
         if (sessionFactory != null)
            sessionFactory.Dispose();
      }

      public IEnumerable<IngredientSource> LoadIngredientsForIndex()
      {
         using (var session = GetStatelessSession())
         {
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

            Models.Ingredients ing = null;
            int? count = null;
            var popularity = QueryOver.Of<RecipeIngredients>()
               .Where(p => p.Ingredient.IngredientId == ing.IngredientId)
               .ToRowCountQuery();

            var ingredients = session.QueryOver<Models.Ingredients>(() => ing)
               .SelectList(list => list
                  .Select(p => p.IngredientId)
                  .Select(p => p.DisplayName)
                  .SelectSubQuery(popularity).WithAlias(() => count)
               )
               .OrderByAlias(() => count).Desc()
               .ThenBy(p => p.DisplayName).Asc()
               .List<Object[]>()
               .Select(i => new IngredientSource((Guid) i[0], (String) i[1]));

            return ingredients.ToList();
         }
      }

      public IEnumerable<RecipeBinding> LoadRecipeGraph()
      {
         using (var session = GetStatelessSession())
         {
            RecipeMetadata metadata = null;
            var recipes = session.QueryOver<Models.Recipes>()
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
                  p => metadata.SkillQuick)
               .List<Object[]>();

            return recipes.Select(r => new RecipeBinding
            {
               Id = (Guid) r[0],
               Rating = Convert.ToByte(r[1]),
               Tags = ((bool) r[2] ? 1 << 0 : 0) +
                      ((bool) r[3] ? 1 << 1 : 0) +
                      ((bool) r[4] ? 1 << 2 : 0) +
                      ((bool) r[5] ? 1 << 3 : 0) +
                      ((bool) r[6] ? 1 << 4 : 0) +
                      ((bool) r[7] ? 1 << 5 : 0) +
                      ((bool) r[8] ? 1 << 6 : 0) +
                      ((bool) r[9] ? 1 << 7 : 0) +
                      ((bool) r[10] ? 1 << 8 : 0) +
                      ((bool) r[11] ? 1 << 9 : 0) +
                      ((bool) r[12] ? 1 << 10 : 0) +
                      ((bool) r[13] ? 1 << 11 : 0) +
                      ((bool) r[14] ? 1 << 12 : 0) +
                      ((bool) r[15] ? 1 << 13 : 0) +
                      ((bool) r[16] ? 1 << 14 : 0) +
                      ((bool) r[17] ? 1 << 15 : 0) +
                      ((bool) r[18] ? 1 << 16 : 0)
            }).ToList();
         }
      }

      public IEnumerable<IngredientBinding> LoadIngredientGraph()
      {
         using (var session = GetStatelessSession())
         {
            IngredientForms joinForm = null;
            Models.Ingredients joinIng = null;

            var recIngs = session.QueryOver<RecipeIngredients>()
               .JoinAlias(r => r.IngredientForm, () => joinForm)
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
                  p => joinForm.FormUnit)
               .TransformUsing(IngredientGraphTransformer.Create())
               .List<IngredientBinding>();

            return recIngs;
         }
      }

      public IEnumerable<RatingBinding> LoadRatingGraph()
      {
         using (var session = GetStatelessSession())
         {
            //Load ratings
            var ratings = session.QueryOver<RecipeRatings>().List();

            return new List<RatingBinding>(ratings.Select(s => new RatingBinding
            {
               RecipeId = s.Recipe.RecipeId,
               UserId = s.UserId,
               Rating = s.Rating
            }));
         }
      }

      public ISynonymLoader<IngredientNode> IngredientLoader
      {
         get
         {
            return new IngredientLoader(this);
         }
      }

      public ISynonymLoader<UnitNode> UnitLoader
      {
         get
         {
            return new UnitLoader(this);
         }
      }

      public ISynonymLoader<FormNode> FormLoader
      {
         get
         {
            return new FormLoader(this);
         }
      }

      public ISynonymLoader<PrepNode> PrepLoader
      {
         get
         {
            return new PrepLoader(this);
         }
      }

      public ISynonymLoader<AnomalousNode> AnomalyLoader
      {
         get
         {
            return new AnomalyLoader(this);
         }
      }

      public Recipe[] ReadRecipes(AuthIdentity identity, Guid[] recipeIds, ReadRecipeOptions options)
      {
         using (var session = GetSession())
         {
            var dbRecipes = session.QueryOver<Models.Recipes>()
               .Fetch(prop => prop.RecipeMetadata).Eager
               .Fetch(prop => prop.Ingredients).Eager
               .Fetch(prop => prop.Ingredients[0].Ingredient).Eager
               .Fetch(prop => prop.Ingredients[0].IngredientForm).Eager
               .AndRestrictionOn(p => p.RecipeId).IsInG(recipeIds)
               .TransformUsing(Transformers.DistinctRootEntity)
               .List();

            if (!dbRecipes.Any())
               throw new RecipeNotFoundException();

            var ret = new List<Recipe>();
            foreach (var dbRecipe in dbRecipes)
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
                  AvgRating = dbRecipe.Rating
               };

               if (options.ReturnMethod)
                  recipe.Method = dbRecipe.Steps;

               if (options.ReturnUserRating) // TODO: We should JOIN this on the dbRecipes for faster loading
               {
                  var id = dbRecipe.RecipeId;
                  var rating = session.QueryOver<RecipeRatings>()
                     .Where(p => p.Recipe.RecipeId == id)
                     .Where(p => p.UserId == identity.UserId)
                     .SingleOrDefault();

                  recipe.UserRating = (rating == null ? Rating.None : (Rating) rating.Rating);
               }

               recipe.Ingredients = dbRecipe.Ingredients.Select(i => new IngredientUsage
               {
                  Amount = i.Qty.HasValue ? new Amount(i.Qty.Value, i.Unit) : null,
                  PrepNote = i.PrepNote,
                  Section = i.Section,
                  Form = i.IngredientForm != null ? i.IngredientForm.AsIngredientForm() : null, // Note: Form will be null when usage has no amount
                  Ingredient = i.Ingredient.AsIngredient()
               }).ToArray();

               recipe.Tags = dbRecipe.RecipeMetadata.Tags;
               ret.Add(recipe);
            }

            return ret.ToArray();
         }
      }

      public SearchResults RecipeSearch(AuthIdentity identity, RecipeQuery query)
      {
         if (SearchProvider == null)
            throw new NoConfiguredSearchProvidersException();

         return SearchProvider.Search(identity, query);
      }

      public void RateRecipe(AuthIdentity identity, Guid recipeId, Rating rating)
      {
         using (var session = GetSession())
         {
            using (var transaction = session.BeginTransaction())
            {
               var existingRate = session.QueryOver<RecipeRatings>()
                  .Where(p => p.UserId == identity.UserId)
                  .Where(p => p.Recipe.RecipeId == recipeId)
                  .SingleOrDefault();

               if (existingRate != null) // Update existing
               {
                  existingRate.Rating = (byte) rating;
                  session.Update(existingRate);
               }
               else // Create rating
               {
                  session.Save(new RecipeRatings
                  {
                     UserId = identity.UserId,
                     Recipe = new Models.Recipes {RecipeId = recipeId},
                     Rating = (byte) rating
                  });
               }

               transaction.Commit();
            }
         }
      }

      public RecipeResult CreateRecipe(AuthIdentity identity, Recipe recipe)
      {
         using (var session = GetSession())
         {
            using (var transaction = session.BeginTransaction())
            {
               // Create Recipe
               var dbRecipe = new Models.Recipes
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
                  Steps = recipe.Method
               };

               session.Save(dbRecipe);

               // Create Ingredients
               short displayOrder = 0;
               recipe.Ingredients.ForEach(i =>
               {
                  var dbIngredient = new RecipeIngredients
                  {
                     Recipe = dbRecipe,
                     Ingredient = Models.Ingredients.FromId(i.Ingredient.Id),
                     IngredientForm = (i.Form != null ? IngredientForms.FromId(i.Form.FormId) : null),
                     Qty = (i.Amount != null ? (float?) i.Amount.SizeHigh : null),
                     QtyLow = (i.Amount != null ? (float?) i.Amount.SizeLow : null),
                     Unit = (i.Amount != null ? i.Amount.Unit : Units.Unit),
                     Section = i.Section,
                     DisplayOrder = ++displayOrder
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
                  SkillQuick = recipe.Tags.HasTag(RecipeTag.Quick)
               };

               session.Save(dbMetadata);
               transaction.Commit();

               return new RecipeResult
               {
                  RecipeCreated = true,
                  NewRecipeId = dbRecipe.RecipeId
               };
            }
         }
      }

      public IngredientFormsCollection ReadFormsForIngredient(Guid ingredientId)
      {
         using (var session = GetSession())
         {
            var dbIng = session.QueryOver<Models.Ingredients>()
               .Fetch(prop => prop.Forms).Eager
               .Where(p => p.IngredientId == ingredientId)
               .SingleOrDefault();

            if (dbIng == null)
               throw new IngredientNotFoundException();

            return new IngredientFormsCollection(from f in dbIng.Forms select f.AsIngredientForm());
         }
      }

      public Ingredient ReadIngredient(string ingredient)
      {
         using (var session = GetSession())
         {
            var dbIng = session.QueryOver<Models.Ingredients>()
               .Fetch(prop => prop.Metadata).Eager
               .Where(p => p.DisplayName == ingredient.Trim())
               .SingleOrDefault();

            if (dbIng == null)
               throw new IngredientNotFoundException();

            return dbIng.AsIngredient();
         }
      }

      public Ingredient ReadIngredient(Guid ingid)
      {
         using (var session = GetSession())
         {
            var dbIng = session.QueryOver<Models.Ingredients>()
               .Fetch(prop => prop.Metadata).Eager
               .Where(p => p.IngredientId == ingid)
               .SingleOrDefault();

            if (dbIng == null)
               throw new IngredientNotFoundException();

            return dbIng.AsIngredient();
         }
      }

      public void DequeueRecipe(AuthIdentity identity, params Guid[] recipeIds)
      {
         using (var session = GetSession())
         {
            var recipes = (from r in recipeIds select new Models.Recipes {RecipeId = r}).ToArray();

            var dbRecipes = session.QueryOver<QueuedRecipes>()
               .Where(p => p.UserId == identity.UserId);

            if (recipeIds.Any())
            {
               dbRecipes = dbRecipes.AndRestrictionOn(p => p.Recipe).IsInG(recipes);
            }

            using (var transaction = session.BeginTransaction())
            {
               dbRecipes.List().ForEach(session.Delete);
               transaction.Commit();
            }
         }
      }

      public void EnqueueRecipes(AuthIdentity identity, params Guid[] recipeIds)
      {
         using (var session = GetSession())
         {
            // Check for dupes
            var recipes = (from r in recipeIds select new Models.Recipes {RecipeId = r}).ToArray();

            var dupes = session.QueryOver<QueuedRecipes>()
               .Where(p => p.UserId == identity.UserId)
               .AndRestrictionOn(p => p.Recipe).IsInG(recipes)
               .List<QueuedRecipes>();

            var existing = (from r in dupes select r.Recipe.RecipeId).ToList();

            // Enqueue each recipe
            using (var transaction = session.BeginTransaction())
            {
               var now = DateTime.Now;
               foreach (var rid in recipeIds.Where(rid => !existing.Contains(rid)))
               {
                  session.Save(new QueuedRecipes
                  {
                     Recipe = new Models.Recipes {RecipeId = rid},
                     UserId = identity.UserId,
                     QueuedDate = now
                  });
               }

               transaction.Commit();
            }
         }
      }

      public RecipeBrief[] GetRecipeQueue(AuthIdentity identity)
      {
         using (var session = GetSession())
         {
            var dbRecipes = session.QueryOver<QueuedRecipes>()
               .Fetch(prop => prop.Recipe).Eager
               .Where(p => p.UserId == identity.UserId)
               .List();

            return (from r in dbRecipes select r.Recipe.AsRecipeBrief()).ToArray();
         }
      }

      public Menu[] GetMenus(AuthIdentity identity, IList<Menu> menus, GetMenuOptions options)
      {
         using (var session = GetSession())
         {
            // menus will be null if all menus should be loaded, or a list of Menu objects to specify individual menus to load
            if (options == null) throw new ArgumentNullException("options");
            if (identity == null) throw new ArgumentNullException("identity");

            var loadFav = true;
            var query = session.QueryOver<Models.Menus>()
               .Where(p => p.UserId == identity.UserId);

            if (menus != null) // Load individual menus
            {
               loadFav = menus.Contains(Menu.Favorites);
               var ids = menus.Where(m => m.Id.HasValue).Select(m => m.Id.Value).ToArray();
               query = query.AndRestrictionOn(p => p.MenuId).IsInG(ids);
            }

            var dbMenus = query.List();
            var ret = new List<Menu>();

            if (loadFav)
               ret.Add(Menu.Favorites);

            ret.AddRange(dbMenus.Select(m => m.AsMenu()));

            if (!options.LoadRecipes) // We're done!
               return ret.ToArray();

            // Load recipes into each menu
            ICriterion filter = (loadFav
               ? Expression.Or(Expression.IsNull("Menu"), Expression.InG("Menu", dbMenus)) // Menu can be null, or in loaded menu list
               : Expression.InG("Menu", dbMenus)); // Menu must be in loaded menu list

            var dbFavorites = session.QueryOver<Favorites>()
               .Fetch(prop => prop.Recipe).Eager
               .Where(p => p.UserId == identity.UserId)
               .Where(filter)
               .List();

            return ret.Select(m =>
               new Menu(m)
               {
                  Recipes = (m.Id.HasValue
                     ? dbFavorites.Where(f => f.Menu != null && f.Menu.MenuId == m.Id)
                     : dbFavorites.Where(f => f.Menu == null)
                     ).Select(r => r.Recipe.AsRecipeBrief()).ToArray()
               }).ToArray();
         }
      }

      public MenuResult CreateMenu(AuthIdentity identity, Menu menu, params Guid[] recipeIds)
      {
         using (var session = GetSession())
         {
            menu.Title = menu.Title.Trim();
            var ret = new MenuResult();

            using (var transaction = session.BeginTransaction())
            {
               Models.Menus dbMenu;
               var dupes = session.QueryOver<Models.Menus>()
                  .Where(p => p.UserId == identity.UserId)
                  .Where(p => p.Title == menu.Title)
                  .ToRowCountQuery()
                  .RowCount();

               if (dupes > 0)
               {
                  throw new MenuAlreadyExistsException();
               }

               session.Save(dbMenu = new Models.Menus
               {
                  UserId = identity.UserId,
                  Title = menu.Title,
                  CreatedDate = DateTime.Now,
               });

               foreach (var rid in recipeIds.NeverNull())
               {
                  var fav = new Favorites
                  {
                     UserId = identity.UserId,
                     Recipe = new Models.Recipes() {RecipeId = rid},
                     Menu = dbMenu
                  };

                  session.Save(fav);
               }

               transaction.Commit();

               ret.MenuCreated = true;
               ret.NewMenuId = dbMenu.MenuId;
            }

            return ret;
         }
      }

      public void DeleteMenus(AuthIdentity identity, params Guid[] menuIds)
      {
         using (var session = GetSession())
         {
            using (var transaction = session.BeginTransaction())
            {
               var dbMenu = session.QueryOver<Models.Menus>()
                  .AndRestrictionOn(p => p.MenuId).IsInG(menuIds)
                  .Where(p => p.UserId == identity.UserId)
                  .Fetch(prop => prop.Recipes).Eager()
                  .List();

               dbMenu.ForEach(session.Delete);
               transaction.Commit();
            }
         }
      }

      public MenuResult UpdateMenu(AuthIdentity identity, Guid? menuId, Guid[] recipesAdd, Guid[] recipesRemove, MenuMove[] recipesMove, bool clear, string newName = null)
      {
         var ret = new MenuResult();
         ret.MenuUpdated = true; // TODO: Verify actual changes were made before setting MenuUpdated to true

         using (var session = GetSession())
         {
            using (var transaction = session.BeginTransaction())
            {
               Models.Menus dbMenu = null;
               IList<Favorites> dbRecipes = null;
               if (menuId.HasValue)
               {
                  dbMenu = session.QueryOver<Models.Menus>()
                     .Fetch(prop => prop.Recipes).Eager
                     .Where(p => p.MenuId == menuId)
                     .SingleOrDefault();

                  if (dbMenu == null)
                     throw new MenuNotFoundException();

                  if (dbMenu.UserId != identity.UserId) // User does not have access to modify this menu
                     throw new UserDoesNotOwnMenuException();

                  if (!String.IsNullOrWhiteSpace(newName) && dbMenu != null) // Rename menu
                     dbMenu.Title = newName.Trim();

                  dbRecipes = dbMenu.Recipes;
               }
               else
               {
                  dbRecipes = session.QueryOver<Favorites>()
                     .Where(p => p.UserId == identity.UserId)
                     .Where(p => p.Menu == null)
                     .List();
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
                        Recipe = new Models.Recipes() {RecipeId = rid},
                        Menu = dbMenu
                     };

                     session.Save(fav);
                  }
               }

               if (recipesRemove.Any()) // Remove recipes from menu
               {
                  var toDelete = (from r in dbRecipes where recipesRemove.Contains(r.Recipe.RecipeId) select r);
                  toDelete.ForEach(session.Delete);
               }

               if (clear) // Remove every recipe from menu
               {
                  dbRecipes.ForEach(session.Delete);
               }

               if (recipesMove.Any()) // Move items to another menu
               {
                  foreach (var moveAction in recipesMove)
                  {
                     Models.Menus dbTarget = null;
                     if (moveAction.TargetMenu.HasValue)
                     {
                        dbTarget = session.QueryOver<Models.Menus>()
                           .Where(p => p.MenuId == moveAction.TargetMenu.Value)
                           .Where(p => p.UserId == identity.UserId)
                           .SingleOrDefault();

                        if (dbTarget == null)
                           throw new MenuNotFoundException(moveAction.TargetMenu.Value);
                     }

                     var rToMove = (moveAction.MoveAll
                        ? dbRecipes
                        : dbRecipes.Where(r => moveAction.RecipesToMove.Contains(r.Recipe.RecipeId)));

                     rToMove.ForEach(a => a.Menu = dbTarget);
                  }
               }

               transaction.Commit();
            }
         }

         return ret;
      }

      public void MoveMenuItem(AuthIdentity identity, Guid recipeId, Menu fromMenu, Menu toMenu)
      {
         using (var session = GetSession())
         {
            using (var transaction = session.BeginTransaction())
            {
               if (!fromMenu.Id.HasValue || !toMenu.Id.HasValue)
                  throw new MenuIdRequiredException();

               var dbFavorite = session.QueryOver<Favorites>()
                  .Where(p => p.Menu.MenuId == fromMenu.Id.Value)
                  .Where(p => p.Recipe.RecipeId == recipeId)
                  .SingleOrDefault();

               if (dbFavorite == null)
                  throw new RecipeNotFoundException();

               var dbToMenu = session.QueryOver<Models.Menus>()
                  .Where(p => p.MenuId == toMenu.Id.Value)
                  .SingleOrDefault();

               if (dbToMenu == null)
                  throw new MenuNotFoundException();

               dbFavorite.Menu = dbToMenu;
               session.Update(dbFavorite);
               transaction.Commit();
            }
         }
      }

      public ShoppingList[] GetShoppingLists(AuthIdentity identity, IList<ShoppingList> lists, GetShoppingListOptions options)
      {
         using (var session = GetSession())
         {
            var loadDef = true;
            var query = session.QueryOver<Models.ShoppingLists>()
               .Where(p => p.UserId == identity.UserId);

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
            ICriterion filter = (loadDef
               ? Expression.Or(Expression.IsNull("ShoppingList"), Expression.InG("ShoppingList", dbLists)) // Menu can be null, or in loaded menu list
               : Expression.InG("ShoppingList", dbLists)); // Menu must be in loaded menu list

            var dbItems = session.QueryOver<ShoppingListItems>()
               .Fetch(prop => prop.Ingredient).Eager
               .Fetch(prop => prop.Recipe).Eager
               .Where(p => p.UserId == identity.UserId)
               .Where(filter)
               .List();

            return ret.Select(m =>
               new ShoppingList(
                  m.Id,
                  m.Title,
                  (m.Id.HasValue
                     ? dbItems.Where(f => f.ShoppingList != null && f.ShoppingList.ShoppingListId == m.Id)
                     : dbItems.Where(f => f.ShoppingList == null)).Select(r => r.AsShoppingListItem())
                  )).ToArray();
         }
      }

      public ShoppingListResult CreateShoppingList(AuthIdentity identity, ShoppingList list)
      {
         using (var session = GetSession())
         {
            var ret = new ShoppingListResult();

            using (var transaction = session.BeginTransaction())
            {
               var dbList = new Models.ShoppingLists();
               dbList.Title = list.Title.Trim();
               dbList.UserId = identity.UserId;
               session.Save(dbList);

               if (list.Any()) // Create ShoppingListItems
               {
                  list.ToList().ForEach(i =>
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
      }

      public void DeleteShoppingLists(AuthIdentity identity, ShoppingList[] lists)
      {
         if (!lists.Any())
            throw new ArgumentException("DeleteShoppingLists requires at least one list to delete.");

         using (var session = GetSession())
         {
            using (var transaction = session.BeginTransaction())
            {
               var dbLists = session.QueryOver<Models.ShoppingLists>()
                  .AndRestrictionOn(p => p.ShoppingListId).IsInG(lists.Where(l => l.Id.HasValue).Select(l => l.Id.Value))
                  .Where(p => p.UserId == identity.UserId)
                  .List();

               dbLists.ForEach(session.Delete);
               transaction.Commit();
            }
         }
      }

      public ShoppingListResult UpdateShoppingList(AuthIdentity identity, Guid? listId, Guid[] toRemove, ShoppingListModification[] toModify, IShoppingListSource[] toAdd, string newName = null)
      {
         using (var session = GetSession())
         {
            using (var transaction = session.BeginTransaction())
            {
               // Deletes
               if (toRemove.Any())
               {
                  var dbDeletes = session.QueryOver<ShoppingListItems>()
                     .Where(p => p.UserId == identity.UserId)
                     .Where(listId.HasValue
                        ? Expression.Eq("ShoppingList", listId.Value)
                        : Expression.IsNull("ShoppingList")
                     ).AndRestrictionOn(p => p.ItemId).IsInG(toRemove)
                     .List();

                  dbDeletes.ForEach(session.Delete);
               }

               // Updates
               Models.ShoppingLists dbList = null;
               IList<ShoppingListItems> dbItems = null;
               if (listId.HasValue)
               {
                  dbList = session.QueryOver<Models.ShoppingLists>()
                     .Fetch(prop => prop.Items).Eager
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
                  dbItems = session.QueryOver<ShoppingListItems>()
                     .Where(p => p.UserId == identity.UserId)
                     .Where(p => p.ShoppingList == null)
                     .List();
               }

               toModify.ForEach(item =>
               {
                  var dbItem = dbItems.FirstOrDefault(i => i.ItemId == item.ModifiedItemId);
                  if (dbItem == null) return;

                  if (item.CrossOut.HasValue) dbItem.CrossedOut = item.CrossOut.Value;
                  if (item.NewAmount != null) dbItem.Amount = item.NewAmount;
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
                           Raw = source.Raw
                        };

                        session.Save(newItem);
                        dbItems.Add(newItem);
                     }

                     return;
                  }

                  if (source.Ingredient != null && source.Amount == null) // Raw ingredient without any amount
                  {
                     var existingItem = dbItems.FirstOrDefault(i => i.Ingredient != null && i.Ingredient.IngredientId == source.Ingredient.Id);

                     if (existingItem == null) // Add it
                     {
                        var newItem = new ShoppingListItems
                        {
                           ShoppingList = dbList,
                           UserId = identity.UserId,
                           Ingredient = Models.Ingredients.FromId(source.Ingredient.Id)
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
                     var existingItem = dbItems.FirstOrDefault(i => i.Ingredient != null && i.Ingredient.IngredientId == source.Ingredient.Id);

                     if (existingItem == null) // Add it
                     {
                        var newItem = new ShoppingListItems
                        {
                           ShoppingList = dbList,
                           UserId = identity.UserId,
                           Ingredient = Models.Ingredients.FromId(source.Ingredient.Id),
                           Amount = source.Amount
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
                     dbList != null ? (Guid?) dbList.ShoppingListId : null,
                     dbList != null ? dbList.Title : null,
                     dbItems.Select(i => i.AsShoppingListItem()))
               };
            }
         }
      }

      public DataStore Export()
      {
         var store = new DataStore();
         using (var exporter = new DatabaseExporter(GetStatelessSession()))
         {
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
         }

         return store;
      }

      public void Import(IProvisionSource source)
      {
         KPCContext.Log.DebugFormat("Importing data from {0} into DBContext.", source.GetType().Name);

         var store = source.Export();
         if (store == null)
            throw new DataStoreException("Given data source contains no data to import.");

         if (sessionFactory == null)
            InitializeSessionFactory();

         using (var importer = new DatabaseImporter(GetSession()))
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

         KPCContext.Log.DebugFormat("Done importing data from into DBContext.");
      }

      public void InitializeStore()
      {
         if (sessionFactory == null)
            sessionFactory = InitializeSessionFactory();

         KPCContext.Log.DebugFormat("Creating database schema on configured database.");
         var export = new SchemaExport(nhConfig);
         export.Create(false, true);
         KPCContext.Log.DebugFormat("Done creating database schema.");
      }
   }
}