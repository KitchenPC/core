using System;

namespace KitchenPC
{
   public class Amount : IEquatable<Amount>
   {
      public Single? SizeLow;
      public Single SizeHigh;
      public Units Unit;

      /// <summary>Attempts to find a more suitable unit for this amount.</summary>
      public static Amount Normalize(Amount amount, float multiplier)
      {
         var ret = new Amount(amount)*multiplier;
         var low = ret.SizeLow.GetValueOrDefault();
         var hi = ret.SizeHigh;

         if (KitchenPC.Unit.GetConvType(ret.Unit) == UnitType.Weight)
         {
            if (ret.Unit == Units.Ounce && (low%16 + hi%16) == 0)
            {
               ret /= 16;
               ret.Unit = Units.Pound;
            }
         }

         if (KitchenPC.Unit.GetConvType(ret.Unit) == UnitType.Volume)
         {
            //If teaspoons, convert to Tlb (3tsp in 1Tbl)
            if (ret.Unit == Units.Teaspoon && (low%3 + hi%3) == 0)
            {
               ret /= 3;
               ret.Unit = Units.Tablespoon;
            }

            //If Fl Oz, convert to cup (8 fl oz in 1 cup)
            if (ret.Unit == Units.FluidOunce && (low%8 + hi%8) == 0)
            {
               ret /= 8;
               ret.Unit = Units.Cup;
            }

            //If pints, convert to quarts (2 pints in a quart)
            if (ret.Unit == Units.Pint && (low%2 + hi%2) == 0)
            {
               ret /= 2;
               ret.Unit = Units.Quart;
            }

            //If quarts, convert to gallons (4 quarts in a gallon)
            if (ret.Unit == Units.Quart && (low%4 + hi%4) == 0)
            {
               ret /= 4;
               ret.Unit = Units.Gallon;
            }
         }

         return ret;
      }

      public Amount(Single size, Units unit)
      {
         SizeHigh = size;
         Unit = unit;
      }

      public Amount(Single? from, Single to, Units unit)
      {
         SizeLow = from;
         SizeHigh = to;
         Unit = unit;
      }

      public Amount(Amount amount)
      {
         SizeLow = amount.SizeLow;
         SizeHigh = amount.SizeHigh;
         Unit = amount.Unit;
      }

      public Amount() : this(0, Units.Unit)
      {
      }

      public static Amount operator *(Amount amt, float coef)
      {
         return new Amount(amt.SizeLow*coef, amt.SizeHigh*coef, amt.Unit);
      }

      public static Amount operator /(Amount amt, float den)
      {
         return new Amount(amt.SizeLow/den, amt.SizeHigh/den, amt.Unit);
      }

      public static Amount operator +(Amount amt, float oper)
      {
         return new Amount(amt.SizeLow + oper, amt.SizeHigh + oper, amt.Unit);
      }

      public static Amount operator -(Amount amt, float oper)
      {
         return new Amount(amt.SizeLow - oper, amt.SizeHigh - oper, amt.Unit);
      }

      public static Amount operator +(Amount a1, Amount a2)
      {
         if (a1.Unit == a2.Unit) //Just add
         {
            if (a1.SizeLow.HasValue && a2.SizeLow.HasValue) //Combine the lows, combine the highs
               return new Amount(a1.SizeLow + a2.SizeLow, a1.SizeHigh + a2.SizeHigh, a1.Unit);

            if (a1.SizeLow.HasValue) //(1-2) + 1 = (2-3)
               return new Amount(a1.SizeLow + a2.SizeHigh, a1.SizeHigh + a2.SizeHigh, a1.Unit);

            if (a2.SizeLow.HasValue) //1 + (1-2) = (2-3)
               return new Amount(a1.SizeHigh + a2.SizeLow, a1.SizeHigh + a2.SizeHigh, a1.Unit);

            //just combine the highs
            return new Amount(a1.SizeHigh + a2.SizeHigh, a1.Unit);
         }

         if (UnitConverter.CanConvert(a1.Unit, a2.Unit))
         {
            //TODO: Handle range + nonrange
            var newLow = a2.SizeLow.HasValue ? (float?) UnitConverter.Convert(a2.SizeLow.Value, a2.Unit, a1.Unit) : null;
            var newHigh = a1.SizeHigh + UnitConverter.Convert(a2.SizeHigh, a2.Unit, a1.Unit);
            return new Amount(newLow, newHigh, a1.Unit);
         }

         throw new IncompatibleAmountException();
      }

      public Amount Round(int decimalPlaces)
      {
         var ret = new Amount(this);

         ret.SizeLow = SizeLow.HasValue ? (float?) Math.Round(SizeLow.Value, decimalPlaces) : null;
         ret.SizeHigh = (float) Math.Round(SizeHigh, decimalPlaces);

         return ret;
      }

      public Amount RoundUp(Single nearestMultiple)
      {
         var ret = new Amount(this);
         ret.SizeLow = SizeLow.HasValue ? (float?) (Math.Ceiling(SizeLow.Value/nearestMultiple)*nearestMultiple) : null;
         ret.SizeHigh = (float) Math.Ceiling(SizeHigh/nearestMultiple)*nearestMultiple;

         return ret;
      }

      public override string ToString()
      {
         return ToString((SizeLow.HasValue || SizeHigh > 1) ? KitchenPC.Unit.GetPlural(Unit) : KitchenPC.Unit.GetSingular(Unit));
      }

      public string ToString(string unit)
      {
         string hi;
         string low;

         if (KitchenPC.Unit.GetConvType(Unit) == UnitType.Weight) //Render in decimal
         {
            hi = Math.Round(SizeHigh, 2).ToString();
            low = SizeLow.HasValue ? Math.Round(SizeLow.Value, 2).ToString() : null;
         }
         else //Render in fractions
         {
            hi = Fractions.FromDecimal((decimal) SizeHigh);
            low = SizeLow.HasValue ? Fractions.FromDecimal((decimal) SizeLow.Value) : null;
         }

         var amt = (low != null) ? String.Format("{0} - {1}", low, hi) : hi;
         return String.Format("{0} {1}", amt, unit).Trim();
      }

      public bool Equals(Amount other)
      {
         if (other is Amount) //Check for null
         {
            return (other.SizeLow == SizeLow && other.SizeHigh == SizeHigh && other.Unit == Unit);
         }

         return false;
      }

      public override bool Equals(object obj)
      {
         if (obj is Amount)
         {
            return this.Equals((Amount) obj);
         }

         return false;
      }

      public static bool operator ==(Amount x, Amount y)
      {
         if (x is Amount)
         {
            return x.Equals(y);
         }

         return !(y is Amount);
      }

      public static bool operator !=(Amount x, Amount y)
      {
         if (x is Amount)
         {
            return !x.Equals(y);
         }

         return (y is Amount);
      }

      public override int GetHashCode()
      {
         return SizeLow.GetHashCode() ^ SizeHigh.GetHashCode();
      }
   }
}