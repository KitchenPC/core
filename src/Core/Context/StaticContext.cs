using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;
using KitchenPC.Context.Fluent;
using KitchenPC.Data;
using KitchenPC.Data.DTO;
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
   public class StaticContext : IKPCContext, IProvisionTarget, IProvisionSource
   {
      readonly StaticContextBuilder builder;
      public string DataDirectory { get; set; }
      public bool CompressedStore { get; set; }
      public Func<AuthIdentity> GetIdentity { get; set; }
      public Parser Parser { get; private set; }
      public ModelerProxy ModelerProxy { get; private set; }

      DataStore store;
      IngredientParser ingParser;
      ISearchProvider searchProvder;

      /// <summary>
      /// Returns a StaticContextBuilder used to configure a StaticContext instance.
      /// </summary>
      public static StaticContextBuilder Configure
      {
         get
         {
            return new StaticContext().builder;
         }
      }

      StaticContext()
      {
         builder = new StaticContextBuilder(this);
      }

      /// <summary>
      /// Initializes the context and loads necessary data into memory through the configured data file.
      /// This must be done before the context is usable.
      /// </summary>
      public void Initialize()
      {
         var file = CompressedStore ? "KPCData.gz" : "KPCData.xml";
         var path = Path.Combine(DataDirectory, file);
         KPCContext.Log.DebugFormat("Attempting to open local data file: {0}", path);

         if (!File.Exists(path))
            throw new FileNotFoundException("Could not initialize StaticContext.  Data file not found.", path);

         var serializer = new XmlSerializer(typeof (DataStore));
         using (var fileReader = new FileStream(path, FileMode.Open))
         {
            if (CompressedStore)
            {
               using (var reader = new GZipStream(fileReader, CompressionMode.Decompress))
               {
                  store = serializer.Deserialize(reader) as DataStore;
               }
            }
            else
            {
               store = serializer.Deserialize(fileReader) as DataStore;
            }

            if (store == null)
               throw new DataStoreException("Could not deserialize data store.  It might be correct or an invalid format.");
         }

         // Initialize ingredient parser
         ingParser = new IngredientParser();
         ingParser.CreateIndex(store.Ingredients.Select(i => new IngredientSource(i.IngredientId, i.DisplayName)));

         // Initialize modeler
         ModelerProxy = new ModelerProxy(this);
         ModelerProxy.LoadSnapshot();

         // Initialize natural language parsing
         IngredientSynonyms.InitIndex(new StaticIngredientLoader(store));
         UnitSynonyms.InitIndex(new StaticUnitLoader(store));
         FormSynonyms.InitIndex(new StaticFormLoader(store));
         PrepNotes.InitIndex(new StaticPrepLoader(store));
         Anomalies.InitIndex(new StaticAnomalyLoader(store));
         NumericVocab.InitIndex();

         Parser = new Parser();
         LoadTemplates();
      }

      /// <summary>
      /// Returns the identity of the current user using the GetIdentity function.
      /// </summary>
      public AuthIdentity Identity
      {
         get
         {
            return GetIdentity();
         }
      }

      /// <summary>
      /// Takes part of an ingredient name and returns possible matches, useful for autocomplete UIs.
      /// </summary>
      /// <param name="query">Part or all of an ingredient name.  Must be at least three characters.</param>
      /// <returns>An enumeration of IngredientNode objects describing possible matches and their IDs.</returns>
      public IEnumerable<IngredientNode> AutocompleteIngredient(string query)
      {
         return ingParser.MatchIngredient(query);
      }

      /// <summary>
      /// Returns an object able to load modeling information.  This will be called automatically when the modeler is initialized.
      /// </summary>
      public IModelerLoader ModelerLoader
      {
         get
         {
            return new StaticModelerLoader(store);
         }
      }

      /// <summary>
      /// Creates a new recipe modeling session.  Recipe modeling allows the user to generate optimal sets of recipes based on given ingredient usage and criteria.
      /// </summary>
      /// <param name="profile">A profile for the current user.  Pass in UserProfile.Anonymous to indicate a generic user.</param>
      /// <returns>A modeling session able to generate and compile recipe sets based on the given profile.</returns>
      public ModelingSession CreateModelingSession(IUserProfile profile)
      {
         return ModelerProxy.CreateSession(profile);
      }

      /// <summary>
      /// Attempts to parse an ingredient usage using natural language processing (NLP).
      /// </summary>
      /// <param name="input">An ingredient usage, such as "2 eggs" or "1/4 cup of shredded cheese"</param>
      /// <returns>A Result object indicating if the usage could be parsed, and if so, the normalized ingredient usage information.</returns>
      public Result ParseIngredientUsage(string input)
      {
         return Parser.Parse(input);
      }

      /// <summary>
      /// Attempts to parse an ingredient by name using natural language processing (NLP).  A single ingredient might have various synonyms, spellings, etc.
      /// </summary>
      /// <param name="input">The name of an ingredient.</param>
      /// <returns>A KitchenPC Ingredient object, or null if no matching ingredient was found.</returns>
      public Ingredient ParseIngredient(string input)
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
      public SearchResults RecipeSearch(RecipeQuery query)
      {
         if (searchProvder == null)
            searchProvder = new StaticSearch(store);

         return searchProvder.Search(Identity, query);
      }

      /// <summary>
      /// Reads full information for one or more recipes in the database.
      /// </summary>
      /// <param name="recipeIds">An array containing recipe IDs to load.</param>
      /// <param name="options">Indicates which properties to load.  Use ReadRecipeOptions.None to only load base recipe data.</param>
      /// <returns></returns>
      public Recipe[] ReadRecipes(Guid[] recipeIds, ReadRecipeOptions options)
      {
         var recipes = store.Recipes.Where(r => recipeIds.Contains(r.RecipeId)).ToList();

         if (!recipes.Any())
            throw new RecipeNotFoundException();

         var riIndex = store.GetIndexedRecipeIngredients();
         var ingIndex = store.GetIndexedIngredients();
         var formIndex = store.GetIndexedIngredientForms();
         var rmetaIndex = store.GetIndexedRecipeMetadata();
         var imetaIndex = store.GetIndexedIngredientMetadata();

         var ret = new List<Recipe>();
         foreach (var r in recipes)
         {
            var recipe = new Recipe
            {
               Id = r.RecipeId,
               Title = r.Title,
               Description = r.Description,
               DateEntered = r.DateEntered,
               ImageUrl = r.ImageUrl,
               ServingSize = r.ServingSize,
               PrepTime = r.PrepTime,
               CookTime = r.CookTime,
               Credit = r.Credit,
               CreditUrl = r.CreditUrl,
               AvgRating = r.Rating
            };

            if (options.ReturnMethod)
               recipe.Method = r.Steps;

            if (options.ReturnUserRating)
            {
               var userRating = store.RecipeRatings.SingleOrDefault(ur => (ur.RecipeId == r.RecipeId && ur.UserId == Identity.UserId));
               recipe.UserRating = userRating != null ? (Rating) userRating.Rating : Rating.None;
            }

            recipe.Ingredients = riIndex[r.RecipeId].Select(i => new IngredientUsage
            {
               Amount = i.Qty.HasValue ? new Amount(i.Qty.Value, i.Unit) : null,
               PrepNote = i.PrepNote,
               Section = i.Section,
               Form = i.IngredientFormId.HasValue ? IngredientForms.ToIngredientForm(formIndex[i.IngredientFormId.Value]) : null,
               Ingredient = Data.DTO.Ingredients.ToIngredient(ingIndex[i.IngredientId], imetaIndex[i.IngredientId])
            }).ToArray();

            recipe.Tags = RecipeMetadata.ToRecipeTags(rmetaIndex[r.RecipeId]);
            ret.Add(recipe);
         }

         return ret.ToArray();
      }

      /// <summary>
      /// Associates a rating with the current user and a specified recipe.
      /// </summary>
      /// <param name="recipeId">Recipe ID to rate</param>
      /// <param name="rating">Rating to give this recipe</param>
      public void RateRecipe(Guid recipeId, Rating rating)
      {
         var userRating = store.RecipeRatings.SingleOrDefault(ur => (ur.RecipeId == recipeId && ur.UserId == Identity.UserId));
         if (userRating == null)
         {
            store.RecipeRatings.Add(new RecipeRatings
            {
               RatingId = Guid.NewGuid(),
               RecipeId = recipeId,
               UserId = Identity.UserId,
               Rating = (short) rating
            });
         }
         else
         {
            userRating.Rating = (short) rating;
         }
      }

      /// <summary>
      /// Creates a new recipe.
      /// </summary>
      /// <param name="recipe">Fully constructed Recipe object.</param>
      public RecipeResult CreateRecipe(Recipe recipe)
      {
         Recipe.Validate(recipe);
         recipe.Id = Guid.NewGuid();

         // TODO: We should update indexes rather than clear them all out, however this context isn't designed for performance
         store.ClearIndexes();

         // Recipes
         store.Recipes.Add(new Data.DTO.Recipes
         {
            RecipeId = recipe.Id,
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
         });

         // RecipeIngredients
         short displayOrder = 0;
         recipe.Ingredients.ForEach(i =>
         {
            store.RecipeIngredients.Add(new RecipeIngredients
            {
               RecipeIngredientId = Guid.NewGuid(),
               RecipeId = recipe.Id,
               IngredientId = i.Ingredient.Id,
               IngredientFormId = (i.Form != null ? (Guid?) i.Form.FormId : null),
               Qty = (i.Amount != null ? (float?) i.Amount.SizeHigh : null),
               QtyLow = (i.Amount != null ? (float?) i.Amount.SizeLow : null),
               Unit = (i.Amount != null ? i.Amount.Unit : Units.Unit),
               Section = i.Section,
               DisplayOrder = ++displayOrder
            });
         });

         // RecipeMetadata
         store.RecipeMetadata.Add(new RecipeMetadata
         {
            RecipeMetadataId = Guid.NewGuid(),
            RecipeId = recipe.Id,
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
         });

         return new RecipeResult
         {
            RecipeCreated = true,
            NewRecipeId = recipe.Id
         };
      }

      /// <summary>
      /// Removes one or more recipes from the user's recipe queue.
      /// </summary>
      /// <param name="recipeIds">A list of recipe IDs to remove from the queue.  IDs not in the queue will be ignored.</param>
      public void DequeueRecipe(params Guid[] recipeIds)
      {
         var existing = store.QueuedRecipes
            .Where(r => r.UserId == Identity.UserId)
            .Where(r => recipeIds.Contains(r.RecipeId));

         store.QueuedRecipes.RemoveAll(existing.Contains);
      }

      /// <summary>
      /// Adds one or more recipes to the user's recipe queue.
      /// </summary>
      /// <param name="recipeIds">A list of recipe IDs to add to the queue.  IDs already in the queue will be ignored.</param>
      public void EnqueueRecipes(params Guid[] recipeIds)
      {
         var existing = store.QueuedRecipes
            .Where(r => r.UserId == Identity.UserId)
            .Where(r => recipeIds.Contains(r.RecipeId))
            .Select(r => r.RecipeId);

         var noDupes = recipeIds.Except(existing).ToList();
         if (!noDupes.Any())
            return;

         store.QueuedRecipes.AddRange(noDupes.Select(r => new QueuedRecipes
         {
            QueueId = Guid.NewGuid(),
            QueuedDate = DateTime.Now,
            UserId = Identity.UserId,
            RecipeId = r
         }));
      }

      /// <summary>
      /// Returns the user's current recipe queue.
      /// </summary>
      /// <returns>An array of RecipeBrief objects for each recipe in the queue</returns>
      public RecipeBrief[] GetRecipeQueue()
      {
         var rIndex = store.GetIndexedRecipes();
         var queue = store.QueuedRecipes.Where(q => q.UserId == Identity.UserId);

         return queue
            .Select(item => rIndex[item.RecipeId])
            .Select(Data.DTO.Recipes.ToRecipeBrief)
            .ToArray();
      }

      /// <summary>
      /// Reads the available forms for the given ingredient ID.  Forms indicate ways an ingredient might be used within a recipe, such as "chopped", "sliced" or "melted".
      /// </summary>
      /// <param name="id">An ingredient ID</param>
      /// <returns>An IngredientFormsCollection object containing an array of ingredient forms.</returns>
      public IngredientFormsCollection ReadFormsForIngredient(Guid id)
      {
         var forms = store.IngredientForms
            .Where(f => f.IngredientId == id)
            .Select(IngredientForms.ToIngredientForm);

         return new IngredientFormsCollection(forms);
      }

      /// <summary>
      /// Returns ingredient information, such as ID, metadata, unit information, etc.
      /// </summary>
      /// <param name="ingredient">The name of an ingredient.  This must be an exact match.</param>
      /// <returns>A KitchenPC Ingredient object, or null if no matching ingredient was found.</returns>
      public Ingredient ReadIngredient(string ingredient)
      {
         var ing = store.Ingredients.FirstOrDefault(i => String.Compare(i.DisplayName, ingredient, StringComparison.OrdinalIgnoreCase) == 0);
         return ing == null ? null : Data.DTO.Ingredients.ToIngredient(ing);
      }

      /// <summary>
      /// Returns ingredient information, such as ID, metadata, unit information, etc.
      /// </summary>
      /// <param name="ingid">The ID of the ingredient.</param>
      /// <returns>A KitchenPC Ingredient object, or null if no matching ingredient was found.</returns>
      public Ingredient ReadIngredient(Guid ingid)
      {
         var ingIndex = store.GetIndexedIngredients();
         Data.DTO.Ingredients dtoIng;

         return ingIndex.TryGetValue(ingid, out dtoIng) ? Data.DTO.Ingredients.ToIngredient(dtoIng) : null;
      }

      /// <summary>
      /// Converts a usage of an ingredient within a recipe to an IngredientAggregation object, suitable for aggregating with other usages of the same ingredient.
      /// </summary>
      /// <param name="usage">An IngredientUsage object, usually from a recipe.</param>
      /// <returns>An IngredientAggregation object, usually to be combined with other uses of that ingredient to form a shopping list.</returns>
      public IngredientAggregation ConvertIngredientUsage(IngredientUsage usage)
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
      /// Returns one or more saved shopping lists from the current user.
      /// </summary>
      /// <param name="lists">A list of ShoppingList objects indicating the ID of the list to load, or ShoppingList.Default for the default list.</param>
      /// <param name="options">Indicates what data to load.  Use GetShoppingListOptions.None to simply load the names of the lists.</param>
      /// <returns>An array of ShoppingList objects with all the desired properties loaded.</returns>
      public ShoppingList[] GetShoppingLists(IList<ShoppingList> lists, GetShoppingListOptions options)
      {
         var loadDef = true;
         var query = store.ShoppingLists
            .Where(p => p.UserId == Identity.UserId);

         if (lists != null) // Load individual lists
         {
            loadDef = lists.Contains(ShoppingList.Default);
            var ids = lists.Where(l => l.Id.HasValue).Select(l => l.Id.Value).ToArray();
            query = query.Where(p => ids.Contains(p.ShoppingListId));
         }

         var dbLists = query.ToList();

         if (!options.LoadItems) // We're done!
         {
            return (loadDef ? new ShoppingList[] {ShoppingList.Default} : new ShoppingList[0])
               .Concat(dbLists.Select(Data.DTO.ShoppingLists.ToShoppingList))
               .ToArray();
         }

         // All user's shopping list items
         var dbItems = store.ShoppingListItems
            .Where(p => p.UserId == Identity.UserId)
            .ToList();

         var indexRecipes = store.GetIndexedRecipes();
         var indexIngredients = store.GetIndexedIngredients();

         var ret = new List<ShoppingList>();
         if (loadDef) ret.Add(ShoppingList.Default);
         ret.AddRange(dbLists.Select(Data.DTO.ShoppingLists.ToShoppingList));

         // Add items to list
         foreach (var list in ret)
         {
            var itemsInList = dbItems.Where(p => p.ShoppingListId.Equals(list.Id));
            var items = itemsInList.Select(item =>
               new ShoppingListItem(item.ItemId)
               {
                  Raw = item.Raw,
                  Ingredient = item.IngredientId.HasValue ? Data.DTO.Ingredients.ToIngredient(indexIngredients[item.IngredientId.Value]) : null,
                  Recipe = item.RecipeId.HasValue ? Data.DTO.Recipes.ToRecipeBrief(indexRecipes[item.RecipeId.Value]) : null,
                  CrossedOut = item.CrossedOut,
                  Amount = (item.Qty.HasValue && item.Unit.HasValue) ? new Amount(item.Qty.Value, item.Unit.Value) : null
               });

            list.AddItems(items.ToList());
         }

         return ret.ToArray();
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
      public ShoppingListResult CreateShoppingList(string name, Recipe[] recipes, Ingredient[] ingredients, IngredientUsage[] usages, string[] items)
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
      public ShoppingListResult CreateShoppingList(ShoppingList list)
      {
         var ret = new ShoppingListResult();
         var dbList = new Data.DTO.ShoppingLists
         {
            ShoppingListId = Guid.NewGuid(),
            Title = list.Title.Trim(),
            UserId = Identity.UserId
         };

         store.ShoppingLists.Add(dbList);

         if (list.Any()) // Create ShoppingListItems
         {
            list.ToList().ForEach(i => store.ShoppingListItems.Add(new ShoppingListItems
            {
               ItemId = Guid.NewGuid(),
               UserId = Identity.UserId,
               ShoppingListId = dbList.ShoppingListId,
               CrossedOut = i.CrossedOut,
               IngredientId = i.Ingredient != null ? (Guid?) i.Ingredient.Id : null,
               RecipeId = i.Recipe != null ? (Guid?) i.Recipe.Id : null,
               Raw = i.Raw,
               Qty = i.Amount != null ? (float?) i.Amount.SizeHigh : null,
               Unit = i.Amount != null ? (Units?) i.Amount.Unit : null
            }));
         }

         ret.NewShoppingListId = dbList.ShoppingListId;
         ret.List = list;
         return ret;
      }

      /// <summary>
      /// Updates a shopping list.
      /// </summary>
      /// <param name="list">A shopping list owned by the current user.</param>
      /// <param name="updates">A set of update commands indicating how the shopping list should be updated.</param>
      /// <param name="newName">An optional new name for this shopping list.</param>
      /// <returns></returns>
      public ShoppingListResult UpdateShoppingList(ShoppingList list, ShoppingListUpdateCommand[] updates, string newName = null)
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
            .Concat(rawInputs);

         var removedItems = updates.Where(u => u.Command == ShoppingListUpdateType.RemoveItem).Select(i => i.RemoveItem.Value).ToArray();
         var modifiedItems = updates.Where(u => u.Command == ShoppingListUpdateType.ModifyItem).Select(i => i.ModifyItem);

         return UpdateShoppingList(list.Id, removedItems, modifiedItems, newItems, newName);
      }

      ShoppingListResult UpdateShoppingList(Guid? listId, Guid[] toRemove, IEnumerable<ShoppingListModification> toModify, IEnumerable<IShoppingListSource> toAdd, string newName = null)
      {
         // Deletes
         if (toRemove.Any())
         {
            var dbDeletes = store.ShoppingListItems
               .Where(p => p.UserId == Identity.UserId)
               .Where(p => p.ShoppingListId == listId)
               .Where(p => toRemove.Contains(p.ItemId));

            store.ShoppingListItems.RemoveAll(dbDeletes.Contains);
         }

         // Updates
         Guid? shoppingListId = null;
         Data.DTO.ShoppingLists dbList = null;
         List<ShoppingListItems> dbItems = null;
         if (listId.HasValue)
         {
            dbList = store.ShoppingLists
               .Where(p => p.UserId == Identity.UserId)
               .SingleOrDefault(p => p.ShoppingListId == listId);

            if (dbList == null)
               throw new ShoppingListNotFoundException();

            dbItems = store.ShoppingListItems
               .Where(p => p.UserId == Identity.UserId)
               .Where(p => p.ShoppingListId.Equals(dbList.ShoppingListId))
               .ToList();

            if (!String.IsNullOrWhiteSpace(newName))
               dbList.Title = newName;

            shoppingListId = dbList.ShoppingListId;
         }
         else
         {
            dbItems = store.ShoppingListItems
               .Where(p => p.UserId == Identity.UserId)
               .Where(p => p.ShoppingListId == null)
               .ToList();
         }

         toModify.ForEach(item =>
         {
            var dbItem = store.ShoppingListItems.SingleOrDefault(p => p.ItemId == item.ModifiedItemId);
            if (dbItem == null) return;

            if (item.CrossOut.HasValue) dbItem.CrossedOut = item.CrossOut.Value;
            if (item.NewAmount != null)
            {
               dbItem.Qty = item.NewAmount.SizeHigh;
               dbItem.Unit = item.NewAmount.Unit;
            }
         });

         toAdd.ForEach(item =>
         {
            var source = item.GetItem();

            if (source.Ingredient == null && !String.IsNullOrWhiteSpace(source.Raw)) // Raw shopping list item
            {
               if (!dbItems.Any(i => source.Raw.Equals(i.Raw, StringComparison.OrdinalIgnoreCase))) // Add it
               {
                  store.ShoppingListItems.Add(new ShoppingListItems
                  {
                     ItemId = Guid.NewGuid(),
                     ShoppingListId = shoppingListId,
                     UserId = Identity.UserId,
                     Raw = source.Raw
                  });
               }

               return;
            }

            if (source.Ingredient != null && source.Amount == null) // Raw ingredient without any amount
            {
               var existingItem = dbItems.FirstOrDefault(i => i.IngredientId.HasValue && i.IngredientId.Value == source.Ingredient.Id);

               if (existingItem == null) // Add it
               {
                  store.ShoppingListItems.Add(new ShoppingListItems
                  {
                     ItemId = Guid.NewGuid(),
                     ShoppingListId = shoppingListId,
                     UserId = Identity.UserId,
                     IngredientId = source.Ingredient.Id
                  });
               }
               else // Clear out existing amount
               {
                  existingItem.Qty = null;
                  existingItem.Unit = null;
               }
            }

            if (source.Ingredient != null && source.Amount != null) // Ingredient with amount, aggregate if necessary
            {
               var existingItem = dbItems.FirstOrDefault(i => i.IngredientId.HasValue && i.IngredientId.Value == source.Ingredient.Id);

               if (existingItem == null) // Add it
               {
                  store.ShoppingListItems.Add(new ShoppingListItems
                  {
                     ItemId = Guid.NewGuid(),
                     ShoppingListId = shoppingListId,
                     UserId = Identity.UserId,
                     IngredientId = source.Ingredient.Id,
                     Qty = source.Amount != null ? (float?) source.Amount.SizeHigh : null,
                     Unit = source.Amount != null ? (Units?) source.Amount.Unit : null
                  });
               }
               else if (existingItem.Qty.HasValue) // Add to total
               {
                  existingItem.Qty += source.Amount.SizeHigh;
               }
            }
         });

         // Load full list to return
         var indexIngredients = store.GetIndexedIngredients();
         var indexRecipes = store.GetIndexedRecipes();

         var items = store.ShoppingListItems
            .Where(p => p.UserId == Identity.UserId)
            .Where(l => l.ShoppingListId == shoppingListId).Select(item =>
               new ShoppingListItem(item.ItemId)
               {
                  Raw = item.Raw,
                  Ingredient = item.IngredientId.HasValue ? Data.DTO.Ingredients.ToIngredient(indexIngredients[item.IngredientId.Value]) : null,
                  Recipe = item.RecipeId.HasValue ? Data.DTO.Recipes.ToRecipeBrief(indexRecipes[item.RecipeId.Value]) : null,
                  CrossedOut = item.CrossedOut,
                  Amount = (item.Qty.HasValue && item.Unit.HasValue) ? new Amount(item.Qty.Value, item.Unit.Value) : null
               });

         return new ShoppingListResult
         {
            List = new ShoppingList(shoppingListId, dbList != null ? dbList.Title : "", items)
         };
      }

      /// <summary>
      /// Aggregates one or more recipes into a set of aggregated ingredients.  This is normally used to create a list of items needed to buy for multiple recipes without having to create a shopping list.
      /// </summary>
      /// <param name="recipeIds">A list of recipe IDs to aggregate.</param>
      /// <returns>A list of IngredientAggregation objects, one per unique ingredient in the set of recipes</returns>
      public IList<IngredientAggregation> AggregateRecipes(params Guid[] recipeIds)
      {
         var ings = new Dictionary<Guid, IngredientAggregation>(); //List of all ingredients and total usage

         foreach (var id in recipeIds)
         {
            //Lookup ingredient through modeler cache
            var rNode = ModelerProxy.FindRecipe(id);
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

      /// <summary>
      /// Aggregates one or more IngredientUsage objects.
      /// </summary>
      /// <param name="usages">IngredientUsage objects, usually from one or more recipes</param>
      /// <returns>A list of IngredientAggregation objects, one per unique ingredient in the set of recipes</returns>
      public IList<IngredientAggregation> AggregateIngredients(params IngredientUsage[] usages)
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
      /// Deletes one or more shopping lists owned by the current user.
      /// </summary>
      /// <param name="lists">One or more shopping lists to delete.  Note, the default shopping list cannot be deleted.</param>
      public void DeleteShoppingLists(ShoppingList[] lists)
      {
         var ids = lists.Where(p => p.Id.HasValue).Select(p => p.Id.Value).ToList();

         var dbItems = store.ShoppingListItems
            .Where(p => p.UserId == Identity.UserId)
            .Where(p => p.ShoppingListId.HasValue)
            .Where(p => ids.Contains(p.ShoppingListId.Value));

         var dbLists = store.ShoppingLists
            .Where(p => p.UserId == Identity.UserId)
            .Where(p => ids.Contains(p.ShoppingListId));

         store.ShoppingListItems.RemoveAll(dbItems.Contains);
         store.ShoppingLists.RemoveAll(dbLists.Contains);
      }

      /// <summary>
      /// Returns the specified set of menus owned by the current user.
      /// </summary>
      /// <param name="menus">One or more Menu objects.  Use Menu.Favorites to load the default favorites menu.</param>
      /// <param name="options">Specifies what data to load.</param>
      /// <returns>An array of Menu objects with the specified data loaded.</returns>
      public Menu[] GetMenus(IList<Menu> menus, GetMenuOptions options)
      {
         var loadFav = true;
         var query = store.Menus.Where(p => p.UserId == Identity.UserId);

         if (menus != null) // Load individual menus
         {
            loadFav = menus.Contains(Menu.Favorites);
            var ids = menus.Where(m => m.Id.HasValue).Select(m => m.Id.Value);
            query = query.Where(p => ids.Contains(p.MenuId));
         }

         var dbMenus = query.ToList();
         var ret = new List<Menu>();

         if (loadFav)
            ret.Add(Menu.Favorites);

         ret.AddRange(dbMenus.Select(Data.DTO.Menus.ToMenu));

         if (!options.LoadRecipes) // We're done!
            return ret.ToArray();

         var indexRecipes = store.GetIndexedRecipes();
         var dbFavorites = store.Favorites.Where(p => p.UserId == Identity.UserId);

         return ret.Select(m =>
            new Menu(m)
            {
               Recipes = (m.Id.HasValue
                  ? dbFavorites.Where(f => f.MenuId.HasValue && f.MenuId == m.Id)
                  : dbFavorites.Where(f => f.MenuId == null)
                  ).Select(r => Data.DTO.Recipes.ToRecipeBrief(indexRecipes[r.RecipeId])).ToArray()
            }).ToArray();
      }

      /// <summary>
      /// Deletes one or more menus owned by the current user.
      /// </summary>
      /// <param name="menuIds">One or more menus to delete.  Note, the Favorites menu cannot be deleted.</param>
      public void DeleteMenus(params Guid[] menuIds)
      {
         var dbFavorites = store.Favorites
            .Where(p => p.UserId == Identity.UserId)
            .Where(p => p.MenuId.HasValue)
            .Where(p => menuIds.Contains(p.MenuId.Value));

         var dbMenus = store.Menus
            .Where(p => p.UserId == Identity.UserId)
            .Where(p => menuIds.Contains(p.MenuId));

         store.Favorites.RemoveAll(dbFavorites.Contains);
         store.Menus.RemoveAll(dbMenus.Contains);
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
      public MenuResult UpdateMenu(Guid? menuId, Guid[] recipesAdd, Guid[] recipesRemove, MenuMove[] recipesMove, bool clear, string newName = null)
      {
         var ret = new MenuResult();
         ret.MenuUpdated = true; // TODO: Verify actual changes were made before setting MenuUpdated to true

         Data.DTO.Menus dbMenu = null;
         if (menuId.HasValue)
         {
            dbMenu = store.Menus.SingleOrDefault(p => p.MenuId == menuId);
            if (dbMenu == null)
               throw new MenuNotFoundException();

            if (dbMenu.UserId != Identity.UserId) // User does not have access to modify this menu
               throw new UserDoesNotOwnMenuException();
         }

         var dbFavorites = store.Favorites
            .Where(p => p.MenuId == menuId)
            .ToList();

         if (!String.IsNullOrWhiteSpace(newName) && dbMenu != null) // Rename menu
            dbMenu.Title = newName.Trim();

         if (recipesAdd.Any()) // Add recipes to menu
         {
            var existing = dbFavorites.Select(f => f.RecipeId);
            recipesAdd = recipesAdd.Except(existing).ToArray(); //Remove dupes

            foreach (var rid in recipesAdd)
            {
               var fav = new Favorites
               {
                  FavoriteId = Guid.NewGuid(),
                  UserId = Identity.UserId,
                  RecipeId = rid,
                  MenuId = menuId
               };

               store.Favorites.Add(fav);
            }
         }

         if (recipesRemove.Any()) // Remove recipes from menu
         {
            var toDelete = (from r in dbFavorites where recipesRemove.Contains(r.RecipeId) select r);
            toDelete.ForEach(r => store.Favorites.Remove(r));
         }

         if (clear) // Remove every recipe from menu
         {
            store.Favorites.RemoveAll(dbFavorites.Contains);
         }

         if (recipesMove.Any()) // Move items to another menu
         {
            foreach (var moveAction in recipesMove)
            {
               Data.DTO.Menus dbTarget = null;
               if (moveAction.TargetMenu.HasValue)
               {
                  dbTarget = store.Menus
                     .Where(p => p.UserId == Identity.UserId)
                     .SingleOrDefault(p => p.MenuId == moveAction.TargetMenu);

                  if (dbTarget == null)
                     throw new MenuNotFoundException(moveAction.TargetMenu.Value);
               }

               var rToMove = (moveAction.MoveAll
                  ? dbFavorites
                  : dbFavorites.Where(r => moveAction.RecipesToMove.Contains(r.RecipeId)));

               rToMove.ForEach(a => a.MenuId = dbTarget != null ? (Guid?) dbTarget.MenuId : null);
            }
         }

         return ret;
      }

      /// <summary>
      /// Created a new menu owned by the current user.
      /// </summary>
      /// <param name="menu">A menu to create.</param>
      /// <param name="recipeIds">Zero or more recipes to add to the newly created menu.</param>
      /// <returns>A result indicating the new menu ID.</returns>
      public MenuResult CreateMenu(Menu menu, params Guid[] recipeIds)
      {
         menu.Title = menu.Title.Trim();
         var ret = new MenuResult();

         Data.DTO.Menus dbMenu;
         var dupes = store.Menus
            .Where(p => p.UserId == Identity.UserId)
            .Any(p => p.Title == menu.Title);

         if (dupes)
         {
            throw new MenuAlreadyExistsException();
         }

         store.Menus.Add(dbMenu = new Data.DTO.Menus
         {
            MenuId = Guid.NewGuid(),
            UserId = Identity.UserId,
            Title = menu.Title,
            CreatedDate = DateTime.Now,
         });

         foreach (var rid in recipeIds.NeverNull())
         {
            var fav = new Favorites
            {
               FavoriteId = Guid.NewGuid(),
               UserId = Identity.UserId,
               RecipeId = rid,
               MenuId = dbMenu.MenuId
            };

            store.Favorites.Add(fav);
         }

         ret.MenuCreated = true;
         ret.NewMenuId = dbMenu.MenuId;

         return ret;
      }

      /// <summary>Provides the ability to fluently work with menus.</summary>
      public MenuAction Menus
      {
         get
         {
            return new MenuAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with recipes.</summary>
      public RecipeAction Recipes
      {
         get
         {
            return new RecipeAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with shopping lists.</summary>
      public ShoppingListAction ShoppingLists
      {
         get
         {
            return new ShoppingListAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with the recipe queue.</summary>
      public QueueAction Queue
      {
         get
         {
            return new QueueAction(this);
         }
      }

      /// <summary>Provides the ability to fluently work with the recipe modeler.</summary>
      public ModelerAction Modeler
      {
         get
         {
            return new ModelerAction(this);
         }
      }

      protected virtual void LoadTemplates()
      {
         Parser.LoadTemplates(
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

      /// <summary>
      /// Imports data from another source into this context.
      /// </summary>
      /// <param name="source">Another KitchenPC context that provides the ability to export data.</param>
      public void Import(IProvisionSource source)
      {
         KPCContext.Log.DebugFormat("Importing data from {0} into StaticContext.", source.GetType().Name);
         InitializeStore();

         // Call source.Export and populate local data store
         var data = source.Export();
         var serializer = new XmlSerializer(data.GetType());

         var file = CompressedStore ? "KPCData.gz" : "KPCData.xml";
         var path = Path.Combine(DataDirectory, file);

         KPCContext.Log.DebugFormat("Serializing data to local file: {0}", path);
         using (var fileWriter = new FileStream(path, FileMode.Create))
         {
            if (CompressedStore)
            {
               using (var writer = new GZipStream(fileWriter, CompressionLevel.Optimal))
               {
                  serializer.Serialize(writer, data);
               }
            }
            else
            {
               serializer.Serialize(fileWriter, data);
            }
         }

         KPCContext.Log.DebugFormat("Done importing data from into StaticContext.");
      }

      /// <summary>
      /// Creates the configured data directory, if it does not already exist.
      /// </summary>
      public void InitializeStore()
      {
         if (String.IsNullOrWhiteSpace(DataDirectory))
            throw new InvalidConfigurationException("StaticContext requires a configured data directory.");

         if (!Directory.Exists(DataDirectory)) // Create directory
         {
            KPCContext.Log.DebugFormat("Creating directory for local data store at: {0}", DataDirectory);
            Directory.CreateDirectory(DataDirectory);
         }
      }

      /// <summary>
      /// Exports data from this context.  This is usually called automatically by the Import method of another context.
      /// </summary>
      /// <returns>A DataStore containing all data available to this context.</returns>
      public DataStore Export()
      {
         return store;
      }
   }
}