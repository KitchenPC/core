using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.NLP
{
   public class Preps : IEnumerable<PrepNode>
   {
      readonly List<PrepNode> notes;

      public Preps()
      {
         notes = new List<PrepNode>();
      }

      public bool HasValue
      {
         get
         {
            return notes.Count != 0;
         }
      }

      public void Add(PrepNode prep)
      {
         notes.Add(prep);
      }

      public void Remove(PrepNode node)
      {
         notes.Remove(node);
      }

      public override string ToString()
      {
         if (notes.Count == 0)
         {
            return String.Empty;
         }

         return String.Join("//", notes.Select(p => { return p.Prep; }).ToArray());
      }

      public IEnumerator<PrepNode> GetEnumerator()
      {
         return notes.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return notes.GetEnumerator();
      }
   }
}