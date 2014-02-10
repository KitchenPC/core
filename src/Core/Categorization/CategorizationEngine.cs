using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Recipes;

namespace KitchenPC.Categorization
{
   public class CategorizationEngine
   {
      readonly Dictionary<Guid, IIngredientCommonality> commonIngs;
      readonly Analyzer analyzer;

      public CategorizationEngine(IDBLoader loader)
      {
         analyzer = new Analyzer();
         commonIngs = (from i in loader.LoadCommonIngredients() select i).ToDictionary(k => k.IngredientId);
         analyzer.LoadTrainingData(loader);
      }

      public CategorizationResult Categorize(Recipe recipe)
      {
         var result = new CategorizationResult();

         CategorizeMeal(recipe, result, analyzer);
         CategorizeDiet(recipe, result);
         CategorizeNutrition(recipe, result);
         CategorizeSkill(recipe, result);
         CategorizeTaste(recipe, result);

         return result;
      }

      static void CategorizeMeal(Recipe recipe, CategorizationResult result, Analyzer analyzer)
      {
         IRecipeClassification trainedRecipe;
         if (analyzer.CheckIfTrained(recipe.Id, out trainedRecipe))
         {
            result.Meal_Breakfast = trainedRecipe.IsBreakfast;
            result.Meal_Lunch = trainedRecipe.IsLunch;
            result.Meal_Dinner = trainedRecipe.IsDinner;
            result.Meal_Dessert = trainedRecipe.IsDessert;
         }
         else
         {
            var analysis = analyzer.GetPrediction(recipe);

            result.Meal_Breakfast = (analysis.FirstPlace.Equals(Category.Breakfast) || analysis.SecondPlace.Equals(Category.Breakfast));
            result.Meal_Lunch = (analysis.FirstPlace.Equals(Category.Lunch) || analysis.SecondPlace.Equals(Category.Lunch));
            result.Meal_Dinner = (analysis.FirstPlace.Equals(Category.Dinner) || analysis.SecondPlace.Equals(Category.Dinner));
            result.Meal_Dessert = (analysis.FirstPlace.Equals(Category.Dessert) || analysis.SecondPlace.Equals(Category.Dessert));
         }
      }

      static void CategorizeDiet(Recipe recipe, CategorizationResult result)
      {
         var ingmeta = (from ing in recipe.Ingredients select ing.Ingredient.Metadata).ToArray();

         var glutenFree = ingmeta.All(ing => ing.HasGluten == false);
         var noAnimals = ingmeta.All(ing => ing.HasAnimal == false);
         var noMeat = ingmeta.All(ing => ing.HasMeat == false);
         var noPork = ingmeta.All(ing => ing.HasPork == false);
         var noRed = ingmeta.All(ing => ing.HasRedMeat == false);

         result.Diet_GlutenFree = glutenFree;
         result.Diet_NoAnimals = noAnimals;
         result.Diet_NoMeat = noMeat;
         result.Diet_NoPork = noPork;
         result.Diet_NoRedMeat = noRed;
      }

