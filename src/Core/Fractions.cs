using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace KitchenPC
{
   public static class Fractions
   {
      static readonly Dictionary<decimal, String> _map;

      static Fractions()
      {
         _map = new Dictionary<decimal, string>();
         _map.Add(0, "");
         _map.Add(0.5m, "1/2");
         _map.Add(0.333m, "1/3");
         _map.Add(0.667m, "2/3");
         _map.Add(0.25m, "1/4");
         _map.Add(0.75m, "3/4");
         _map.Add(0.2m, "1/5");
         _map.Add(0.4m, "2/5");
         _map.Add(0.6m, "3/5");
         _map.Add(0.8m, "4/5");
         _map.Add(0.167m, "1/6");
         _map.Add(0.833m, "5/6");
         _map.Add(0.143m, "1/7");
         _map.Add(0.286m, "2/7");
         _map.Add(0.429m, "3/7");
         _map.Add(0.571m, "4/7");
         _map.Add(0.714m, "5/7");
         _map.Add(0.857m, "6/7");
         _map.Add(0.125m, "1/8");
         _map.Add(0.375m, "3/8");
         _map.Add(0.625m, "5/8");
         _map.Add(0.875m, "7/8");
         _map.Add(0.111m, "1/9");
         _map.Add(0.222m, "2/9");
         _map.Add(0.444m, "4/9");
         _map.Add(0.556m, "5/9");
         _map.Add(0.778m, "7/9");
         _map.Add(0.889m, "8/9");
         _map.Add(0.1m, "1/10");
         _map.Add(0.3m, "3/10");
         _map.Add(0.7m, "7/10");
         _map.Add(0.9m, "9/10");
         _map.Add(0.091m, "1/11");
         _map.Add(0.182m, "2/11");
         _map.Add(0.273m, "3/11");
         _map.Add(0.364m, "4/11");
         _map.Add(0.455m, "5/11");
         _map.Add(0.545m, "6/11");
         _map.Add(0.636m, "7/11");
         _map.Add(0.727m, "8/11");
         _map.Add(0.818m, "9/11");
         _map.Add(0.909m, "10/11");
         _map.Add(0.083m, "1/12");
         _map.Add(0.417m, "5/12");
         _map.Add(0.583m, "7/12");
         _map.Add(0.917m, "11/12");
         _map.Add(0.077m, "1/13");
         _map.Add(0.154m, "2/13");
         _map.Add(0.231m, "3/13");
         _map.Add(0.308m, "4/13");
         _map.Add(0.385m, "5/13");
         _map.Add(0.462m, "6/13");
         _map.Add(0.538m, "7/13");
         _map.Add(0.615m, "8/13");
         _map.Add(0.692m, "9/13");
         _map.Add(0.769m, "10/13");
         _map.Add(0.846m, "11/13");
         _map.Add(0.923m, "12/13");
         _map.Add(0.071m, "1/14");
         _map.Add(0.214m, "3/14");
         _map.Add(0.357m, "5/14");
         _map.Add(0.643m, "9/14");
         _map.Add(0.786m, "11/14");
         _map.Add(0.929m, "13/14");
         _map.Add(0.067m, "1/15");
         _map.Add(0.133m, "2/15");
         _map.Add(0.267m, "4/15");
         _map.Add(0.467m, "7/15");
         _map.Add(0.533m, "8/15");
         _map.Add(0.733m, "11/15");
         _map.Add(0.867m, "13/15");
         _map.Add(0.933m, "14/15");
         _map.Add(0.062m, "1/16");
         _map.Add(0.188m, "3/16");
         _map.Add(0.312m, "5/16");
         _map.Add(0.438m, "7/16");
         _map.Add(0.562m, "9/16");
         _map.Add(0.688m, "11/16");
         _map.Add(0.812m, "13/16");
         _map.Add(0.938m, "15/16");
      }

      public static string FromDecimal(decimal value)
      {
         var whole = Math.Floor(value);
         var frac = String.Empty;
         var rounded = Math.Round(value - whole, 3);

         if (_map.ContainsKey(rounded))
         {
            frac = _map[rounded];
         }
         else
         {
            var ret = Math.Round(value, 1);
            return (ret == 0) ? "1/16" : ret.ToString();
         }

         if (String.IsNullOrEmpty(frac))
         {
            return whole.ToString();
         }
         else if (whole == 0)
         {
            return frac;
         }
         else
         {
            return String.Format("{0} {1}", whole, frac);
         }
      }

      public static decimal ParseFraction(string fraction)
      {
         decimal ret;
         if (TryParseFraction(fraction, out ret))
         {
            return ret;
         }
         else
         {
            throw new ArgumentException("Invalid Fraction: " + fraction);
         }
      }

      public static bool TryParseFraction(string fraction, out decimal result)
      {
         if (Decimal.TryParse(fraction, out result)) //If it's not a fraction, just parse it normally and return
         {
            return true;
         }

         var re = new Regex(@"^\s*(?:(\d+)(?:\s))?\s*(\d+)\s*\/\s*(\d+)\s*$");
         var match = re.Match(fraction);
         if (match.Success)
         {
            int whole;
            int numerator;
            int denominator;

            Int32.TryParse(match.Groups[1].Value, out whole);
            Int32.TryParse(match.Groups[2].Value, out numerator);
            Int32.TryParse(match.Groups[3].Value, out denominator);

            if (denominator == 0)
            {
               return false;
            }

            result = whole + ((decimal) numerator/denominator);
            return true;
         }
         else
         {
            return false;
         }
      }
   }
}