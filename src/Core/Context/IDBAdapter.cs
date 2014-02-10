using System;
using System.Collections.Generic;
using KitchenPC.Data;
using KitchenPC.Ingredients;
using KitchenPC.Menus;
using KitchenPC.Modeler;
using KitchenPC.NLP;
using KitchenPC.Provisioning;
using KitchenPC.Recipes;
using KitchenPC.ShoppingLists;

namespace KitchenPC.Context
{
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
      SearchResults RecipeSearch(AuthIdentity identity, RecipeQuery query);
      IngredientFormsCollection ReadFormsForIngredient(Guid ingredientId);
      Ingredient ReadIngredient(string ingredient);
      Ingredient ReadIngredient(Guid ingid);
      void RateRecipe(AuthIdentity identity, Guid recipeId, Rating rating);
      RecipeResult CreateRecipe(AuthIdentity identity, Recipe recipe);

      //Queue
      void DequeueRecipe(AuthIdentity identity, params Guid[] recipeIds);
      void EnqueueRecipes(AuthIdentity identity, params Guid[] recipeIds);
      RecipeBrief[] GetRecipeQueue(AuthIdentity identity);

      // Shopping list support
      ShoppingList[] GetShoppingLists(AuthIdentity identity, IList<ShoppingList> lists, GetShoppingListOptions options);
      ShoppingListResult CreateShoppingList(AuthIdentity identity, ShoppingList list);
      ShoppingListResult UpdateShoppingList(AuthIdentity identity, Guid? listId, Guid[] toRemove, ShoppingListModification[] toModify, IShoppingListSource[] toAdd, string newName = null);
      void DeleteShoppingLists(AuthIdentity identity, ShoppingList[] lists);

      // Menu support
      Menu[] GetMenus(AuthIdentity identity, IList<Menu> menus, GetMenuOptions options);
      MenuResult CreateMenu(AuthIdentity identity, Menu menu, params Guid[] recipeIds);
      void DeleteMenus(AuthIdentity identity, params Guid[] menuIds);
      MenuResult UpdateMenu(AuthIdentity identity, Guid? menuId, Guid[] recipesAdd, Guid[] recipesRemove, MenuMove[] recipesMove, bool clear, string newName = null);
   }
}