using System;
using KitchenPC.Recipes;

namespace KitchenPC.Modeler
{
   /// <summary>
   /// A fully compiled result set from the modeler containing full recipe briefs and ingredient aggregation data.
   /// </summary>
   public class CompiledModel
   {
      static CompiledModel empty;

      public RecipeBrief[] Briefs;
      public Guid[] RecipeIds { get; set; }
      public PantryItem[] Pantry { get; set; }
      public SuggestedRecipe[] Recipes { get; set; }

      public int Count
      {
         get
         {
            return (Recipes == null ? 0 : Recipes.Length);
         }
         set
         {
         }
      }

      public static CompiledModel Empty
      {
         get
         {
            if (empty == null)
            {
               empty = new CompiledModel()
               {
                  Briefs = new RecipeBrief[0],
                  Pantry = new PantryItem[0],
                  RecipeIds = new Guid[0],
                  Recipes = new SuggestedRecipe[0]
               };
            }

            return empty;
         }
      }
   }
}