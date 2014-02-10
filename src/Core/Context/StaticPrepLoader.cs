using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Data;
using KitchenPC.NLP;

namespace KitchenPC.Context
{
   public class StaticPrepLoader : ISynonymLoader<PrepNode>
   {
      readonly DataStore store;

      public StaticPrepLoader(DataStore store)
      {
         this.store = store;
      }

      public IEnumerable<PrepNode> LoadSynonyms()
      {
         var forms = store.NlpFormSynonyms.Select(p => p.Name);
         var preps = store.NlpPrepNotes.Select(p => p.Name);

         var ret = forms
            .Concat(preps)
            .Distinct()
            .Select(p => new PrepNode(p))
            .ToList();

         return ret;
      }

      public Pairings LoadFormPairings()
      {
         throw new NotImplementedException();
      }
   }
}