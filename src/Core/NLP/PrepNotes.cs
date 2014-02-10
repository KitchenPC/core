using System.Collections.Generic;

namespace KitchenPC.NLP
{
   public class PrepNotes : SynonymTree<PrepNode>
   {
      static readonly object MapInitLock = new object();

      public static void InitIndex(ISynonymLoader<PrepNode> loader)
      {
         lock (MapInitLock)
         {
            index = new AlphaTree<PrepNode>();
            synonymMap = new Dictionary<string, PrepNode>();
            var preps = loader.LoadSynonyms();

            foreach (var prep in preps)
            {
               IndexString(prep.Prep, prep);
            }
         }
      }
   }
}