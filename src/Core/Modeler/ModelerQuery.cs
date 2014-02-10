using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenPC.Modeler
{
   /// <summary>
   /// Represents a query for the modeler, such as a list of ingredients, recipes to avoid, and a number of recipes to return.
   /// </summary>
   public class ModelerQuery
   {
      public string[] Ingredients; //User-entered ingredients to be parsed by NLP
      public Guid? AvoidRecipe; //Avoid specific recipe, useful for swapping out one recipe for another
      public byte NumRecipes;
      public byte Scale;

      public string CacheKey
      {
         get
         {
            var bytes = new List<byte>();
            bytes.Add(NumRecipes); //First byte is number of recipes
            bytes.Add(Scale); //Second byte is the scale

            if (AvoidRecipe.HasValue)
               bytes.AddRange(AvoidRecipe.Value.ToByteArray());

            //Remaining bytes are defined ingredients, delimited by null
            if (Ingredients != null && Ingredients.Length > 0)
            {
               foreach (var ing in Ingredients)
               {
                  bytes.AddRange(Encoding.UTF8.GetBytes(ing.ToLower().Trim()));
                  bytes.Add(0); //Null delimiter
               }
            }

            return Convert.ToBase64String(bytes.ToArray());
         }
      }
   }
}