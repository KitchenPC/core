using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KitchenPC.Context;
using KitchenPC.Recipes;
using log4net;

namespace KitchenPC.Modeler
{
   /// <summary>
   /// Represents a modeling session for a given user with a given pantry.
   /// This object can be re-used (or cached) while the user changes modeling preferences and remodels.
   /// </summary>
   public class ModelingSession
   {
      const int MAX_SUGGESTIONS = 15;
      const double COOLING_RATE = 0.9999;
      const float MISSING_ING_PUNISH = 5.0f;
      const float NEW_ING_PUNISH = 2.0f;
      const float EMPTY_RECIPE_AMOUNT = 0.50f;

      readonly Random random = new Random();
      readonly IngredientNode[] pantryIngredients;
      readonly Dictionary<IngredientNode, float?> pantryAmounts;
      readonly List<IngredientNode> ingBlacklist;
      readonly Dictionary<RecipeNode, byte> ratings;

      readonly bool[] favTags; //Truth table of fav tags
      readonly int[] favIngs; //Array of top 5 fav ings by id

      readonly RecipeTags AllowedTags; //Copy of profile

      Dictionary<IngredientNode, IngredientUsage> totals; //Hold totals for each scoring round so we don't have to reallocate map every time

      readonly DBSnapshot db;
      readonly IKPCContext context;
      readonly IUserProfile profile;
      public static ILog Log = LogManager.GetLogger(typeof (ModelingSession));

      /// <summary>
      /// Create a ModelingSession instance.
      /// </summary>
      /// <param name="context">KitchenPC context used for this modeling session.</param>
      /// <param name="db">Object containing all available recipes, ratings, and trend information.</param>
      /// <param name="profile">Object containing user specific information, such as pantry and user ratings.</param>
      public ModelingSession(IKPCContext context, DBSnapshot db, IUserProfile profile)
      {
         this.db = db;
         this.context = context;
         this.profile = profile;
         this.favTags = new bool[RecipeTag.NUM_TAGS];
         this.favIngs = new int[profile.FavoriteIngredients.Length];

         if (profile.Pantry != null && profile.Pantry.Length == 0) //Empty pantries must be null, not zero items
         {
            throw new EmptyPantryException();
         }

         if (profile.AllowedTags != null)
         {
            AllowedTags = profile.AllowedTags;
         }

         if (profile.Pantry != null)
         {
            pantryAmounts = new Dictionary<IngredientNode, float?>();
            foreach (var item in profile.Pantry)
            {
               var node = this.db.FindIngredient(item.IngredientId);

               //If an ingredient isn't used by any recipe, there's no reason for it to be in the pantry.
               if (node == null)
               {
                  continue;
               }

               //If an ingredient exists, but doesn't have any link to any allowed tags, there's no reason for it to be in the pantry.
               if (AllowedTags != null && (node.AvailableTags & AllowedTags) == 0)
               {
                  continue;
               }

               if (pantryAmounts.ContainsKey(node))
               {
                  throw new DuplicatePantryItemException();
               }

               pantryAmounts.Add(node, item.Amt);
            }

            if (pantryAmounts.Keys.Count == 0)
            {
               throw new ImpossibleQueryException();
            }

            pantryIngredients = pantryAmounts.Keys.ToArray();
         }

         if (profile.FavoriteIngredients != null)
         {
            var i = 0;
            foreach (var ing in profile.FavoriteIngredients)
            {
               var node = this.db.FindIngredient(ing);
               favIngs[i] = node.Key;
            }
         }

         if (profile.FavoriteTags != null)
         {
            foreach (var tag in profile.FavoriteTags)
            {
               this.favTags[tag.Value] = true;
            }
         }

         if (profile.BlacklistedIngredients != null)
         {
            ingBlacklist = new List<IngredientNode>();
            foreach (var ing in profile.BlacklistedIngredients)
            {
               var node = this.db.FindIngredient(ing);
               ingBlacklist.Add(node);
            }
         }

         if (profile.Ratings != null)
         {
            ratings = new Dictionary<RecipeNode, byte>(profile.Ratings.Length);
            foreach (var r in profile.Ratings)
            {
               var n = this.db.FindRecipe(r.RecipeId);
               ratings.Add(n, r.Rating);
            }
         }
         else
         {
            ratings = new Dictionary<RecipeNode, byte>(0);
         }
      }

