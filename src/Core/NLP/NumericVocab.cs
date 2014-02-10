using System.Collections.Generic;

namespace KitchenPC.NLP
{
   public class NumericVocab : SynonymTree<NumericNode>
   {
      static readonly object MapInitLock = new object();

      public static void InitIndex()
      {
         lock (MapInitLock)
         {
            index = new AlphaTree<NumericNode>();
            synonymMap = new Dictionary<string, NumericNode>();

            //Basic numbers, we can add more if needed
            string[] numbers = {"one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen", "twenty"};
            for (var i = 0; i < numbers.Length; i++)
            {
               IndexString(numbers[i], new NumericNode(numbers[i], i + 1));
            }

            //Other numeric tokens
            NumericNode[] tokens =
            {
               new NumericNode("a", 1),
               new NumericNode("an", 1),
               new NumericNode("half a", 0.5f),
               new NumericNode("half of a", 0.5f),
               new NumericNode("half an", 0.5f),
               new NumericNode("half of an", 0.5f),
               new NumericNode("a dozen", 12),
               new NumericNode("one dozen", 12),
               new NumericNode("a couple", 2),
               new NumericNode("a couple of", 2)
            };

            foreach (var t in tokens)
            {
               IndexString(t.Token, t);
            }
         }
      }
   }
}