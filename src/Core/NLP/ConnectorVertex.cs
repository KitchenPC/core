using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.NLP
{
   public class ConnectorVertex<T>
   {
      readonly List<T> connections;

      public ConnectorVertex()
      {
         connections = new List<T>();
      }

      public void AddConnection(T node)
      {
         connections.Add(node);
      }

      public bool HasConnection(T node)
      {
         return connections.Contains<T>(node);
      }

      public IEnumerable<T> GetConnections()
      {
         var e = connections.GetEnumerator();
         while (e.MoveNext())
         {
            yield return e.Current;
         }
      }
   }
}