      /// <summary>
      /// Generates a model with the specified number of recipes and returns the recipe IDs in the optimal order.
      /// </summary>
      /// <param name="recipes">Number of recipes to generate</param>
      /// <param name="scale">Scale indicating importance of optimal ingredient usage vs. user trend usage. 1 indicates ignore user trends, return most efficient set of recipes. 5 indicates ignore pantry and generate recipes user is most likely to rate high.</param>
      /// <returns>An array up to size "recipes" containing recipes from DBSnapshot.</returns>
      public Model Generate(int recipes, byte scale)
      {
         if (recipes > MAX_SUGGESTIONS)
         {
            throw new ArgumentException("Modeler can only generate " + MAX_SUGGESTIONS.ToString() + " recipes at a time.");
         }

         var temperature = 10000.0;
         double deltaScore = 0;
         const double absoluteTemperature = 0.00001;

         totals = new Dictionary<IngredientNode, IngredientUsage>(IngredientNode.NextKey);

         var currentSet = new RecipeNode[recipes]; //current set of recipes
         var nextSet = new RecipeNode[recipes]; //set to compare with current
         InitSet(currentSet); //Initialize with n random recipes
         var score = GetScore(currentSet, scale); //Check initial score

         var timer = new Stopwatch();
         timer.Start();

         while (temperature > absoluteTemperature)
         {
            nextSet = GetNextSet(currentSet); //Swap out a random recipe with another one from the available pool
            deltaScore = GetScore(nextSet, scale) - score;

            //if the new set has a smaller score (good thing)
            //or if the new set has a higher score but satisfies Boltzman condition then accept the set
            if ((deltaScore < 0) || (score > 0 && Math.Exp(-deltaScore/temperature) > random.NextDouble()))
            {
               nextSet.CopyTo(currentSet, 0);
               score += deltaScore;
            }

            //cool down the temperature
            temperature *= COOLING_RATE;
         }

         timer.Stop();
         Log.InfoFormat("Generating set of {0} recipes took {1}ms.", recipes, timer.ElapsedMilliseconds);

         return new Model(currentSet, profile.Pantry, score);
      }

      /// <summary>Takes a model generated from the modeling engine and loads necessary data from the database to deliver relevance to a user interface.</summary>
      /// <param name="model">Model from modeling engine</param>
      /// <returns>CompiledModel object which contains full recipe information about the provided set.</returns>
      public CompiledModel Compile(Model model)
      {
         var results = new CompiledModel();

         var recipes = context.ReadRecipes(model.RecipeIds, ReadRecipeOptions.None);

         results.RecipeIds = model.RecipeIds;
         results.Pantry = model.Pantry;
         results.Briefs = recipes.Select(r => { return new RecipeBrief(r); }).ToArray();
         results.Recipes = recipes.Select(r => new SuggestedRecipe
         {
            Id = r.Id,
            Ingredients = context.AggregateRecipes(r.Id).ToArray()
         }).ToArray();

         return results;
      }

