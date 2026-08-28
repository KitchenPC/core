using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KitchenPC.Core.Fluent;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Menus;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.NLP;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;
using Microsoft.Extensions.Logging;
using IngredientUsage = KitchenPC.Core.Ingredients.IngredientUsage;

namespace KitchenPC.Core.Context;

/// <summary>Implements a KitchenPC Context which is used to interact with the KitchenPC engine, as well as persist data.</summary>
public interface IKPCContext
{
   void Initialize();
   AuthIdentity Identity { get; }
   ILoggerFactory LoggerFactory { get; }

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
   Task<SearchResults> RecipeSearchAsync(
      RecipeQuery query,
      CancellationToken cancellationToken = default
   );
   Recipe[] ReadRecipes(Guid[] recipeIds, ReadRecipeOptions options);
   Task<Recipe[]> ReadRecipesAsync(
      Guid[] recipeIds,
      ReadRecipeOptions options,
      CancellationToken cancellationToken = default
   );
   void RateRecipe(Guid recipeId, Rating rating);
   Task RateRecipeAsync(
      Guid recipeId,
      Rating rating,
      CancellationToken cancellationToken = default
   );
   RecipeResult CreateRecipe(Recipe recipe);

   // Queue support
   void DequeueRecipe(params Guid[] recipeIds);
   Task DequeueRecipeAsync(Guid[] recipeIds, CancellationToken cancellationToken = default);
   void EnqueueRecipes(params Guid[] recipeIds);
   Task EnqueueRecipesAsync(Guid[] recipeIds, CancellationToken cancellationToken = default);
   RecipeBrief[] GetRecipeQueue();
   Task<RecipeBrief[]> GetRecipeQueueAsync(CancellationToken cancellationToken = default);

   // Ingredient support
   IngredientFormsCollection ReadFormsForIngredient(Guid id);
   Ingredient ReadIngredient(String ingredient);
   Ingredient ReadIngredient(Guid ingid);
   Task<Ingredient> ReadIngredientAsync(
      string ingredient,
      CancellationToken cancellationToken = default
   );
   Task<Ingredient> ReadIngredientAsync(
      Guid ingredientId,
      CancellationToken cancellationToken = default
   );
   IngredientAggregation ConvertIngredientUsage(IngredientUsage usage);

   // Shopping list support
   ShoppingList[] GetShoppingLists(IList<ShoppingList> lists, GetShoppingListOptions options);
   ShoppingListResult CreateShoppingList(
      string name,
      Recipe[] recipes,
      Ingredient[] ingredients,
      IngredientUsage[] usages,
      String[] items
   );
   ShoppingListResult CreateShoppingList(ShoppingList list);
   ShoppingListResult UpdateShoppingList(
      ShoppingList list,
      ShoppingListUpdateCommand[] updates,
      string newName = null
   );
   IList<IngredientAggregation> AggregateRecipes(params Guid[] recipeIds);
   IList<IngredientAggregation> AggregateIngredients(params IngredientUsage[] usages);
   void DeleteShoppingLists(ShoppingList[] lists);

   // Menu support
   Menu[] GetMenus(IList<Menu> menus, GetMenuOptions options);
   Task<Menu[]> GetMenusAsync(
      IList<Menu> menus,
      GetMenuOptions options,
      CancellationToken cancellationToken = default
   );
   void DeleteMenus(params Guid[] menuIds);
   Task DeleteMenusAsync(Guid[] menuIds, CancellationToken cancellationToken = default);
   MenuResult UpdateMenu(
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null
   );
   Task<MenuResult> UpdateMenuAsync(
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null,
      CancellationToken cancellationToken = default
   );
   MenuResult CreateMenu(Menu menu, params Guid[] recipeIds);
   Task<MenuResult> CreateMenuAsync(
      Menu menu,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   );

   // Fluent Interfaces (Will eventually replace non-fluent API)
   MenuAction Menus { get; }
   RecipeAction Recipes { get; }
   ShoppingListAction ShoppingLists { get; }
   QueueAction Queue { get; }
   ModelerAction Modeler { get; }

   // Authentication Wrappers (Used for DI)
   IKPCContext AsUserContext(AuthIdentity userContext);
}
