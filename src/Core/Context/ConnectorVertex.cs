using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.Context
{
   public class ConnectorVertex
   {
      readonly List<IngredientNode> connections;

      public ConnectorVertex()
      {
         connections = new List<IngredientNode>();
      }

      public IEnumerable<IngredientNode> Connections
      {
         get
         {
            return connections.AsEnumerable();
         }
      }

      public void AddConnection(IngredientNode node)
      {
         connections.Add(node);
      }

      public bool HasConnection(IngredientNode node)
      {
         return connections.Contains<IngredientNode>(node);
      }
   }
}