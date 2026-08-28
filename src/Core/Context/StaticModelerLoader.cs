using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.Provisioning;
using KitchenPC.Core.Provisioning.DTO;
using KitchenPC.Core.ShoppingLists;

namespace KitchenPC.Core.Context;

public class StaticModelerLoader : IModelerLoader
{
   private readonly DataStore store;
   private IEnumerable<RecipeBinding> recipedata;
   private IEnumerable<IngredientBinding> ingredientdata;
   private IEnumerable<RatingBinding> ratingdata;

   public StaticModelerLoader(DataStore store)
   {
      this.store = store;
   }

   public IEnumerable<RecipeBinding> LoadRecipeGraph()
   {
      if (recipedata != null)
         return recipedata;

      // Initialize Recipe Graph
      var metadata = store.GetIndexedRecipeMetadata();
      var graph = (
         from r in store.Recipes
         join m in metadata on r.RecipeId equals m.Key
         select new RecipeBinding
         {
            Id = r.RecipeId,
            Rating = Convert.ToByte(r.Rating),
            Tags = RecipeMetadata.ToRecipeTags(m.Value),
         }
      );

      return (recipedata = graph);
   }

   public IEnumerable<IngredientBinding> LoadIngredientGraph()
   {
      if (ingredientdata != null)
         return ingredientdata;

      var forms = store.GetIndexedIngredientForms();
      var ingredients = store.GetIndexedIngredients();
      var graph = (
         from ri in store.RecipeIngredients
         join f in forms on ri.IngredientFormId equals f.Key
         join i in ingredients on ri.IngredientId equals i.Key
         where i.Key != ShoppingList.GUID_WATER
         select IngredientBinding.Create(
            i.Key,
            ri.RecipeId,
            ri.Qty,
            ri.Unit,
            i.Value.ConversionType,
            i.Value.UnitWeight,
            f.Value.UnitType,
            f.Value.FormAmount,
            f.Value.FormUnit,
            i.Value.DisplayName
         )
      );

      return (ingredientdata = graph);
   }

   public IEnumerable<RatingBinding> LoadRatingGraph()
   {
      if (ratingdata != null)
         return ratingdata;

      var graph = (
         from r in store.RecipeRatings
         select new RatingBinding
         {
            RecipeId = r.RecipeId,
            UserId = r.UserId,
            Rating = r.Rating,
         }
      );

      return (ratingdata = graph);
   }
}
