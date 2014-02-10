using System;
using System.Collections.Generic;
using KitchenPC.Recipes;

namespace KitchenPC.Ingredients
{
   public class IngredientSection
   {
      public string SectionName { get; private set; }
      public IngredientUsageCollection Ingredients { get; private set; }

      IngredientSection(string name)
      {
         SectionName = name;
         Ingredients = new IngredientUsageCollection();
      }

      public static IngredientSection[] GetSections(Recipe recipe)
      {
         //TODO: This code can probably be done in a LINQ expression, or more efficiently.

         var nullSection = new IngredientSection(null);
         var map = new Dictionary<string, IngredientSection>();

         foreach (var usage in recipe.Ingredients)
         {
            if (String.IsNullOrEmpty(usage.Section))
            {
               nullSection.Ingredients.Add(usage);
            }
            else
            {
               var sectionKey = usage.Section.ToLower();

               IngredientSection sectionList;
               if (map.TryGetValue(sectionKey, out sectionList))
               {
                  sectionList.Ingredients.Add(usage);
               }
               else
               {
                  sectionList = new IngredientSection(usage.Section);
                  sectionList.Ingredients.Add(usage);
                  map.Add(sectionKey, sectionList);
               }
            }
         }

         var ret = new List<IngredientSection>();
         if (nullSection.Ingredients.Count > 0)
         {
            ret.Add(nullSection);
         }

         ret.AddRange(map.Values);

         return ret.ToArray();
      }
   }
}