using System;
using System.Linq;
using KitchenPC.Core;
using KitchenPC.Core.Context;
using KitchenPC.Core.Provisioning;
using KitchenPC.Core.Recipes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecipeDto = KitchenPC.Core.Provisioning.DTO.Recipes;

namespace KitchenPC.UnitTests;

[TestClass]
public class RecipeSearchTest
{
   [TestMethod]
   public void StaticSearchReturnsStablePagesAndTotalCount()
   {
      var recipes = Enumerable
         .Range(0, 205)
         .Select(index => new RecipeDto
         {
            RecipeId = new Guid(index + 1, 0, 0, new byte[8]),
            Title = $"Recipe {index:D3}",
         })
         .ToList();
      var store = new DataStore
      {
         Recipes = recipes,
         RecipeIngredients = [],
         RecipeMetadata = [],
      };
      var search = new StaticSearch(store);

      var firstPage = search.Search(AuthIdentity.Anonymous, CreateQuery(0));
      var secondPage = search.Search(AuthIdentity.Anonymous, CreateQuery(100));
      var finalPage = search.Search(AuthIdentity.Anonymous, CreateQuery(200));
      var beyondEnd = search.Search(AuthIdentity.Anonymous, CreateQuery(300));

      Assert.AreEqual(205, firstPage.TotalCount);
      Assert.AreEqual(RecipeQuery.PageSize, firstPage.Briefs.Length);
      Assert.AreEqual(205, secondPage.TotalCount);
      Assert.AreEqual(RecipeQuery.PageSize, secondPage.Briefs.Length);
      Assert.AreEqual(205, finalPage.TotalCount);
      Assert.AreEqual(5, finalPage.Briefs.Length);
      Assert.AreEqual(205, beyondEnd.TotalCount);
      Assert.AreEqual(0, beyondEnd.Briefs.Length);

      CollectionAssert.AreEqual(
         recipes.Take(RecipeQuery.PageSize).Select(recipe => recipe.RecipeId).ToArray(),
         firstPage.Briefs.Select(recipe => recipe.Id).ToArray()
      );
      CollectionAssert.AreEqual(
         recipes
            .Skip(RecipeQuery.PageSize)
            .Take(RecipeQuery.PageSize)
            .Select(recipe => recipe.RecipeId)
            .ToArray(),
         secondPage.Briefs.Select(recipe => recipe.Id).ToArray()
      );
   }

   private static RecipeQuery CreateQuery(int offset) =>
      new()
      {
         Offset = offset,
         Sort = RecipeQuery.SortOrder.Title,
         Direction = RecipeQuery.SortDirection.Ascending,
      };
}
