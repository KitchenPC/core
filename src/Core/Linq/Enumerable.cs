using System;
using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.Linq
{
   /// <summary>Linq extensions for KitchenPC data types</summary>
   public static class Enumerable
   {
      public static Amount Sum(this IEnumerable<Amount> source)
      {
         var total = new Amount();

         return source.Aggregate(total, (current, a) => current + a);
      }

      public static Amount Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Amount> selector)
      {
         var total = new Amount();

         return source.Aggregate(total, (current, a) => (Amount) (current + selector(a)));
      }
   }
}