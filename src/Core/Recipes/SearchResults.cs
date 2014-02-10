using System;

namespace KitchenPC.Recipes
{
   public class SearchResults
   {
      public RecipeBrief[] Briefs;

      public Int64 TotalCount { get; set; }

      public SearchResults(RecipeBrief[] briefs, Int64 total)
      {
         this.Briefs = briefs;
         this.TotalCount = total;
      }

      public SearchResults()
      {
      }
   }
}