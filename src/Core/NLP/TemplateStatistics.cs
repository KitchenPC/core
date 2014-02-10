using System;
using System.Collections;
using System.Collections.Generic;

namespace KitchenPC.NLP
{
   public class TemplateStatistics : IEnumerable<TemplateStatistics.TemplateUsage>
   {
      public struct TemplateUsage
      {
         public string Template;
         public int Matches;

         public override string ToString()
         {
            return String.Format("{0} ---> {1} matches\n", Template, Matches);
         }
      }

      readonly Dictionary<Template, int> stats;

      public int this[Template t]
      {
         get
         {
            return stats[t];
         }
         set
         {
            stats[t] = value;
         }
      }

      public TemplateStatistics()
      {
         stats = new Dictionary<Template, int>();
      }

      public void RecordTemplate(Template template)
      {
         stats[template] = 0;
      }

      public IEnumerator<TemplateUsage> GetEnumerator()
      {
         var e = stats.GetEnumerator();
         while (e.MoveNext())
         {
            yield return new TemplateUsage() {Template = e.Current.Key.ToString(), Matches = e.Current.Value};
         }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         var e = stats.GetEnumerator();
         while (e.MoveNext())
         {
            yield return new TemplateUsage() {Template = e.Current.Key.ToString(), Matches = e.Current.Value};
         }
      }
   }
}