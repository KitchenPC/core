using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Core.NLP;
using KitchenPC.DB.Models;

namespace KitchenPC.DB.NLP;

public class PrepLoader : ISynonymLoader<PrepNode>
{
   private readonly DatabaseAdapter adapter;

   public PrepLoader(DatabaseAdapter adapter)
   {
      this.adapter = adapter;
   }

   public IEnumerable<PrepNode> LoadSynonyms()
   {
      using var session = adapter.GetStatelessSession();
      var forms = session.QueryOver<NlpFormSynonyms>().Select(p => p.Name).List<String>();
      var preps = session.QueryOver<NlpPrepNotes>().Select(p => p.Name).List<String>();

      var ret = forms.Concat(preps).Distinct().Select(p => new PrepNode(p)).ToList();

      return ret;
   }

   public Pairings LoadFormPairings() => throw new NotImplementedException();
}
