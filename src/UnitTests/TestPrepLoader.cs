using System;
using System.Collections.Generic;
using KitchenPC.NLP;

namespace KitchenPC.UnitTests
{
   internal class TestPrepLoader : ISynonymLoader<PrepNode>
   {
      public Pairings LoadFormPairings()
      {
         throw new NotImplementedException();
      }

      public IEnumerable<PrepNode> LoadSynonyms()
      {
         return new PrepNode[]
         {
            "sliced", "shredded", "crumbled", "diced", "chopped" //Test prep nodes (approved prep notes for any ingredient)
         };
      }
   }
}