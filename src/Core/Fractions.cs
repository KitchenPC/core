using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KitchenPC.Core;

public static class Fractions
{
   private static readonly Dictionary<decimal, string> Map;

   static Fractions()
   {
      Map = new Dictionary<decimal, string>
      {
         { 0, "" },
         { 0.5m, "1/2" },
         { 0.333m, "1/3" },
         { 0.667m, "2/3" },
         { 0.25m, "1/4" },
         { 0.75m, "3/4" },
         { 0.2m, "1/5" },
         { 0.4m, "2/5" },
         { 0.6m, "3/5" },
         { 0.8m, "4/5" },
         { 0.167m, "1/6" },
         { 0.833m, "5/6" },
         { 0.143m, "1/7" },
         { 0.286m, "2/7" },
         { 0.429m, "3/7" },
         { 0.571m, "4/7" },
         { 0.714m, "5/7" },
         { 0.857m, "6/7" },
         { 0.125m, "1/8" },
         { 0.375m, "3/8" },
         { 0.625m, "5/8" },
         { 0.875m, "7/8" },
         { 0.111m, "1/9" },
         { 0.222m, "2/9" },
         { 0.444m, "4/9" },
         { 0.556m, "5/9" },
         { 0.778m, "7/9" },
         { 0.889m, "8/9" },
         { 0.1m, "1/10" },
         { 0.3m, "3/10" },
         { 0.7m, "7/10" },
         { 0.9m, "9/10" },
         { 0.091m, "1/11" },
         { 0.182m, "2/11" },
         { 0.273m, "3/11" },
         { 0.364m, "4/11" },
         { 0.455m, "5/11" },
         { 0.545m, "6/11" },
         { 0.636m, "7/11" },
         { 0.727m, "8/11" },
         { 0.818m, "9/11" },
         { 0.909m, "10/11" },
         { 0.083m, "1/12" },
         { 0.417m, "5/12" },
         { 0.583m, "7/12" },
         { 0.917m, "11/12" },
         { 0.077m, "1/13" },
         { 0.154m, "2/13" },
         { 0.231m, "3/13" },
         { 0.308m, "4/13" },
         { 0.385m, "5/13" },
         { 0.462m, "6/13" },
         { 0.538m, "7/13" },
         { 0.615m, "8/13" },
         { 0.692m, "9/13" },
         { 0.769m, "10/13" },
         { 0.846m, "11/13" },
         { 0.923m, "12/13" },
         { 0.071m, "1/14" },
         { 0.214m, "3/14" },
         { 0.357m, "5/14" },
         { 0.643m, "9/14" },
         { 0.786m, "11/14" },
         { 0.929m, "13/14" },
         { 0.067m, "1/15" },
         { 0.133m, "2/15" },
         { 0.267m, "4/15" },
         { 0.467m, "7/15" },
         { 0.533m, "8/15" },
         { 0.733m, "11/15" },
         { 0.867m, "13/15" },
         { 0.933m, "14/15" },
         { 0.062m, "1/16" },
         { 0.188m, "3/16" },
         { 0.312m, "5/16" },
         { 0.438m, "7/16" },
         { 0.562m, "9/16" },
         { 0.688m, "11/16" },
         { 0.812m, "13/16" },
         { 0.938m, "15/16" }
      };
   }

   public static string FromDecimal(decimal value)
   {
      var whole = Math.Floor(value);
      string frac;
      var rounded = Math.Round(value - whole, 3);

      if (Map.ContainsKey(rounded))
      {
         frac = Map[rounded];
      }
      else
      {
         var ret = Math.Round(value, 1);
         return (ret == 0) ? "1/16" : ret.ToString(CultureInfo.InvariantCulture);
      }

      if (string.IsNullOrEmpty(frac))
      {
         return whole.ToString(CultureInfo.InvariantCulture);
      }

      return whole == 0
         ? frac
         : $"{whole} {frac}";
   }

   public static decimal ParseFraction(string fraction)
   {
      if (TryParseFraction(fraction, out decimal ret))
      {
         return ret;
      }

      throw new ArgumentException("Invalid Fraction: " + fraction);
   }

   public static bool TryParseFraction(string fraction, out decimal result)
   {
      if (decimal.TryParse(fraction, out result)) //If it's not a fraction, just parse it normally and return
      {
         return true;
      }

      var re = new Regex(@"^\s*(?:(\d+)(?:\s))?\s*(\d+)\s*\/\s*(\d+)\s*$");
      var match = re.Match(fraction);
      if (match.Success)
      {
         int.TryParse(match.Groups[1].Value, out int whole);
         int.TryParse(match.Groups[2].Value, out int numerator);
         int.TryParse(match.Groups[3].Value, out int denominator);

         if (denominator == 0)
         {
            return false;
         }

         result = whole + ((decimal) numerator/denominator);
         return true;
      }

      return false;
   }
}
