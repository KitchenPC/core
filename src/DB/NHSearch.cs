using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Recipes;
using KitchenPC.DB.Models;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Criterion.Lambda;

namespace KitchenPC.DB;

/// <summary>
/// A basic, platform-agnostic search provider implemented with NHibernate.
/// </summary>
public class NHSearch : ISearchProvider
{
   private readonly DatabaseAdapter adapter;

   public static NHSearch Instance(DatabaseAdapter adapter) => new(adapter);

   private NHSearch(DatabaseAdapter adapter)
   {
      this.adapter = adapter;
   }

   public SearchResults Search(AuthIdentity identity, RecipeQuery query)
   {
      return SearchAsync(identity, query).GetAwaiter().GetResult();
   }

   public async Task<SearchResults> SearchAsync(
      AuthIdentity identity,
      RecipeQuery query,
      CancellationToken cancellationToken = default
   )
   {
      using var session = adapter.GetSession();
      Recipes recipe = null;

      var q = session.QueryOver(() => recipe).Where(p => !p.Hidden);

      if (!string.IsNullOrWhiteSpace(query.Keywords)) // Add keyword search
      {
         q = q.Where(
            Restrictions.Or(
               Restrictions.InsensitiveLike("Title", string.Format("%{0}%", query.Keywords.Trim())),
               Restrictions.InsensitiveLike(
                  "Description",
                  string.Format("%{0}%", query.Keywords.Trim())
               )
            )
         );
      }

      if (query.Time.MaxPrep.HasValue)
      {
         q = q.Where(p => p.PrepTime <= query.Time.MaxPrep.Value);
      }

      if (query.Time.MaxCook.HasValue)
      {
         q = q.Where(p => p.CookTime <= query.Time.MaxCook.Value);
      }

      if (query.Time.MaxTime.HasValue)
      {
         q = q.Where(
            Restrictions.Le(
               Projections.SqlProjection(
                  "PrepTime::Int4 + CookTime::Int4 as TotalTime",
                  ["TotalTime"],
                  [NHibernateUtil.Int32]
               ),
               query.Time.MaxTime.Value
            )
         );
      }

      if (query.Rating > 0)
      {
         q = q.Where(p => p.Rating >= (int)query.Rating.Value);
      }

      if (query.Include != null && query.Include.Length > 0) // Add ingredients to include
      {
         // Create a sub-query for ingredients to include
         q = q.WithSubquery.WhereExists(
            QueryOver
               .Of<RecipeIngredients>()
               .Where(item => item.Recipe.RecipeId == recipe.RecipeId)
               .Where(
                  Restrictions.InG("Ingredient", query.Include.Select(Ingredients.FromId).ToArray())
               )
               .Select(i => i.RecipeIngredientId)
               .Take(1)
         );
      }

      if (query.Exclude != null && query.Exclude.Length > 0) // Add ingredients to exclude
      {
         // Create a sub-query for ingredients to exclude
         q = q.WithSubquery.WhereNotExists(
            QueryOver
               .Of<RecipeIngredients>()
               .Where(item => item.Recipe.RecipeId == recipe.RecipeId)
               .Where(
                  Restrictions.InG("Ingredient", query.Exclude.Select(Ingredients.FromId).ToArray())
               )
               .Select(i => i.RecipeIngredientId)
               .Take(1)
         );
      }

      if (
         query.Photos == RecipeQuery.PhotoFilter.Photo
         || query.Photos == RecipeQuery.PhotoFilter.HighRes
      )
      {
         q = q.Where(Restrictions.IsNotNull("ImageUrl"));
      }

      if (
         query.Diet
         || query.Nutrition
         || query.Skill
         || query.Taste
         || (query.Meal != MealFilter.All)
         || (query.Photos == RecipeQuery.PhotoFilter.HighRes)
      ) //Need to search in metadata
      {
         RecipeMetadata metadata = null;
         q = q.JoinAlias(r => r.RecipeMetadata, () => metadata);

         //Meal
         if (query.Meal != MealFilter.All)
         {
            if (query.Meal == MealFilter.Breakfast)
               q = q.Where(() => metadata.MealBreakfast);
            if (query.Meal == MealFilter.Dessert)
               q = q.Where(() => metadata.MealDessert);
            if (query.Meal == MealFilter.Dinner)
               q = q.Where(() => metadata.MealDinner);
            if (query.Meal == MealFilter.Lunch)
               q = q.Where(() => metadata.MealLunch);
         }

         //High-res photos
         if (query.Photos == RecipeQuery.PhotoFilter.HighRes)
            q = q.Where(() => metadata.PhotoRes >= 1024 * 768);

         //Diet
         if (query.Diet.GlutenFree)
            q = q.Where(() => metadata.DietGlutenFree);
         if (query.Diet.NoAnimals)
            q = q.Where(() => metadata.DietNoAnimals);
         if (query.Diet.NoMeat)
            q = q.Where(() => metadata.DietNomeat);
         if (query.Diet.NoPork)
            q = q.Where(() => metadata.DietNoPork);
         if (query.Diet.NoRedMeat)
            q = q.Where(() => metadata.DietNoRedMeat);

         //Nutrition
         if (query.Nutrition.LowCalorie)
            q = q.Where(() => metadata.NutritionLowCalorie);
         if (query.Nutrition.LowCarb)
            q = q.Where(() => metadata.NutritionLowCarb);
         if (query.Nutrition.LowFat)
            q = q.Where(() => metadata.NutritionLowFat);
         if (query.Nutrition.LowSodium)
            q = q.Where(() => metadata.NutritionLowSodium);
         if (query.Nutrition.LowSugar)
            q = q.Where(() => metadata.NutritionLowSugar);

         //Skill
         if (query.Skill.Common)
            q = q.Where(() => metadata.SkillCommon).OrderBy(() => metadata.Commonality).Desc();
         if (query.Skill.Easy)
            q = q.Where(() => metadata.SkillEasy);
         if (query.Skill.Quick)
            q = q.Where(() => metadata.SkillQuick);

         //Taste
         if (query.Taste.MildToSpicy != RecipeQuery.SpicinessLevel.Medium)
         {
            q =
               query.Taste.MildToSpicy < RecipeQuery.SpicinessLevel.Medium
                  ? q.Where(() => metadata.TasteMildToSpicy <= query.Taste.Spiciness)
                     .OrderBy(() => metadata.TasteMildToSpicy)
                     .Asc()
                  : q.Where(() => metadata.TasteMildToSpicy >= query.Taste.Spiciness)
                     .OrderBy(() => metadata.TasteMildToSpicy)
                     .Desc();
         }

         if (query.Taste.SavoryToSweet != RecipeQuery.SweetnessLevel.Medium)
         {
            q =
               query.Taste.SavoryToSweet < RecipeQuery.SweetnessLevel.Medium
                  ? q.Where(() => metadata.TasteSavoryToSweet <= query.Taste.Sweetness)
                     .OrderBy(() => metadata.TasteSavoryToSweet)
                     .Asc()
                  : q.Where(() => metadata.TasteSavoryToSweet >= query.Taste.Sweetness)
                     .OrderBy(() => metadata.TasteSavoryToSweet)
                     .Desc();
         }
      }

      IQueryOverOrderBuilder<Recipes, Recipes> orderBy = query.Sort switch
      {
         RecipeQuery.SortOrder.Title => q.OrderBy(p => p.Title),
         RecipeQuery.SortOrder.PrepTime => q.OrderBy(p => p.PrepTime),
         RecipeQuery.SortOrder.CookTime => q.OrderBy(p => p.CookTime),
         RecipeQuery.SortOrder.TotalTime => q.OrderBy(
            Projections.SqlProjection(
               "PrepTime::Int4 + CookTime::Int4 as TotalTime",
               ["TotalTime"],
               [NHibernateUtil.Int32]
            )
         ),
         RecipeQuery.SortOrder.Image => q.OrderBy(p => p.ImageUrl),
         _ => q.OrderBy(p => p.Rating),
      };
      var results = (
         query.Direction == RecipeQuery.SortDirection.Descending ? orderBy.Desc() : orderBy.Asc()
      )
         .Skip(query.Offset)
         .Take(100)
         .ListAsync(cancellationToken);

      var loadedResults = await results;

      return new SearchResults
      {
         Briefs = loadedResults.Select(r => r.AsRecipeBrief()).ToArray(),
         TotalCount = loadedResults.Count, // TODO: This needs to be the total matches, not the returned matches
      };
   }
}
