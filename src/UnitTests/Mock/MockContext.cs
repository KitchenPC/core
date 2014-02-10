using System;
using System.Collections.Generic;
using KitchenPC;
using KitchenPC.Context;
using KitchenPC.Context.Fluent;
using KitchenPC.Ingredients;
using KitchenPC.Menus;
using KitchenPC.Modeler;
using KitchenPC.NLP;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
using IngredientNode = KitchenPC.Context.IngredientNode;
using IngredientUsage = KitchenPC.Ingredients.IngredientUsage;

namespace KPCServer.UnitTests
{
   internal class MockContext : IKPCContext
   {
      public void Initialize()
      {
         ModelerProxy = new ModelerProxy(this);
         ModelerProxy.LoadSnapshot();
      }

      public AuthIdentity Identity
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public IEnumerable<IngredientNode> AutocompleteIngredient(string query)
      {
         throw new NotImplementedException();
      }

      public IModelerLoader ModelerLoader
      {
         get
         {
            return new MockModelerDBLoader("ModelerData.xml");
         }
      }

      public ModelingSession CreateModelingSession(IUserProfile profile)
      {
         return ModelerProxy.CreateSession(profile);
      }

      public Parser Parser
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public ModelerProxy ModelerProxy { get; private set; }

      public Result ParseIngredientUsage(string input)
      {
         throw new NotImplementedException();
      }

      public Ingredient ParseIngredient(string input)
      {
         throw new NotImplementedException();
      }

      public SearchResults RecipeSearch(RecipeQuery query)
      {
         throw new NotImplementedException();
      }

      public Recipe[] ReadRecipes(Guid[] recipeIds, ReadRecipeOptions options)
      {
         throw new NotImplementedException();
      }

      public void RateRecipe(Guid recipeId, Rating rating)
      {
         throw new NotImplementedException();
      }

      public RecipeResult CreateRecipe(Recipe recipe)
      {
         throw new NotImplementedException();
      }

      public void DequeueRecipe(params Guid[] recipeIds)
      {
         throw new NotImplementedException();
      }

      public void EnqueueRecipes(params Guid[] recipeIds)
      {
         throw new NotImplementedException();
      }

      public RecipeBrief[] GetRecipeQueue()
      {
         throw new NotImplementedException();
      }

      public IngredientFormsCollection ReadFormsForIngredient(Guid id)
      {
         throw new NotImplementedException();
      }

      public Ingredient ReadIngredient(string ingredient)
      {
         throw new NotImplementedException();
      }

      public Ingredient ReadIngredient(Guid ingid)
      {
         throw new NotImplementedException();
      }

      public IngredientAggregation ConvertIngredientUsage(IngredientUsage usage)
      {
         throw new NotImplementedException();
      }

      public ShoppingList[] GetShoppingLists(IList<ShoppingList> lists, GetShoppingListOptions options)
      {
         throw new NotImplementedException();
      }

      public ShoppingListResult CreateShoppingList(string name, Recipe[] recipes, Ingredient[] ingredients, IngredientUsage[] usages, string[] items)
      {
         throw new NotImplementedException();
      }

      public ShoppingListResult CreateShoppingList(ShoppingList list)
      {
         throw new NotImplementedException();
      }

      public ShoppingListResult UpdateShoppingList(ShoppingList list, ShoppingListUpdateCommand[] updates, string newName = null)
      {
         throw new NotImplementedException();
      }

      public IList<IngredientAggregation> AggregateRecipes(params Guid[] recipeIds)
      {
         throw new NotImplementedException();
      }

      public IList<IngredientAggregation> AggregateIngredients(params IngredientUsage[] usages)
      {
         throw new NotImplementedException();
      }

      public void DeleteShoppingLists(ShoppingList[] lists)
      {
         throw new NotImplementedException();
      }

      public Menu[] GetMenus(IList<Menu> menus, GetMenuOptions options)
      {
         throw new NotImplementedException();
      }

      public void DeleteMenus(params Guid[] menuIds)
      {
         throw new NotImplementedException();
      }

      public MenuResult UpdateMenu(Guid? menuId, Guid[] recipesAdd, Guid[] recipesRemove, MenuMove[] recipesMove, bool clear, string newName = null)
      {
         throw new NotImplementedException();
      }

      public MenuResult CreateMenu(Menu menu, params Guid[] recipeIds)
      {
         throw new NotImplementedException();
      }

      public MenuAction Menus
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public RecipeAction Recipes
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public ShoppingListAction ShoppingLists
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public QueueAction Queue
      {
         get
         {
            throw new NotImplementedException();
         }
      }

      public ModelerAction Modeler
      {
         get
         {
            throw new NotImplementedException();
         }
      }
   }
}