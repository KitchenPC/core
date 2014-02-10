namespace KitchenPC.NLP
{
   public class AlphaTree<T>
   {
      public Node Head;

      public AlphaTree()
      {
         Head = new Node();
      }

      public class Node
      {
         public Node[] nodes;
         public ConnectorVertex<T> connections;

         public Node()
         {
            nodes = new Node[94];
         }

         public Node AddLink(char c)
         {
            var index = c - 32;
            return (nodes[index] = new Node());
         }

         public bool HasLink(char c)
         {
            var index = c - 32;
            return (nodes[index] != null);
         }

         public Node GetLink(char c)
         {
            var index = c - 32;
            return nodes[index];
         }

         public void AddConnection(T node)
         {
            if (connections == null)
            {
               connections = new ConnectorVertex<T>();
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