      /// <summary>
      /// Judges a set of recipes based on a scale and its efficiency with regards to the current pantry.  The lower the score, the better.
      /// </summary>
      double GetScore(RecipeNode[] currentSet, byte scale)
      {
         double wasted = 0; //Add 1.0 for ingredients that don't exist in pantry, add percentage of leftover otherwise
         float avgRating = 0; //Average rating for all recipes in the set (0-4)
         float tagPoints = 0; //Point for each tag that's one of our favorites
         float tagTotal = 0; //Total number of tags in all recipes
         float ingPoints = 0; //Point for each ing that's one of our favorites
         float ingTotal = 0; //Total number of ingrediets in all recipes

         for (var i = 0; i < currentSet.Length; i++)
         {
            var recipe = currentSet[i];
            var ingredients = (IngredientUsage[]) recipe.Ingredients;

            //Add points for any favorite tags this recipe uses

            tagTotal += recipe.Tags.Length;
            ingTotal += ingredients.Length;

            for (var t = 0; t < recipe.Tags.Length; t++) //TODO: Use bitmasks for storing recipe tags and fav tags, then count bits
            {
               if (favTags[t])
               {
                  tagPoints++;
               }
            }

            byte realRating; //Real rating is the user's rating, else the public rating, else 3.
            if (!ratings.TryGetValue(recipe, out realRating))
            {
               realRating = (recipe.Rating == 0) ? (byte) 3 : recipe.Rating; //if recipe has no ratings, use average rating of 3.
            }
            avgRating += (realRating - 1);

            for (var j = 0; j < ingredients.Length; j++)
            {
               var ingredient = ingredients[j];

               //Add points for any favorite ingredients this recipe uses
               var ingKey = ingredient.Ingredient.Key;

               for (var k = 0; k < favIngs.Length; k++) //For loop is actually faster than 5 "if" statements
               {
                  if (favIngs[k] == ingKey)
                  {
                     ingPoints++;
                     break;
                  }
               }

               IngredientUsage curUsage;
               var fContains = totals.TryGetValue(ingredient.Ingredient, out curUsage);
               if (!fContains)
               {
                  curUsage = new IngredientUsage();
                  curUsage.Amt = ingredient.Amt;
                  totals.Add(ingredient.Ingredient, curUsage);
               }
               else
               {
                  curUsage.Amt += ingredient.Amt;
               }
            }
         }

         if (profile.Pantry != null) //If profile has a pantry, figure out how much of it is wasted
         {
            //For each pantry ingredient that we're not using, punish the score by MISSING_ING_PUNISH amount.
            var pEnum = pantryAmounts.GetEnumerator();
            while (pEnum.MoveNext())
            {
               if (!totals.ContainsKey(pEnum.Current.Key))
               {
                  wasted += MISSING_ING_PUNISH;
               }
            }

            var e = totals.GetEnumerator();
            while (e.MoveNext())
            {
               var curKey = e.Current.Key;

               float? have;
               if (pantryAmounts.TryGetValue(curKey, out have)) //We have this in our pantry
               {
                  if (!have.HasValue) //We have this in our pantry, but no amount is specified - So we "act" like we have whatever we need
                  {
                     continue;
                  }

                  if (!e.Current.Value.Amt.HasValue) //This recipe doesn't specify an amount - So we "act" like we use half of what we have
                  {
                     wasted += EMPTY_RECIPE_AMOUNT;
                     continue;
                  }

                  var need = e.Current.Value.Amt.Value;
                  var ratio = 1 - ((have.Value - need)/have.Value); //Percentage of how much you're using of what you have
                  if (ratio > 1) //If you need more than you have, add the excess ratio to the waste but don't go over the punishment for not having the ingredient at all
                  {
                     wasted += Math.Min(ratio, NEW_ING_PUNISH);
                  }
                  else
                  {
                     wasted += (1 - ratio);
                  }
               }
               else
               {
                  wasted += NEW_ING_PUNISH; //For each ingredient this meal set needs that we don't have, increment by NEW_ING_PUNISH
               }
            }
         }

         double worstScore, trendScore, efficiencyScore;

         if (profile.Pantry == null) //No pantry, efficiency is defined by the overlap of ingredients across recipes
         {
            efficiencyScore = totals.Keys.Count/ingTotal;
         }
         else //Efficiency is defined by how efficient the pantry ingredients are utilized
         {
            worstScore = ((totals.Keys.Count*NEW_ING_PUNISH) + (profile.Pantry.Length*MISSING_ING_PUNISH)); //Worst possible efficiency score
            efficiencyScore = (wasted/worstScore);
         }

         avgRating /= currentSet.Length;
         trendScore = 1 - ((((avgRating/4)*4) + (tagPoints/tagTotal) + (ingPoints/ingTotal))/6);

         totals.Clear();

         if (scale == 1)
            return efficiencyScore;
         else if (scale == 2)
            return (efficiencyScore + efficiencyScore + trendScore)/3;
         else if (scale == 3)
            return (efficiencyScore + trendScore)/2;
         else if (scale == 4)
            return (efficiencyScore + trendScore + trendScore)/3;
         else if (scale == 5)
            return trendScore;

         return 0;
      }

