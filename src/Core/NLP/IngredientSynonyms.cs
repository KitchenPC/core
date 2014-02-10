using System.Collections.Generic;

namespace KitchenPC.NLP
{
   public class IngredientSynonyms : SynonymTree<IngredientNode>
   {
      static readonly object MapInitLock = new object();

      public static void InitIndex(ISynonymLoader<IngredientNode> loader)
      {
         lock (MapInitLock)
         {
            index = new AlphaTree<IngredientNode>();
            synonymMap = new Dictionary<string, IngredientNode>();
            var ings = loader.LoadSynonyms();

            foreach (var ing in ings)
            {
               IndexString(ing.IngredientName, ing);
            }
         }
      }
   }
}