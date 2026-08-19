using System;

namespace KitchenPC.Core.Recipes;

public class SearchResults
{
   public RecipeBrief[] Briefs { get; set; }

   public Int64 TotalCount { get; set; }

   public SearchResults(RecipeBrief[] briefs, Int64 total)
   {
      this.Briefs = briefs;
      this.TotalCount = total;
   }

   public SearchResults() { }
}
