using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Fluent;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.Menus;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.NLP;
using KitchenPC.Core.Recipes;
using KitchenPC.Core.ShoppingLists;
using IngredientNode = KitchenPC.Core.Context.IngredientNode;
using IngredientUsage = KitchenPC.Core.Ingredients.IngredientUsage;

namespace KitchenPC.UnitTests.Mock;

internal class MockContext : IKPCContext
{
   public void Initialize()
   {
      ModelerProxy = new ModelerProxy(this);
      ModelerProxy.LoadSnapshot();
   }

   public AuthIdentity Identity => throw new NotImplementedException();

   public IKPCContext AsUserContext(AuthIdentity userContext) =>
      throw new NotImplementedException();

   public IEnumerable<IngredientNode> AutocompleteIngredient(string query) =>
      throw new NotImplementedException();

   public IModelerLoader ModelerLoader => new MockModelerDBLoader("ModelerData.xml");

   public ModelingSession CreateModelingSession(IUserProfile profile) =>
      ModelerProxy.CreateSession(profile);

   public Parser Parser => throw new NotImplementedException();

   public ModelerProxy ModelerProxy { get; private set; }

   public Result ParseIngredientUsage(string input) => throw new NotImplementedException();

   public Ingredient ParseIngredient(string input) => throw new NotImplementedException();

   public SearchResults RecipeSearch(RecipeQuery query) => throw new NotImplementedException();

   public Recipe[] ReadRecipes(Guid[] recipeIds, ReadRecipeOptions options) =>
      throw new NotImplementedException();

   public void RateRecipe(Guid recipeId, Rating rating)
   {
      throw new NotImplementedException();
   }

   public RecipeResult CreateRecipe(Recipe recipe) => throw new NotImplementedException();

   public void DequeueRecipe(params Guid[] recipeIds)
   {
      throw new NotImplementedException();
   }

   public void EnqueueRecipes(params Guid[] recipeIds)
   {
      throw new NotImplementedException();
   }

   public RecipeBrief[] GetRecipeQueue() => throw new NotImplementedException();

   public IngredientFormsCollection ReadFormsForIngredient(Guid id) =>
      throw new NotImplementedException();

   public Ingredient ReadIngredient(string ingredient) => throw new NotImplementedException();

   public Ingredient ReadIngredient(Guid ingid) => throw new NotImplementedException();

   public IngredientAggregation ConvertIngredientUsage(IngredientUsage usage) =>
      throw new NotImplementedException();

   public ShoppingList[] GetShoppingLists(
      IList<ShoppingList> lists,
      GetShoppingListOptions options
   ) => throw new NotImplementedException();

   public ShoppingListResult CreateShoppingList(
      string name,
      Recipe[] recipes,
      Ingredient[] ingredients,
      IngredientUsage[] usages,
      string[] items
   ) => throw new NotImplementedException();

   public ShoppingListResult CreateShoppingList(ShoppingList list) =>
      throw new NotImplementedException();

   public ShoppingListResult UpdateShoppingList(
      ShoppingList list,
      ShoppingListUpdateCommand[] updates,
      string newName = null
   ) => throw new NotImplementedException();

   public IList<IngredientAggregation> AggregateRecipes(params Guid[] recipeIds) =>
      throw new NotImplementedException();

   public IList<IngredientAggregation> AggregateIngredients(params IngredientUsage[] usages) =>
      throw new NotImplementedException();

   public void DeleteShoppingLists(ShoppingList[] lists)
   {
      throw new NotImplementedException();
   }

   public Menu[] GetMenus(IList<Menu> menus, GetMenuOptions options) =>
      throw new NotImplementedException();

   public void DeleteMenus(params Guid[] menuIds)
   {
      throw new NotImplementedException();
   }

   public MenuResult UpdateMenu(
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null
   ) => throw new NotImplementedException();

   public MenuResult CreateMenu(Menu menu, params Guid[] recipeIds) =>
      throw new NotImplementedException();

   public MenuAction Menus => throw new NotImplementedException();

   public RecipeAction Recipes => throw new NotImplementedException();

   public ShoppingListAction ShoppingLists => throw new NotImplementedException();

   public QueueAction Queue => throw new NotImplementedException();

   public ModelerAction Modeler => throw new NotImplementedException();

   public Task<SearchResults> RecipeSearchAsync(
      RecipeQuery query,
      CancellationToken cancellationToken = default
   ) => Task.FromException<SearchResults>(new NotImplementedException());

   public Task<Recipe[]> ReadRecipesAsync(
      Guid[] recipeIds,
      ReadRecipeOptions options,
      CancellationToken cancellationToken = default
   ) => Task.FromException<Recipe[]>(new NotImplementedException());

   public Task RateRecipeAsync(
      Guid recipeId,
      Rating rating,
      CancellationToken cancellationToken = default
   ) => Task.FromException(new NotImplementedException());

   public Task DequeueRecipeAsync(
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   ) => Task.FromException(new NotImplementedException());

   public Task EnqueueRecipesAsync(
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   ) => Task.FromException(new NotImplementedException());

   public Task<RecipeBrief[]> GetRecipeQueueAsync(CancellationToken cancellationToken = default) =>
      Task.FromException<RecipeBrief[]>(new NotImplementedException());

   public Task<Ingredient> ReadIngredientAsync(
      string ingredient,
      CancellationToken cancellationToken = default
   ) => Task.FromException<Ingredient>(new NotImplementedException());

   public Task<Ingredient> ReadIngredientAsync(
      Guid ingredientId,
      CancellationToken cancellationToken = default
   ) => Task.FromException<Ingredient>(new NotImplementedException());

   public Task<Menu[]> GetMenusAsync(
      IList<Menu> menus,
      GetMenuOptions options,
      CancellationToken cancellationToken = default
   ) => Task.FromException<Menu[]>(new NotImplementedException());

   public Task DeleteMenusAsync(Guid[] menuIds, CancellationToken cancellationToken = default) =>
      Task.FromException(new NotImplementedException());

   public Task<MenuResult> UpdateMenuAsync(
      Guid? menuId,
      Guid[] recipesAdd,
      Guid[] recipesRemove,
      MenuMove[] recipesMove,
      bool clear,
      string newName = null,
      CancellationToken cancellationToken = default
   ) => Task.FromException<MenuResult>(new NotImplementedException());

   public Task<MenuResult> CreateMenuAsync(
      Menu menu,
      Guid[] recipeIds,
      CancellationToken cancellationToken = default
   ) => Task.FromException<MenuResult>(new NotImplementedException());
}
