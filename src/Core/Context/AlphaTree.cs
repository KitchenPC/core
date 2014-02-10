namespace KitchenPC.Context
{
   public class AlphaTree
   {
      public Node Head;

      public AlphaTree()
      {
         Head = new Node();
      }

      public class Node
      {
         readonly Node[] nodes;
         public ConnectorVertex connections;

         public Node()
         {
            nodes = new Node[26];
         }

         public Node AddLink(char c)
         {
            var index = c - 97;
            return (nodes[index] = new Node());
         }

         public bool HasLink(char c)
         {
            var index = c - 97;
            return (nodes[index] != null);
         }

         public Node GetLink(char c)
         {
            var index = c - 97;
            return nodes[index];
         }

         public void AddConnection(IngredientNode node)
         {
            if (connections == null)
            {
               connections = new ConnectorVertex();
            }
            else
            {
               if (connections.HasConnection(node))
                  return;
            }

            connections.AddConnection(node);
         }
      }
   }
}