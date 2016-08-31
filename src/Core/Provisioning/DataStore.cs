using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Data.DTO;

namespace KitchenPC.Data
{
   /// <summary>Holds all data from a persisted KitchenPC context.  Used to migrate data from one context to another.</summary>
   public class DataStore
   {
      // Indexes
      Dictionary<Guid, RecipeMetadata> indexedRecipeMetadata;
      Dictionary<Guid, DTO.Ingredients> indexedIngredients;
      Dictionary<Guid, IngredientMetadata> indexedIngredientMetadata;
      Dictionary<Guid, IngredientForms> indexedIngredientForms;
      Dictionary<Guid, DTO.Recipes> indexedRecipes;
      Dictionary<Guid, RecipeIngredients[]> indexedRecipeIngredients;
      IEnumerable<RecipeData> searchIndex;

      // Core Data (Immutable)
      public IngredientForms[] IngredientForms { get; set; }
      public IngredientMetadata[] IngredientMetadata { get; set; }
      public DTO.Ingredients[] Ingredients { get; set; }
      public NlpAnomalousIngredients[] NlpAnomalousIngredients { get; set; }
      public NlpDefaultPairings[] NlpDefaultPairings { get; set; }
      public NlpFormSynonyms[] NlpFormSynonyms { get; set; }
      public NlpIngredientSynonyms[] NlpIngredientSynonyms { get; set; }
      public NlpPrepNotes[] NlpPrepNotes { get; set; }
      public NlpUnitSynonyms[] NlpUnitSynonyms { get; set; }

      // Recipe Data
      public List<DTO.Recipes> Recipes { get; set; }
      public List<RecipeMetadata> RecipeMetadata { get; set; }
      public List<RecipeIngredients> RecipeIngredients { get; set; }

      // User Data
      public List<Favorites> Favorites { get; set; }
      public List<DTO.Menus> Menus { get; set; }
      public List<QueuedRecipes> QueuedRecipes { get; set; }
      public List<RecipeRatings> RecipeRatings { get; set; }
      public List<DTO.ShoppingLists> ShoppingLists { get; set; }
      public List<ShoppingListItems> ShoppingListItems { get; set; }

      public void ClearIndexes()
      {
         indexedRecipeMetadata = null;
         indexedIngredients = null;
         indexedIngredientMetadata = null;
         indexedIngredientForms = null;
         indexedRecipes = null;
         indexedRecipeIngredients = null;
         searchIndex = null;
      }

      public Dictionary<Guid, RecipeMetadata> GetIndexedRecipeMetadata()
      {
         if (indexedRecipeMetadata == null)
            indexedRecipeMetadata = RecipeMetadata.ToDictionary(m => m.RecipeId);

         return indexedRecipeMetadata;
      }

      public Dictionary<Guid, DTO.Ingredients> GetIndexedIngredients()
      {
         if (indexedIngredients == null)
            indexedIngredients = Ingredients.ToDictionary(i => i.IngredientId);

         return indexedIngredients;
      }

      public Dictionary<Guid, IngredientMetadata> GetIndexedIngredientMetadata()
      {
         if (indexedIngredientMetadata == null)
            indexedIngredientMetadata = IngredientMetadata.ToDictionary(i => i.IngredientId);

         return indexedIngredientMetadata;
      }

      public Dictionary<Guid, IngredientForms> GetIndexedIngredientForms()
      {
         if (indexedIngredientForms == null)
            indexedIngredientForms = IngredientForms.ToDictionary(i => i.IngredientFormId);

         return indexedIngredientForms;
      }

      public Dictionary<Guid, DTO.Recipes> GetIndexedRecipes()
      {
         if (indexedRecipes == null)
            indexedRecipes = Recipes.ToDictionary(r => r.RecipeId);

         return indexedRecipes;
      }

      public Dictionary<Guid, RecipeIngredients[]> GetIndexedRecipeIngredients()
      {
         if (indexedRecipeIngredients == null)
            indexedRecipeIngredients = RecipeIngredients.GroupBy(r => r.RecipeId).ToDictionary(g => g.Key, i => i.ToArray());

         return indexedRecipeIngredients;
      }

      public IEnumerable<RecipeData> GetSearchIndex()
      {
         var indexIngredients = GetIndexedRecipeIngredients();
         var indexMetadata = GetIndexedRecipeMetadata();

         if (searchIndex == null)
            searchIndex = Recipes.Select(v => new RecipeData
            {
               Recipe = v,
               Metadata = indexMetadata.ContainsKey(v.RecipeId) ? indexMetadata[v.RecipeId] : null,
               Ingredients = indexIngredients.ContainsKey(v.RecipeId) ? indexIngredients[v.RecipeId] : null
            });

         return searchIndex;
      }
   }
}