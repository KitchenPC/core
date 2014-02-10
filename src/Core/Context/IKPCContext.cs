using System;
using System.Collections.Generic;
using KitchenPC.Context.Fluent;
using KitchenPC.Ingredients;
using KitchenPC.Menus;
using KitchenPC.Modeler;
using KitchenPC.NLP;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;
using IngredientUsage = KitchenPC.Ingredients.IngredientUsage;

namespace KitchenPC.Context
{
   /// <summary>Implements a KitchenPC Context which is used to interact with the KitchenPC engine, as well as persist data.</summary>
   public interface IKPCContext
   {
      void Initialize();
      AuthIdentity Identity { get; }

      // Autocomplete support
      IEnumerable<IngredientNode> AutocompleteIngredient(string query);

      // Modeler support
      IModelerLoader ModelerLoader { get; }
      ModelingSession CreateModelingSession(IUserProfile profile);
      Parser Parser { get; }
      ModelerProxy ModelerProxy { get; }

      // NLP Support
      Result ParseIngredientUsage(string input);
      Ingredient ParseIngredient(string input);

      // Recipe support
      SearchResults RecipeSearch(RecipeQuery query);
      Recipe[] ReadRecipes(Guid[] recipeIds, ReadRecipeOptions options);
      void RateRecipe(Guid recipeId, Rating rating);
      RecipeResult CreateRecipe(Recipe recipe);

      // Queue support
      void DequeueRecipe(params Guid[] recipeIds);
      void EnqueueRecipes(params Guid[] recipeIds);
      RecipeBrief[] GetRecipeQueue();

      // Ingredient support
      IngredientFormsCollection ReadFormsForIngredient(Guid id);
      Ingredient ReadIngredient(String ingredient);
      Ingredient ReadIngredient(Guid ingid);
      IngredientAggregation ConvertIngredientUsage(IngredientUsage usage);

      // Shopping list support
      ShoppingList[] GetShoppingLists(IList<ShoppingList> lists, GetShoppingListOptions options);
      ShoppingListResult CreateShoppingList(string name, Recipe[] recipes, Ingredient[] ingredients, IngredientUsage[] usages, String[] items);
      ShoppingListResult CreateShoppingList(ShoppingList list);
      ShoppingListResult UpdateShoppingList(ShoppingList list, ShoppingListUpdateCommand[] updates, string newName = null);
      IList<IngredientAggregation> AggregateRecipes(params Guid[] recipeIds);
      IList<IngredientAggregation> AggregateIngredients(params IngredientUsage[] usages);
      void DeleteShoppingLists(ShoppingList[] lists);

      // Menu support
      Menu[] GetMenus(IList<Menu> menus, GetMenuOptions options);
      void DeleteMenus(params Guid[] menuIds);
      MenuResult UpdateMenu(Guid? menuId, Guid[] recipesAdd, Guid[] recipesRemove, MenuMove[] recipesMove, bool clear, string newName = null);
      MenuResult CreateMenu(Menu menu, params Guid[] recipeIds);

      // Fluent Interfaces (Will eventually replace non-fluent API)
      MenuAction Menus { get; }
      RecipeAction Recipes { get; }
      ShoppingListAction ShoppingLists { get; }
      QueueAction Queue { get; }
      ModelerAction Modeler { get; }
   }
}