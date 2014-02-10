using System.Collections.Generic;
using System.Linq;
using KitchenPC.Recipes;

namespace KitchenPC.Categorization
{
   public class RecipeIndex
   {
      readonly Dictionary<IToken, int> index = new Dictionary<IToken, int>();

      public int EntryCount
      {
         get
         {
            return index.Values.Sum();
         }
      }

      public int GetTokenCount(IToken token)
      {
         return index.ContainsKey(token) ? index[token] : 0;
      }

      public void Add(Recipe recipe)
      {
         var tokens = Tokenizer.Tokenize(recipe);
         foreach (var token in tokens)
         {
            if (index.ContainsKey(token))
            {
               index[token]++;
            }
            else
            {
               index.Add(token, 1);
            }
         }
      }
   }
}