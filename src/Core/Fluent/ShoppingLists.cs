using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Ingredients;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;

namespace KitchenPC.Context.Fluent
{
   /// <summary>Provides the ability to fluently express shopping list related actions, such as loading, creating or updating lists.</summary>
   public class ShoppingListAction
   {
      readonly IKPCContext context;

      public ShoppingListAction(IKPCContext context)
      {
         this.context = context;
      }

      public ShoppingListLoader Load(ShoppingList list)
      {
         return new ShoppingListLoader(context, list);
      }

      public ShoppingListLoader LoadAll
      {
         get
         {
            return new ShoppingListLoader(context);
         }
      }

      public ShoppingListDeleter Delete(ShoppingList list)
      {
         if (list == ShoppingList.Default)
            throw new FluentExpressionException("Cannot delete default shopping list.");

         return new ShoppingListDeleter(context, list);
      }

      public ShoppingListCreator Create
      {
         get
         {
            return new ShoppingListCreator(context);
         }
      }

      public ShoppingListUpdater Update(ShoppingList list)
      {
         return new ShoppingListUpdater(context, list);
      }
   }

   public class ShoppingListLoader
   {
      readonly IKPCContext context;
      readonly IList<ShoppingList> listsToLoad;
      readonly bool loadAll;
      bool loadItems;

      public ShoppingListLoader WithItems
      {
         get
         {
            loadItems = true;
            return this;
         }
      }

      public ShoppingListLoader(IKPCContext context)
      {
         this.context = context;
         this.loadAll = true;
      }

      public ShoppingListLoader(IKPCContext context, ShoppingList list)
      {
         this.context = context;
         listsToLoad = new List<ShoppingList>() {list};
      }

      public ShoppingListLoader Load(ShoppingList list)
      {
         if (loadAll)
            throw new FluentExpressionException("To specify individual shopping lists to load, remove the LoadAll clause from your expression.");

         listsToLoad.Add(list);
         return this;
      }

      public IList<ShoppingList> List()
      {
         var options = new GetShoppingListOptions
         {
            LoadItems = loadItems
         };

         return context.GetShoppingLists(listsToLoad, options);
      }
   }

   public class ShoppingListDeleter
   {
      readonly IKPCContext context;
      readonly IList<ShoppingList> deleteQueue;

      public ShoppingListDeleter(IKPCContext context, ShoppingList list)
      {
         this.context = context;
         deleteQueue = new List<ShoppingList>() {list};
      }

      public ShoppingListDeleter Delete(ShoppingList list)
      {
         if (list == ShoppingList.Default)
            throw new FluentExpressionException("Cannot delete default shopping list.");

         deleteQueue.Add(list);
         return this;
      }

      public void Commit()
      {
         context.DeleteShoppingLists(deleteQueue.ToArray());
      }
   }

   public class ShoppingListCreator
   {
      readonly IKPCContext context;
      string listName;
      readonly IList<ShoppingListAdder> addQueue;

      public ShoppingListCreator(IKPCContext context)
      {
         this.context = context;
         addQueue = new List<ShoppingListAdder>();
         listName = "New Shopping List";
      }

      public ShoppingListCreator WithName(string name)
      {
         this.listName = name;
         return this;
      }

      public ShoppingListCreator AddItems(Func<ShoppingListAddAction, ShoppingListAddAction> addAction)
      {
         var action = ShoppingListAdder.Create();
         var result = addAction(action);

         addQueue.Add(result.Adder);
         return this;
      }

      public ShoppingListCreator AddItems(ShoppingListAdder adder)
      {
         addQueue.Add(adder);
         return this;
      }

      public ShoppingListResult Commit()
      {
         var recipes = addQueue.SelectMany(r => r.Recipes).ToArray();
         var ingredients = addQueue.SelectMany(i => i.Ingredients).ToArray();
         var usages = addQueue.SelectMany(u => u.Usages).ToArray();
         var raw = addQueue.SelectMany(r => r.ToParse).ToArray();

         return context.CreateShoppingList(listName, recipes, ingredients, usages, raw);
      }
   }

   public class ShoppingListUpdater
   {
      readonly IKPCContext context;
      readonly ShoppingList list;
      readonly IList<ShoppingListAdder> addQueue;
      readonly IList<ShoppingListItemUpdater> updateQueue;
      readonly IList<ShoppingListItem> removeQueue;
      string newName;

      public ShoppingListUpdater(IKPCContext context, ShoppingList list)
      {
         this.context = context;
         this.list = list;

         addQueue = new List<ShoppingListAdder>();
         updateQueue = new List<ShoppingListItemUpdater>();
         removeQueue = new List<ShoppingListItem>();
      }

      public ShoppingListUpdater Rename(string newname)
      {
         if (list == ShoppingList.Default)
            throw new FluentExpressionException("Cannot rename default shopping list.");

         newName = newname;
         return this;
      }

      public ShoppingListUpdater AddItems(Func<ShoppingListAddAction, ShoppingListAddAction> addAction)
      {
         var action = ShoppingListAdder.Create();
         var result = addAction(action);

         addQueue.Add(result.Adder);
         return this;
      }

      public ShoppingListUpdater AddItems(ShoppingListAdder adder)
      {
         addQueue.Add(adder);
         return this;
      }

      public ShoppingListUpdater UpdateItem(ShoppingListItem item, Func<ShoppingListItemUpdateAction, ShoppingListItemUpdateAction> updateAction)
      {
         var action = ShoppingListItemUpdater.Create(item);
         var result = updateAction(action);

         updateQueue.Add(result.Updater);
         return this;
      }

