using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using KitchenPC.Context.Fluent;
using KitchenPC.Data;
using KitchenPC.Ingredients;
using KitchenPC.Menus;
using KitchenPC.Modeler;
using KitchenPC.NLP;
using KitchenPC.Provisioning;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
using IngredientUsage = KitchenPC.Ingredients.IngredientUsage;

namespace KitchenPC.Context
{
   /// <summary>A KitchenPC Context that loads and saves data through a configured database adapter.</summary>
   public class DBContext : IKPCContext, IProvisionSource, IProvisionTarget
   {
      static readonly ReaderWriterLockSlim InitLock = new ReaderWriterLockSlim();
      readonly DBContextBuilder builder;

      IngredientParser ingParser;
      ModelerProxy modeler;
      Parser parser;

      /// <summary>Gets or sets the IDBAdapter used to directly talk with the database.</summary>
      public IDBAdapter Adapter { get; set; }

      /// <summary>Gets or sets a function that, when called, will return the identity of the current user.  This function will allow KitchenPC to load and save user specific data.</summary>
      public Func<AuthIdentity> GetIdentity { get; set; }

      /// <summary>
      /// Returns the identity of the current user using the GetIdentity function.
      /// </summary>
      public virtual AuthIdentity Identity
      {
         get
         {
            return GetIdentity();
         }
      }

      /// <summary>
      /// Returns a DBContextBuilder used to configure a DBContext instance.
      /// </summary>
      public static DBContextBuilder Configure
      {
         get
         {
            return new DBContext().builder;
         }
      }

      DBContext()
      {
         builder = new DBContextBuilder(this);
      }

      public virtual Parser Parser
      {
         get
         {
            using (InitLock.ReadLock())
            {
               return parser;
            }
         }
      }

      public virtual ModelerProxy ModelerProxy
      {
         get
         {
            using (InitLock.ReadLock())
            {
               return modeler;
            }
         }
      }

