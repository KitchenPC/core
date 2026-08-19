using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Menus;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.NLP;
using KitchenPC.Core.Provisioning;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;

namespace KitchenPC.Core.Context;

public interface IDBAdapter : IProvisionSource, IProvisionTarget
{
   void Initialize(IKPCContext context);
   IEnumerable<IngredientSource> LoadIngredientsForIndex();

   //Modeler support
   IEnumerable<RecipeBinding> LoadRecipeGraph();
   IEnumerable<IngredientBinding> LoadIngredientGraph();
   IEnumerable<RatingBinding> LoadRatingGraph();

   //NLP Support
   ISynonymLoader<NLP.IngredientNode> IngredientLoader { get; }
   ISynonymLoader<UnitNode> UnitLoader { get; }
   ISynonymLoader<FormNode> FormLoader { get; }
   ISynonymLoader<PrepNode> PrepLoader { get; }
   ISynonymLoader<AnomalousNode> AnomalyLoader { get; }

   //Recipes and Ingredients
   Recipe[] ReadRecipes(AuthIdentity identity, Guid[] recipeIds, ReadRecipeOptions options);
   Task<Recipe[]> ReadRecipesAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      ReadRecipeOptions options,
      CancellationToken cancellationToken = default
   );
   SearchResults RecipeSearch(AuthIdentity identity, RecipeQuery query);
   Task<SearchResults> RecipeSearchAsync(
      AuthIdentity identity,
      RecipeQuery query,
      CancellationToken cancellationToken = default
   );
   IngredientFormsCollection ReadFormsForIngredient(Guid ingredientId);
   Ingredient ReadIngredient(string ingredient);
   Ingredient ReadIngredient(Guid ingid);
   Task<Ingredient> ReadIngredientAsync(
      string ingredient,
      CancellationToken cancellationToken = default
   );
   Task<Ingredient> ReadIngredientAsync(
      Guid ingredientId,
      CancellationToken cancellationToken = default
   );
   void RateRecipe(AuthIdentity identity, Guid recipeId, Rating rating);
   Task RateRecipeAsync(
      AuthIdentity identity,
      Guid recipeId,
      Rating rating,
      CancellationToken cancellationToken = default
   );
   RecipeResult CreateRecipe(AuthIdentity identity, Recipe recipe);

   //Queue
   void DequeueRecipe(AuthIdentity identity, params Guid[] recipeIds);
   Task DequeueRecipeAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   );
   void EnqueueRecipes(AuthIdentity identity, params Guid[] recipeIds);
   Task EnqueueRecipesAsync(
      AuthIdentity identity,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   );
   RecipeBrief[] GetRecipeQueue(AuthIdentity identity);
   Task<RecipeBrief[]> GetRecipeQueueAsync(
      AuthIdentity identity,
      CancellationToken cancellationToken = default
   );

   // Shopping list support
   ShoppingList[] GetShoppingLists(
      AuthIdentity identity,
      IList<ShoppingList> lists,
      GetShoppingListOptions options
   );
   ShoppingListResult CreateShoppingList(AuthIdentity identity, ShoppingList list);
   ShoppingListResult UpdateShoppingList(
      AuthIdentity identity,
      Guid? listId,
      Guid[] toRemove,
      ShoppingListModification[] toModify,
      IShoppingListSource[] toAdd,
      string newName = null
   );
   void DeleteShoppingLists(AuthIdentity identity, ShoppingList[] lists);

   // Menu support
   Menu[] GetMenus(AuthIdentity identity, IList<Menu> menus, GetMenuOptions options);
   Task<Menu[]> GetMenusAsync(
      AuthIdentity identity,
      IList<Menu> menus,
      GetMenuOptions options,
      CancellationToken cancellationToken = default
   );
   MenuResult CreateMenu(AuthIdentity identity, Menu menu, params Guid[] recipeIds);
   Task<MenuResult> CreateMenuAsync(
      AuthIdentity identity,
      Menu menu,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   );
   void DeleteMenus(AuthIdentity identity, params Guid[] menuIds);
   Task DeleteMenusAsync(
      AuthIdentity identity,
      Guid[] menuIds,
      CancellationToken cancellationToken = default
   );
   MenuResult UpdateMenu(
      AuthIdentity identity,
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null
   );
   Task<MenuResult> UpdateMenuAsync(
      AuthIdentity identity,
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null,
      CancellationToken cancellationToken = default
   );
}
