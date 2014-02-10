using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KitchenPC.Ingredients;

namespace KitchenPC.Recipes
{
   public class Recipe
   {
      readonly IngredientUsageCollection _ingredients;

      public Guid Id;
      public Guid OwnerId;
      public String OwnerAlias;
      public String Title;
      public String Description;
      public String ImageUrl;
      public String Credit;
      public String CreditUrl;
      public String Permalink;
      public String Method;
      public DateTime DateEntered;
      public short? PrepTime = 0;
      public short? CookTime = 0;
      public short AvgRating = 0;
      public Rating UserRating = Rating.None;
      public short ServingSize = 4;
      public int Comments = 0;
      public RecipeTags Tags = null;
      public bool PublicEdit = false;
      public bool AllowDelete = false;
      public int InMenus = 0;

      public static Recipe FromId(Guid recipeId)
      {
         return new Recipe(recipeId, null, null, null);
      }

      public IngredientUsage[] Ingredients
      {
         get
         {
            return _ingredients.ToArray();
         }
         set
         {
            _ingredients.Clear();

            if (value != null)
            {
               foreach (var usage in value)
                  _ingredients.Add(usage);
            }
         }
      }

      public Recipe(Guid id, String title, String description, String imageurl)
      {
         _ingredients = new IngredientUsageCollection();

         Id = id;
         Title = title;
         Description = description;
         ImageUrl = imageurl;
      }

      public Recipe() : this(Guid.Empty, null, null, null)
      {
      }

      public void AddIngredient(IngredientUsage ingredient)
      {
         _ingredients.Add(ingredient);
      }

      public void AddIngredients(IEnumerable<IngredientUsage> ingredients)
      {
         foreach (var ingredientUsage in ingredients)
         {
            AddIngredient(ingredientUsage);
         }
      }

      public static void Validate(Recipe recipe)
      {
         if (String.IsNullOrEmpty(recipe.Title) || recipe.Title.Trim().Length == 0)
         {
            throw new InvalidRecipeDataException("A recipe title is required.");
         }

         if (recipe.PrepTime < 0)
         {
            throw new InvalidRecipeDataException("PrepTime must be equal to or greater than zero.");
         }

         if (recipe.CookTime < 0)
         {
            throw new InvalidRecipeDataException("CookTime must be equal to or greater than zero.");
         }

         if (recipe.ServingSize <= 0)
         {
            throw new InvalidRecipeDataException("ServingSize must be greater than zero.");
         }

         if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
         {
            throw new InvalidRecipeDataException("Recipes must contain at least one ingredient.");
         }

         if (recipe.Tags == null || recipe.Tags.Length == 0)
         {
            throw new InvalidRecipeDataException("Recipes must contain at least one tag.");
         }

         if (recipe.Description != null && recipe.Description.Length > 250)
         {
            recipe.Description = recipe.Description.Substring(0, 250) + "...";
         }

         if (recipe.CreditUrl != null)
         {
            const string pattern = @"^https?\://(?<domain>[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3})(/\S*)?$";
            var m = Regex.Match(recipe.CreditUrl, pattern);
            if (m.Success && m.Groups["domain"].Success)
            {
               recipe.CreditUrl = m.Value;
               recipe.Credit = m.Groups["domain"].Value.ToLower(); //TODO: Clean up domain name
            }
            else //Bad URL, clear Credit info
            {
               recipe.Credit = null;
               recipe.CreditUrl = null;
            }
         }
      }
   }
}