      /// <summary>Provides the ability to fluently work with menus.</summary>
      public virtual MenuAction Menus
      {
         get
         {
            return new MenuAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with recipes.</summary>
      public virtual RecipeAction Recipes
      {
         get
         {
            return new RecipeAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with shopping lists.</summary>
      public virtual ShoppingListAction ShoppingLists
      {
         get
         {
            return new ShoppingListAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with the recipe queue.</summary>
      public virtual QueueAction Queue
      {
         get
         {
            return new QueueAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with the recipe modeler.</summary>
      public virtual ModelerAction Modeler
      {
         get
         {
            return new ModelerAction(this);
         }
      }

      /// <summary>
      /// Initializes the context and loads necessary data into memory through the configured database adapter.
      /// This must be done before the context is usable.
      /// </summary>
      public virtual void Initialize()
      {
         if (Adapter == null)
            throw new InvalidConfigurationException("DBContext requires a configured database adapter.  Please check your configuration.");

         // Initiaze NHibernate session
         Adapter.Initialize(this);

         new Thread(delegate()
         {
            using (InitLock.WriteLock())
            {
               // Initialize ingredient parser
               ingParser = new IngredientParser();
               var ingredientIndex = Adapter.LoadIngredientsForIndex();
               ingParser.CreateIndex(ingredientIndex);

               // Initialize modeler
               modeler = new ModelerProxy(this);
               modeler.LoadSnapshot();

               // Initialize natural language parsing
               IngredientSynonyms.InitIndex(Adapter.IngredientLoader);
               UnitSynonyms.InitIndex(Adapter.UnitLoader);
               FormSynonyms.InitIndex(Adapter.FormLoader);
               PrepNotes.InitIndex(Adapter.PrepLoader);
               Anomalies.InitIndex(Adapter.AnomalyLoader);
               NumericVocab.InitIndex();

               parser = new Parser();
               LoadTemplates();
            }
         }).Start();

         Thread.Sleep(500); // Provides time for initialize thread to start and acquire InitLock
      }

      /// <summary>
      /// Returns an object able to load modeling information.  This will be called automatically when the modeler is initialized.
      /// </summary>
      public virtual IModelerLoader ModelerLoader
      {
         get
         {
            return new DBModelerLoader(Adapter);
         }
      }

      /// <summary>
      /// Takes part of an ingredient name and returns possible matches, useful for autocomplete UIs.
      /// </summary>
      /// <param name="query">Part or all of an ingredient name.  Must be at least three characters.</param>
      /// <returns>An enumeration of IngredientNode objects describing possible matches and their IDs.</returns>
      public virtual IEnumerable<IngredientNode> AutocompleteIngredient(string query)
      {
         using (InitLock.ReadLock())
         {
            return ingParser.MatchIngredient(query);
         }
      }

      /// <summary>
      /// Attempts to parse an ingredient usage using natural language processing (NLP).
      /// </summary>
      /// <param name="input">An ingredient usage, such as "2 eggs" or "1/4 cup of shredded cheese"</param>
      /// <returns>A Result object indicating if the usage could be parsed, and if so, the normalized ingredient usage information.</returns>
      public virtual Result ParseIngredientUsage(string input)
      {
         return Parser.Parse(input);
      }

      /// <summary>
      /// Creates a new recipe modeling session.  Recipe modeling allows the user to generate optimal sets of recipes based on given ingredient usage and criteria.
      /// </summary>
      /// <param name="profile">A profile for the current user.  Pass in UserProfile.Anonymous to indicate a generic user.</param>
      /// <returns>A modeling session able to generate and compile recipe sets based on the given profile.</returns>
      public virtual ModelingSession CreateModelingSession(IUserProfile profile)
      {
         return ModelerProxy.CreateSession(profile);
      }

      /// <summary>
      /// Attempts to parse an ingredient by name using natural language processing (NLP).  A single ingredient might have various synonyms, spellings, etc.
      /// </summary>
      /// <param name="input">The name of an ingredient.</param>
      /// <returns>A KitchenPC Ingredient object, or null if no matching ingredient was found.</returns>
      public virtual Ingredient ParseIngredient(string input)
      {
         if (input == null)
            throw new ArgumentNullException("input");

         var result = ParseIngredientUsage(input.Trim());
         if (result is Match)
         {
            return result.Usage.Ingredient;
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// Searches for recipes matching the specified criteria.
      /// </summary>
      /// <param name="query">A RecipeQuery object indicating the recipes to match.</param>
      /// <returns>A SearchResults object containing recipe briefs and a total count.</returns>
      public virtual SearchResults RecipeSearch(RecipeQuery query)
      {
         return Adapter.RecipeSearch(Identity, query);
      }

      /// <summary>
      /// Reads full information for one or more recipes in the database.
      /// </summary>
      /// <param name="recipeIds">An array containing recipe IDs to load.</param>
      /// <param name="options">Indicates which properties to load.  Use ReadRecipeOptions.None to only load base recipe data.</param>
      /// <returns></returns>
      public virtual Recipe[] ReadRecipes(Guid[] recipeIds, ReadRecipeOptions options)
      {
         return Adapter.ReadRecipes(Identity, recipeIds, options);
      }

      /// <summary>
      /// Removes one or more recipes from the user's recipe queue.
      /// </summary>
      /// <param name="recipeIds">A list of recipe IDs to remove from the queue.  IDs not in the queue will be ignored.</param>
      public virtual void DequeueRecipe(params Guid[] recipeIds)
      {
         Adapter.DequeueRecipe(Identity, recipeIds);
      }

      /// <summary>
      /// Adds one or more recipes to the user's recipe queue.
      /// </summary>
      /// <param name="recipeIds">A list of recipe IDs to add to the queue.  IDs already in the queue will be ignored.</param>
      public virtual void EnqueueRecipes(Guid[] recipeIds)
      {
         Adapter.EnqueueRecipes(Identity, recipeIds);
      }

      /// <summary>
      /// Returns the user's current recipe queue.
      /// </summary>
      /// <returns>An array of RecipeBrief objects for each recipe in the queue</returns>
      public virtual RecipeBrief[] GetRecipeQueue()
      {
         return Adapter.GetRecipeQueue(Identity);
      }

      /// <summary>
      /// Associates a rating with the current user and a specified recipe.
      /// </summary>
      /// <param name="recipeId">Recipe ID to rate</param>
      /// <param name="rating">Rating to give this recipe</param>
      public virtual void RateRecipe(Guid recipeId, Rating rating)
      {
         Adapter.RateRecipe(Identity, recipeId, rating);
      }

      /// <summary>
      /// Creates a new recipe.
      /// </summary>
      /// <param name="recipe">Fully constructed Recipe object.</param>
      public RecipeResult CreateRecipe(Recipe recipe)
      {
         Recipe.Validate(recipe);

         return Adapter.CreateRecipe(Identity, recipe);
      }

      /// <summary>
      /// Reads the available forms for the given ingredient ID.  Forms indicate ways an ingredient might be used within a recipe, such as "chopped", "sliced" or "melted".
      /// </summary>
      /// <param name="id">An ingredient ID</param>
      /// <returns>An IngredientFormsCollection object containing an array of ingredient forms.</returns>
      public virtual IngredientFormsCollection ReadFormsForIngredient(Guid id)
      {
         return Adapter.ReadFormsForIngredient(id);
      }

      /// <summary>
      /// Returns ingredient information, such as ID, metadata, unit information, etc.
      /// </summary>
      /// <param name="ingredient">The name of an ingredient.  This must be an exact match.</param>
      /// <returns>A KitchenPC Ingredient object, or null if no matching ingredient was found.</returns>
      public virtual Ingredient ReadIngredient(string ingredient)
      {
         return Adapter.ReadIngredient(ingredient);
      }

      /// <summary>
      /// Returns ingredient information, such as ID, metadata, unit information, etc.
      /// </summary>
      /// <param name="ingid">The ID of the ingredient.</param>
      /// <returns>A KitchenPC Ingredient object, or null if no matching ingredient was found.</returns>
      public virtual Ingredient ReadIngredient(Guid ingid)
      {
         return Adapter.ReadIngredient(ingid);
      }

      /// <summary>
      /// Converts a usage of an ingredient within a recipe to an IngredientAggregation object, suitable for aggregating with other usages of the same ingredient.
      /// </summary>
      /// <param name="usage">An IngredientUsage object, usually from a recipe.</param>
      /// <returns>An IngredientAggregation object, usually to be combined with other uses of that ingredient to form a shopping list.</returns>
      public virtual IngredientAggregation ConvertIngredientUsage(IngredientUsage usage)
      {
         //TODO: Does this method need to be part of the context?  Perhaps IngredientUsage should have a method to convert to an aggregation

         var ing = ReadIngredient(usage.Ingredient.Id);
         if (ing == null)
            throw new IngredientNotFoundException();

         var a = new IngredientAggregation(ing);
         a.AddUsage(usage);

         return a;
      }

      /// <summary>
      /// Aggregates one or more IngredientUsage objects.
      /// </summary>
      /// <param name="usages">IngredientUsage objects, usually from one or more recipes</param>
      /// <returns>A list of IngredientAggregation objects, one per unique ingredient in the set of recipes</returns>
      public virtual IList<IngredientAggregation> AggregateIngredients(params IngredientUsage[] usages)
      {
         var ings = new Dictionary<Guid, IngredientAggregation>();

         foreach (var usage in usages)
         {
            IngredientAggregation agg;

            if (!ings.TryGetValue(usage.Ingredient.Id, out agg))
            {
               ings.Add(usage.Ingredient.Id, new IngredientAggregation(usage.Ingredient, usage.Amount));
               continue;
            }

            //TODO: If usage.Unit is different than agg.Amount.Unit then we have a problem, throw an exception if that happens?
            if (agg.Amount == null) //This aggregation contains an empty amount, so we can't aggregate
               continue;
            else if (usage.Amount == null) //This amount is null, cancel aggregation
               agg.Amount = null;
            else
               agg.Amount += usage.Amount;
         }

         return ings.Values.ToList();
      }

      /// <summary>
      /// Aggregates one or more recipes into a set of aggregated ingredients.  This is normally used to create a list of items needed to buy for multiple recipes without having to create a shopping list.
      /// </summary>
      /// <param name="recipeIds">A list of recipe IDs to aggregate.</param>
      /// <returns>A list of IngredientAggregation objects, one per unique ingredient in the set of recipes</returns>
      public virtual IList<IngredientAggregation> AggregateRecipes(params Guid[] recipeIds)
      {
         using (InitLock.ReadLock())
         {
            var ings = new Dictionary<Guid, IngredientAggregation>(); //List of all ingredients and total usage

            foreach (var id in recipeIds)
            {
               //Lookup ingredient through modeler cache
               var rNode = modeler.FindRecipe(id);
               if (rNode == null) //Our cache is out of date, skip this result
                  continue;

               foreach (var usage in rNode.Ingredients)
               {
                  var ingId = usage.Ingredient.IngredientId;
                  var ingName = ingParser.GetIngredientById(ingId);
                  var ing = new Ingredient(ingId, ingName);
                  ing.ConversionType = usage.Ingredient.ConvType;

                  IngredientAggregation agg;
                  if (!ings.TryGetValue(ingId, out agg))
                  {
                     ings.Add(ingId, agg = new IngredientAggregation(ing)
                     {
                        Amount = new Amount(0, usage.Unit)
                     });
                  }

                  //TODO: If usage.Unit is different than agg.Amount.Unit then we have a problem, throw an exception if that happens?
                  if (agg.Amount == null) //This aggregation contains an empty amount, so we can't aggregate
                     continue;
                  else if (!usage.Amt.HasValue) //This amount is null, cancel aggregation
                     agg.Amount = null;
                  else
                     agg.Amount += usage.Amt.Value;
               }
            }

            return ings.Values.ToList();
         }
      }

      /// <summary>
      /// Returns the specified set of menus owned by the current user.
      /// </summary>
      /// <param name="menus">One or more Menu objects.  Use Menu.Favorites to load the default favorites menu.</param>
      /// <param name="options">Specifies what data to load.</param>
      /// <returns>An array of Menu objects with the specified data loaded.</returns>
      public virtual Menu[] GetMenus(IList<Menu> menus, GetMenuOptions options)
      {
         return Adapter.GetMenus(Identity, menus, options);
      }

      /// <summary>
      /// Updates a specified menu owned by the current user.
      /// </summary>
      /// <param name="menuId">The Menu ID to update, or null to update the Favorites menu.</param>
      /// <param name="recipesAdd">A list of recipe IDs to add to the menu.  Duplicates will be ignored.</param>
      /// <param name="recipesRemove">A list of recipe IDs to remove from the menu.</param>
      /// <param name="recipesMove">A list of items to move from this menu to another menu.</param>
      /// <param name="clear">If true, all recipes will be removed from this menu.</param>
      /// <param name="newName">An optional new name for this menu.  Note, the favorites menu cannot be renamed.</param>
      /// <returns></returns>
      public virtual MenuResult UpdateMenu(Guid? menuId, Guid[] recipesAdd, Guid[] recipesRemove, MenuMove[] recipesMove, bool clear, string newName = null)
      {
         return Adapter.UpdateMenu(Identity, menuId, recipesAdd, recipesRemove, recipesMove, clear, newName);
      }

      /// <summary>
      /// Created a new menu owned by the current user.
      /// </summary>
      /// <param name="menu">A menu to create.</param>
      /// <param name="recipeIds">Zero or more recipes to add to the newly created menu.</param>
      /// <returns>A result indicating the new menu ID.</returns>
      public virtual MenuResult CreateMenu(Menu menu, params Guid[] recipeIds)
      {
         return Adapter.CreateMenu(Identity, menu, recipeIds);
      }

      /// <summary>
      /// Deletes one or more menus owned by the current user.
      /// </summary>
      /// <param name="menuIds">One or more menus to delete.  Note, the Favorites menu cannot be deleted.</param>
      public virtual void DeleteMenus(params Guid[] menuIds)
      {
         Adapter.DeleteMenus(Identity, menuIds);
      }

      /// <summary>
      /// Creates a new shopping list for the current user.
      /// </summary>
      /// <param name="name">The name of the new shopping list.</param>
      /// <param name="recipes">Zero or more recipes to add to this list.</param>
      /// <param name="ingredients">Zero or more ingredients to add to this list.</param>
      /// <param name="usages">Zero or more ingredient usages to add to this list.</param>
      /// <param name="items">Zero or more raw usages.  Raw usages will be parsed using NLP, and unsuccessful matches will be added to the list as raw items.</param>
      /// <returns>A fully aggregated shopping list, with like items combined and forms normalized.</returns>
      public virtual ShoppingListResult CreateShoppingList(string name, Recipe[] recipes, Ingredient[] ingredients, IngredientUsage[] usages, String[] items)
      {
         var parsedIng = Parser.ParseAll(items).ToList();

         var recipeAgg = AggregateRecipes(recipes.Select(r => r.Id).ToArray());
         var ingAgg = ingredients.Select(i => new IngredientAggregation(i, null));
         var ingUsages = AggregateIngredients(usages);
         var parsedUsages = AggregateIngredients(parsedIng.Where(u => u is Match).Select(u => u.Usage).ToArray());
         var rawInputs = parsedIng.Where(u => u is NoMatch).Select(u => new ShoppingListItem(u.Input));

         var allItems = recipeAgg
            .Concat(ingAgg)
            .Concat(ingUsages)
            .Concat(parsedUsages)
            .Concat(rawInputs);

         var list = new ShoppingList(null, name, allItems);
         return CreateShoppingList(list);
      }

      /// <summary>
      /// Creates a new shopping list for the current user.
      /// </summary>
      /// <param name="list">A ShoppingList object containing a normalized shopping list.</param>
      /// <returns>A result indicating the ID assigned to the newly created list.</returns>
      public virtual ShoppingListResult CreateShoppingList(ShoppingList list)
      {
         return Adapter.CreateShoppingList(Identity, list);
      }

      /// <summary>
      /// Updates a shopping list.
      /// </summary>
      /// <param name="list">A shopping list owned by the current user.</param>
      /// <param name="updates">A set of update commands indicating how the shopping list should be updated.</param>
      /// <param name="newName">An optional new name for this shopping list.</param>
      /// <returns></returns>
      public virtual ShoppingListResult UpdateShoppingList(ShoppingList list, ShoppingListUpdateCommand[] updates, string newName = null)
      {
         // Aggregate new items
         var parsedIng = Parser.ParseAll(updates.Where(u => !String.IsNullOrWhiteSpace(u.NewRaw)).Select(r => r.NewRaw).ToArray()).ToList();

         var recipeAgg = AggregateRecipes(updates.Where(u => u.NewRecipe != null)
            .Select(r => r.NewRecipe.Id).ToArray());

         var ingAgg = updates.Where(u => u.NewIngredient != null)
            .Select(i => new IngredientAggregation(i.NewIngredient, null));

         var ingUsages = AggregateIngredients(updates.Where(u => u.NewUsage != null)
            .Select(u => u.NewUsage).ToArray());

         var parsedUsages = AggregateIngredients(parsedIng.Where(u => u is Match)
            .Select(u => u.Usage).ToArray());

         var rawInputs = parsedIng.Where(u => u is NoMatch)
            .Select(u => new ShoppingListItem(u.Input));

         var newItems = recipeAgg
            .Concat(ingAgg)
            .Concat(ingUsages)
            .Concat(parsedUsages)
            .Concat(rawInputs)
            .ToArray();

         var removedItems = updates.Where(u => u.Command == ShoppingListUpdateType.RemoveItem).Select(i => i.RemoveItem.Value).ToArray();
         var modifiedItems = updates.Where(u => u.Command == ShoppingListUpdateType.ModifyItem).Select(i => i.ModifyItem).ToArray();

         return Adapter.UpdateShoppingList(Identity, list.Id, removedItems, modifiedItems, newItems, newName);
      }

      /// <summary>
      /// Deletes one or more shopping lists owned by the current user.
      /// </summary>
      /// <param name="lists">One or more shopping lists to delete.  Note, the default shopping list cannot be deleted.</param>
      public virtual void DeleteShoppingLists(ShoppingList[] lists)
      {
         Adapter.DeleteShoppingLists(Identity, lists);
      }

      /// <summary>
      /// Returns one or more saved shopping lists from the current user.
      /// </summary>
      /// <param name="lists">A list of ShoppingList objects indicating the ID of the list to load, or ShoppingList.Default for the default list.</param>
      /// <param name="options">Indicates what data to load.  Use GetShoppingListOptions.None to simply load the names of the lists.</param>
      /// <returns>An array of ShoppingList objects with all the desired properties loaded.</returns>
      public virtual ShoppingList[] GetShoppingLists(IList<ShoppingList> lists, GetShoppingListOptions options)
      {
         return Adapter.GetShoppingLists(Identity, lists, options);
      }

      protected virtual void LoadTemplates()
      {
         parser.LoadTemplates(
            new Template("[ING]") {AllowPartial = true}, //Allow partial ingredient parsing (such as "eggs")

            "[ING]: [AMT] [UNIT]", //cheddar cheese: 5 cups
            "[ING]: [AMT]", //eggs: 5
            "[FORM] [ING]: [AMT]", //shredded cheddar cheese: 1 cup
            "[FORM] [ING]: [AMT] [UNIT]", //shredded cheddar cheese: 1 cup

            "[AMT] [UNIT] [FORM] [ING]", //5 cups melted cheddar cheese
            "[AMT] [UNIT] [ING]", //5 cups cheddar cheese
            "[AMT] [UNIT] of [ING]", //5 cups of cheddar cheese
            "[AMT] [UNIT] of [FORM] [ING]", //two cups of shredded cheddar cheese
            "[AMT] [ING]", //5 eggs

            "[AMT] [UNIT] [FORM], [ING]", // 2 cups chopped, unsalted peanuts
            //"[AMT] [UNIT] [ING], [FORM]",   // 5 cups flour, sifted
            //"[AMT] [UNIT] [ING] [FORM]",    // 1 cup graham cracker crumbs
            //"[AMT] [ING] [FORM]",           // 5 graham cracker squares
            //"[AMT] [ING], [FORM]",          // 3 bananas, sliced
            "[AMT] [FORM] [ING]", // 3 mashed bananas

            //"[AMT] [ING] ([FORM])",         // 4 bananas (mashed)
            //"[AMT] [UNIT] [ING] ([FORM])",  // 5 cups flour (sifted)

            // ----- Prep notes
            "[AMT] [ING], [PREP]", //1 carrot, shredded
            "[AMT] [UNIT] [ING], [PREP]", //1 cup butter, melted
            "[AMT] [UNIT] [FORM] [ING], [PREP]", //1 cup chopped walnuts, toasted
            "[AMT] [ING] - [PREP]", //1 carrot - shredded
            "[AMT] [UNIT] [ING] - [PREP]", //1 cup butter - melted

            new Template("[AMT] [UNIT] [ING] (optional)") {DefaultPrep = "optional"}, // 1 cup brown sugar (optional)
            new Template("[AMT] [UNIT] [FORM] [ING] (optional)") {DefaultPrep = "optional"}, // 1 cup chopped walnuts (optional)
            //new Template("[AMT] [UNIT] [ING], [FORM] (optional)") { DefaultPrep = "optional" }, // 1 cup walnuts, chopped (optional)

            new Template("[AMT] [UNIT] [ING] (divided)") {DefaultPrep = "divided"}, // 1 cup brown sugar (divided)
            new Template("[AMT] [UNIT] [ING], divided") {DefaultPrep = "divided"}, // 1 cup brown sugar, divided

            // ----- anomalies
            "[AMT] [UNIT] [ANOMALY]", // 1 cup graham cracker crumbs
            "[AMT] [UNIT] [ANOMALY], [PREP]", // 1 cup graham cracker crumbs, divided
            "[AMT] [UNIT] [ANOMALY] - [PREP]", // 1 cup graham cracker crumbs - optional
            "[AMT] [ANOMALY]", // 1 chocolate bar square
            "[AMT] [ANOMALY], [PREP]", // 1 chocolate bar square, melted
            "[AMT] [ANOMALY] - [PREP]" // 1 chocolate bar square - melted
            );
      }

      public DataStore Export()
      {
         return Adapter.Export();
      }

      public void Import(IProvisionSource source)
      {
         Adapter.Import(source);
      }

      /// <summary>Creates the default database schema in the configured database.</summary>
      public void InitializeStore()
      {
         Adapter.InitializeStore();
      }
   }
}