      static void CategorizeNutrition(Recipe recipe, CategorizationResult result)
      {
         float totalGrams = 0, totalFat = 0, totalSugar = 0, totalCal = 0, totalSodium = 0, totalCarbs = 0;
         var noMatch = false;

         //First, convert every ingredient to weight
         foreach (var usage in recipe.Ingredients)
         {
            if (usage.Amount == null) //No amount specified for this ingredient (TODO: Any way to estimate this?)
            {
               noMatch = true;
               continue;
            }

            var meta = usage.Ingredient.Metadata;

            if (meta == null) //TODO: Log if ingredient has no metadata
            {
               noMatch = true;
               continue;
            }

            var amt = FormConversion.GetWeightForUsage(usage);
            if (amt == null)
            {
               noMatch = true;
               continue; //Cannot convert this ingredient to grams, skip it
            }

            var grams = amt.SizeHigh;
            totalGrams += grams;

            if (!(meta.FatPerUnit.HasValue && meta.SugarPerUnit.HasValue && meta.CaloriesPerUnit.HasValue && meta.SodiumPerUnit.HasValue && meta.CarbsPerUnit.HasValue))
            {
               noMatch = true;
            }

            if (meta.FatPerUnit.HasValue)
               totalFat += (meta.FatPerUnit.Value*grams)/100f; //Total fat per 100g

            if (meta.SugarPerUnit.HasValue)
               totalSugar += (meta.SugarPerUnit.Value*grams)/100f; //Total sugar per 100g;

            if (meta.CaloriesPerUnit.HasValue)
               totalCal += (meta.CaloriesPerUnit.Value*grams)/100f; //Total Calories per 100g

            if (meta.SodiumPerUnit.HasValue)
               totalSodium += (meta.SodiumPerUnit.Value*grams)/100f; //Total sodium per 100g

            if (meta.CarbsPerUnit.HasValue)
               totalCarbs += (meta.CarbsPerUnit.Value*grams)/100f; //Total carbs per 100g
         }

         result.USDAMatch = !noMatch; //Set to true if every ingredient has an exact USDA match

         //Set totals
         result.Nutrition_TotalFat = (short) totalFat;
         result.Nutrition_TotalSugar = (short) totalSugar;
         result.Nutrition_TotalCalories = (short) totalCal;
         result.Nutrition_TotalSodium = (short) totalSodium;
         result.Nutrition_TotalCarbs = (short) totalCarbs;

         //Flag RecipeMetadata depending on totals in recipe
         if (!noMatch)
         {
            result.Nutrition_LowFat = totalFat <= (totalCal*.03); //Definition of Low Fat is 3g of fat per 100 Cal
            result.Nutrition_LowSugar = totalSugar <= (totalCal*.02); //There is no FDA definition of "Low Sugar" (Can estimate 2g of sugar per 100 Cal or less)
            result.Nutrition_LowCalorie = totalCal <= (totalGrams*1.2); //Definition of Low Calorie is 120 cal per 100g
            result.Nutrition_LowSodium = totalSodium <= (totalGrams*1.4); //Definition of Low Sodium is 140mg per 100g
            result.Nutrition_LowCarb = totalCarbs <= (totalCal*.05); //No definition for Low Carb, but we can use 5g per 100 Cal or less
         }
      }

      void CategorizeSkill(Recipe recipe, CategorizationResult result)
      {
         //Common: Has 3 or more ingredients and all ingredients are considered "common"
         result.Skill_Common = recipe.Ingredients.Length >= 3 && recipe.Ingredients.All(i => commonIngs.ContainsKey(i.Ingredient.Id));
         result.Commonality = Convert.ToSingle(result.Skill_Common ? recipe.Ingredients.Average(i => commonIngs[i.Ingredient.Id].Commonality) : 0f);

         //Easy: Has the word "easy" in the title, or (prep <= 15min and ingredients <= 5)
         result.Skill_Easy = (recipe.Title.ToLower().Contains("easy") || (recipe.PrepTime <= 15 && recipe.Ingredients.Length <= 5));

         //Quick: prep <= 10 and cooktime <= 20
         result.Skill_Quick = (recipe.PrepTime <= 10 && recipe.CookTime <= 20);
      }

      static void CategorizeTaste(Recipe recipe, CategorizationResult result)
      {
         var totalMass = new Amount(0, Units.Gram);
         float totalSweet = 0f, totalSpicy = 0f;
         foreach (var usage in recipe.Ingredients)
         {
            if (usage.Amount == null) continue; //No amount specified for this ingredient (TODO: Any way to estimate this?)
            if (usage.Ingredient.Metadata == null) continue; //TODO: Log if ingredient has no metadata

            var meta = usage.Ingredient.Metadata;

            var amt = FormConversion.GetWeightForUsage(usage);
            if (amt == null) continue;

            totalMass += amt;
            totalSweet += (amt.SizeHigh*meta.Sweet);
            totalSpicy += (amt.SizeHigh*meta.Spicy);
         }

         if (totalMass.SizeHigh == 0) return; //Nothing to calc, exit

         var maxRating = totalMass.SizeHigh*4;
         var recipeSweet = (totalSweet/maxRating); //Pct sweet the recipe is
         var recipeSpicy = (totalSpicy/maxRating); //Pct spicy the recipe is

         result.Taste_SavoryToSweet = Convert.ToByte(recipeSweet*100); //Scale in terms of percentage
         result.Taste_MildToSpicy = Convert.ToByte(recipeSpicy*100);
      }
   }
}