      public ShoppingListUpdater UpdateItem(ShoppingListItem item, ShoppingListItemUpdater updater)
      {
         updateQueue.Add(updater);
         return this;
      }

      public ShoppingListUpdater RemoveItem(ShoppingListItem item)
      {
         removeQueue.Add(item);
         return this;
      }

      public ShoppingListResult Commit()
      {
         // Build ShoppingListUpdateCommand array with each update
         var updates = new List<ShoppingListUpdateCommand>();

         if (addQueue.Any())
         {
            // Grab new raw entries
            var newRaws = addQueue.SelectMany(i => i.ToParse).Where(i => !String.IsNullOrWhiteSpace(i))
               .Select(i => new ShoppingListUpdateCommand
               {
                  Command = ShoppingListUpdateType.AddItem,
                  NewRaw = i
               });

            // Grab new Recipes
            var newRecipes = addQueue.SelectMany(i => i.Recipes).Where(i => i != null)
               .Select(i => new ShoppingListUpdateCommand
               {
                  Command = ShoppingListUpdateType.AddItem,
                  NewRecipe = i
               });

            // Grab new IngredientUsages
            var newUsages = addQueue.SelectMany(i => i.Usages).Where(i => i != null)
               .Select(i => new ShoppingListUpdateCommand
               {
                  Command = ShoppingListUpdateType.AddItem,
                  NewUsage = i
               });

            // Grab new Ingredients
            var newIngredients = addQueue.SelectMany(i => i.Ingredients).Where(i => i != null)
               .Select(i => new ShoppingListUpdateCommand
               {
                  Command = ShoppingListUpdateType.AddItem,
                  NewIngredient = i
               });

            updates.AddRange(newRaws);
            updates.AddRange(newRecipes);
            updates.AddRange(newUsages);
            updates.AddRange(newIngredients);
         }

         if (removeQueue.Any())
         {
            updates.AddRange(removeQueue.Where(i => i.Id.HasValue).Select(i => new ShoppingListUpdateCommand
            {
               Command = ShoppingListUpdateType.RemoveItem,
               RemoveItem = i.Id
            }));
         }

         if (updateQueue.Any())
         {
            updates.AddRange(updateQueue.Where(i => i.Item.Id.HasValue).Select(i => new ShoppingListUpdateCommand
            {
               Command = ShoppingListUpdateType.ModifyItem,
               ModifyItem = new ShoppingListModification(i.Item.Id.Value, i.NewAmount, i.CrossedOut)
            }));
         }

         return context.UpdateShoppingList(list, updates.ToArray(), newName);
      }
   }

   /// <summary>Represents one or more items to be added to a shopping list.</summary>
   public class ShoppingListAdder
   {
      public IList<Recipe> Recipes { get; set; } // Recipes to add
      public IList<Ingredient> Ingredients { get; set; } // Resolved ingredients to add
      public IList<IngredientUsage> Usages { get; set; } // Ingredient Usages which will be aggregated upon save
      public IList<String> ToParse { get; set; } // Raw ingredient strings that will be parsed, or added in raw form

      public ShoppingListAdder()
      {
         Recipes = new List<Recipe>();
         Ingredients = new List<Ingredient>();
         Usages = new List<IngredientUsage>();
         ToParse = new List<String>();
      }

      public static ShoppingListAddAction Create()
      {
         var adder = new ShoppingListAdder();
         return new ShoppingListAddAction(adder);
      }
   }

   /// <summary>Provides the ability to fluently express a ShoppingListAdder object.</summary>
   public class ShoppingListAddAction
   {
      readonly ShoppingListAdder adder;

      public ShoppingListAddAction(ShoppingListAdder adder)
      {
         this.adder = adder;
      }

      public ShoppingListAddAction AddRecipe(Recipe recipe)
      {
         adder.Recipes.Add(recipe);
         return this;
      }

      public ShoppingListAddAction AddIngredient(Ingredient ingredient)
      {
         adder.Ingredients.Add(ingredient);
         return this;
      }

      public ShoppingListAddAction AddUsage(IngredientUsage usage)
      {
         adder.Usages.Add(usage);
         return this;
      }

      public ShoppingListAddAction AddItem(string raw)
      {
         adder.ToParse.Add(raw);
         return this;
      }

      public ShoppingListAdder Adder
      {
         get
         {
            return adder;
         }
      }
   }

   /// <summary>Represents a set of changes to a single shopping list item.</summary>
   public class ShoppingListItemUpdater
   {
      public ShoppingListItem Item;
      public Boolean? CrossedOut;
      public Amount NewAmount;

      public ShoppingListItemUpdater(ShoppingListItem item)
      {
         this.Item = item;
      }

      public static ShoppingListItemUpdateAction Create(ShoppingListItem item)
      {
         var updater = new ShoppingListItemUpdater(item);
         return new ShoppingListItemUpdateAction(updater);
      }
   }

   /// <summary>Provides the ability to fluently express a ShoppingListItemUpdater object.</summary>
   public class ShoppingListItemUpdateAction
   {
      readonly ShoppingListItemUpdater updater;

      public ShoppingListItemUpdateAction(ShoppingListItemUpdater updater)
      {
         this.updater = updater;
      }

      public ShoppingListItemUpdateAction CrossOut
      {
         get
         {
            updater.CrossedOut = true;
            return this;
         }
      }

      public ShoppingListItemUpdateAction UnCrossOut
      {
         get
         {
            updater.CrossedOut = false;
            return this;
         }
      }

      public ShoppingListItemUpdateAction NewAmount(Amount newAmount)
      {
         updater.NewAmount = newAmount;
         return this;
      }

      public ShoppingListItemUpdater Updater
      {
         get
         {
            return updater;
         }
      }
   }
}