      /// <summary>
      /// Initializes currentSet with random recipes from the available recipe pool.
      /// </summary>
      void InitSet(RecipeNode[] currentSet)
      {
         var inUse = new List<Guid>(currentSet.Length);

         for (var i = 0; i < currentSet.Length; i++)
         {
            RecipeNode g;
            var timeout = 0;
            do
            {
               g = Fish();

               if (++timeout > 100) //Ok we've tried 100 times to find a recipe not already in this set, there just isn't enough data to work with for this query
               {
                  throw new ImpossibleQueryException(); //TODO: Maybe we can lower the demanded meals and return what we do have
               }
            } while (inUse.Contains(g.RecipeId));

            inUse.Add(g.RecipeId);
            currentSet[i] = g;
         }
      }

      /// <summary>
      /// Swap out a random recipe with another one from the available pool
      /// </summary>
      RecipeNode[] GetNextSet(RecipeNode[] currentSet)
      {
         var rndIndex = random.Next(currentSet.Length);
         var existingRecipe = currentSet[rndIndex];
         RecipeNode newRecipe;

         var timeout = 0;
         while (true)
         {
            if (++timeout > 100) //We've tried 100 times to replace a recipe in this set, but cannot find anything that isn't already in this set.
            {
               throw new ImpossibleQueryException(); //TODO: If this is the only set of n which match that query, we've solved it - just return this set as final!
            }

            newRecipe = Fish();
            if (newRecipe == existingRecipe)
            {
               continue;
            }

            var fFound = false;
            for (var i = 0; i < currentSet.Length; i++)
            {
               if (newRecipe == currentSet[i])
               {
                  fFound = true;
                  break;
               }
            }

            if (!fFound)
            {
               break;
            }
         }

         var retSet = new RecipeNode[currentSet.Length];
         currentSet.CopyTo(retSet, 0);
         retSet[rndIndex] = newRecipe;

         return retSet;
      }

      /// <summary>
      /// Finds a random recipe in the available recipe pool
      /// </summary>
      /// <returns></returns>
      RecipeNode Fish()
      {
         RecipeNode recipeNode;

         if (pantryIngredients == null) //No pantry, fish through Recipe index
         {
            int rnd;
            var tag = (AllowedTags == null) ? random.Next(RecipeTag.NUM_TAGS) : AllowedTags[random.Next(AllowedTags.Length)].Value;
            var recipesByTag = db.FindRecipesByTag(tag);
            if (recipesByTag == null || recipesByTag.Length == 0) //Nothing in that tag
               return Fish();

            rnd = random.Next(recipesByTag.Length);
            recipeNode = recipesByTag[rnd];
         }
         else //Fish through random pantry ingredient
         {
            var rndIng = random.Next(pantryIngredients.Length);
            var ingNode = pantryIngredients[rndIng];

            RecipeNode[] recipes;
            if (AllowedTags != null && AllowedTags.Length > 0)
            {
               if ((AllowedTags & ingNode.AvailableTags) == 0) //Does this ingredient have any allowed tags?
               {
                  return Fish(); //No - Find something else
               }

               //Pick random tag from allowed tags (since this set is smaller, better to guess an available tag)
               while (true)
               {
                  var t = random.Next(AllowedTags.Length); //NOTE: Next will NEVER return MaxValue, so we don't subtract 1 from Length!
                  var rndTag = AllowedTags[t];
                  recipes = ingNode.RecipesByTag[rndTag.Value] as RecipeNode[];

                  if (recipes != null)
                  {
                     break;
                  }
               }
            }
            else //Just pick a random available tag
            {
               var rndTag = random.Next(ingNode.AvailableTags.Length);
               var tag = ingNode.AvailableTags[rndTag];
               recipes = ingNode.RecipesByTag[tag.Value] as RecipeNode[];
            }

            var rndRecipe = random.Next(recipes.Length);
            recipeNode = recipes[rndRecipe];
         }

         //If there's a blacklist, make sure no ingredients are blacklisted otherwise try again
         if (this.ingBlacklist != null)
         {
            var ingredients = (IngredientUsage[]) recipeNode.Ingredients;
            for (var i = 0; i < ingredients.Length; i++)
            {
               if (this.ingBlacklist.Contains(ingredients[i].Ingredient))
               {
                  return Fish();
               }
            }
         }

         //Discard if this recipe is to be avoided
         if (profile.AvoidRecipe.HasValue && profile.AvoidRecipe.Value.Equals(recipeNode.RecipeId))
            return Fish();

         return recipeNode;
      }
   }
}