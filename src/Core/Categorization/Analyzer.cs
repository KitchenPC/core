using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Recipes;

namespace KitchenPC.Categorization
{
   public class Analyzer
   {
      const float TOLERANCE = .05f;

      float I;
      float invI;

      public RecipeIndex BreakfastIndex;
      public RecipeIndex LunchIndex;
      public RecipeIndex DinnerIndex;
      public RecipeIndex DessertIndex;

      Dictionary<Guid, IRecipeClassification> trainingData;

      public void LoadTrainingData(IDBLoader loader)
      {
         trainingData = new Dictionary<Guid, IRecipeClassification>();

         BreakfastIndex = new RecipeIndex();
         LunchIndex = new RecipeIndex();
         DinnerIndex = new RecipeIndex();
         DessertIndex = new RecipeIndex();

         var data = loader.LoadTrainingData();

         foreach (var recipe in data)
         {
            trainingData.Add(recipe.Recipe.Id, recipe);

            if (recipe.IsBreakfast) BreakfastIndex.Add(recipe.Recipe);
            if (recipe.IsLunch) LunchIndex.Add(recipe.Recipe);
            if (recipe.IsDinner) DinnerIndex.Add(recipe.Recipe);
            if (recipe.IsDessert) DessertIndex.Add(recipe.Recipe);
         }
      }

      public bool CheckIfTrained(Guid recipeId, out IRecipeClassification ret)
      {
         return trainingData.TryGetValue(recipeId, out ret);
      }

      public AnalyzerResult GetPrediction(Recipe recipe)
      {
         var winsBr = new Ranking(Category.Breakfast);
         var winsLu = new Ranking(Category.Lunch);
         var winsDi = new Ranking(Category.Dinner);
         var winsDe = new Ranking(Category.Dessert);

         //Setup Tournament
         Compete(recipe, BreakfastIndex, LunchIndex, winsBr, winsLu, winsDi, winsDe);
         Compete(recipe, BreakfastIndex, DinnerIndex, winsBr, winsLu, winsDi, winsDe);
         Compete(recipe, BreakfastIndex, DessertIndex, winsBr, winsLu, winsDi, winsDe);
         Compete(recipe, LunchIndex, DinnerIndex, winsBr, winsLu, winsDi, winsDe);
         Compete(recipe, LunchIndex, DessertIndex, winsBr, winsLu, winsDi, winsDe);
         Compete(recipe, DinnerIndex, DessertIndex, winsBr, winsLu, winsDi, winsDe);

         //Choose winner
         Ranking tag1, tag2;
         var result = GetWinner(winsBr, winsLu, winsDi, winsDe, out tag1, out tag2);

         return result;
      }

      void Compete(Recipe entry, RecipeIndex first, RecipeIndex second, Ranking winsBr, Ranking winsLu, Ranking winsDi, Ranking winsDe)
      {
         var res = GetPrediction(entry, first, second);
         if (res > .5f - TOLERANCE && res < .5f + TOLERANCE)
            return; //No winner

         var diff = (float) Math.Abs(res - 0.5);
         var winner = (res < 0.5 ? second : first);

         if (winner == BreakfastIndex) winsBr.Score += diff;
         if (winner == LunchIndex) winsLu.Score += diff;
         if (winner == DinnerIndex) winsDi.Score += diff;
         if (winner == DessertIndex) winsDe.Score += diff;
      }

      static AnalyzerResult GetWinner(Ranking winsBr, Ranking winsLu, Ranking winsDi, Ranking winsDe, out Ranking firstPlace, out Ranking secondPlace)
      {
         var meals = new Ranking[] {winsBr, winsLu, winsDi, winsDe};
         var sorted = (from m in meals orderby m.Score descending select m).ToArray();

         firstPlace = sorted[0];

         secondPlace = sorted[1].Score/sorted[0].Score > 0.8f ? sorted[1] : null;

         var ret = new AnalyzerResult(firstPlace.Type, (secondPlace != null ? secondPlace.Type : Category.None));
         return ret;
      }

      float GetPrediction(Recipe recipe, RecipeIndex first, RecipeIndex second)
      {
         invI = I = 0; //Reset I/invI
         var tokens = Tokenizer.Tokenize(recipe);

         foreach (var token in tokens)
         {
            var firstCount = first.GetTokenCount(token);
            var secondCount = second.GetTokenCount(token);

            CalcProbability(firstCount, first.EntryCount, secondCount, second.EntryCount);
         }

         var prediction = CombineProbability();
         return prediction;
      }

      void CalcProbability(float cat1count, float cat1total, float cat2count, float cat2total)
      {
         const float s = 1f;
         const float x = .5f;

         var bw = cat1count/cat1total;
         var gw = cat2count/cat2total;
         var pw = ((bw)/((bw) + (gw)));
         var n = cat1count + cat2count;
         var fw = ((s*x) + (n*pw))/(s + n);

         LogProbability(fw);
      }

      void LogProbability(float prob)
      {
         if (float.IsNaN(prob)) return;

         I = I == 0 ? prob : I*prob;
         invI = invI == 0 ? (1 - prob) : invI*(1 - prob);
      }

      float CombineProbability()
      {
         return I/(I + invI);
      }
   }
}