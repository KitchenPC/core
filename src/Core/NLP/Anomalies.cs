using System.Collections.Generic;

namespace KitchenPC.NLP
{
   public class Anomalies : SynonymTree<AnomalousNode>
   {
      static readonly object MapInitLock = new object();

      public static void InitIndex(ISynonymLoader<AnomalousNode> loader)
      {
         lock (MapInitLock)
         {
            index = new AlphaTree<AnomalousNode>();
            synonymMap = new Dictionary<string, AnomalousNode>();
            var anomalies = loader.LoadSynonyms();

            foreach (var anom in anomalies)
            {
               IndexString(anom.Name, anom);
            }
         }
      }
   }
}