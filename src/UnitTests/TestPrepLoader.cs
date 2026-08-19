using System;
using System.Collections.Generic;
using KitchenPC.Core.NLP;

namespace KitchenPC.UnitTests;

internal class TestPrepLoader : ISynonymLoader<PrepNode>
{
   public Pairings LoadFormPairings() => throw new NotImplementedException();

   public IEnumerable<PrepNode> LoadSynonyms() =>
      new PrepNode[]
      {
         "sliced",
         "shredded",
         "crumbled",
         "diced",
         "chopped", //Test prep nodes (approved prep notes for any ingredient)
      };
}
