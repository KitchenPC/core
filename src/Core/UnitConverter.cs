using System;

namespace KitchenPC
{
   public static class UnitConverter
   {
      static readonly Single[,] ConversionMatrix =
      {
         {1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}, //Unit
         {-1, 1.0f, 3.0f, 6.0f, 48.0f, 96.0f, 192.0f, 768.0f, -1, -1, -1}, //Teaspoon
         {-1, 1/3.0f, 1, 2.0f, 16.0f, 32.0f, 64.0f, 256.0f, -1, -1, -1}, //Tablespoon
         {-1, 1/6.0f, 0.5f, 1, 8.0f, 16.0f, 32.0f, 128.0f, -1, -1, -1}, //FluidOunce
         {-1, 1/48.0f, 1/16.0f, 0.125f, 1, 2.0f, 4.0f, 16.0f, -1, -1, -1}, //Cup
         {-1, 1/96.0f, 1/32.0f, 1/16.0f, 0.5f, 1, 2.0f, 8.0f, -1, -1, -1}, //Pint
         {-1, 1/192.0f, 1/64.0f, 1/32.0f, 0.25f, 0.5f, 1, 4.0f, -1, -1, -1}, //Quart
         {-1, 1/768.0f, 1/256.0f, 1/128.0f, 1/16.0f, 0.125f, 0.25f, 1, -1, -1, -1}, //Gallon
         {-1, -1, -1, -1, -1, -1, -1, -1, 1, 28.3495231f, 453.59237f}, //Gram
         {-1, -1, -1, -1, -1, -1, -1, -1, 1/28.3495231f, 1, 16.0f}, //Ounce
         {-1, -1, -1, -1, -1, -1, -1, -1, 1/453.59237f, 0.0625f, 1} //Pound
      };

      public static bool CanConvert(Units fromUnit, Units toUnit)
      {
         return (ConversionMatrix[(int) toUnit, (int) fromUnit] != -1);
      }

      public static Single Convert(Single amount, Units fromUnit, Units toUnit)
      {
         if (fromUnit == toUnit)
            return amount;

         if (ConversionMatrix[(int) toUnit, (int) fromUnit] == -1)
            throw new ArgumentException("Cannot convert from unit " + Unit.GetSingular(fromUnit) + " to unit " + Unit.GetSingular(toUnit));

         var coefficient = ConversionMatrix[(int) toUnit, (int) fromUnit];
         return amount*coefficient;
      }

      public static Amount Convert(Amount amount, Units toUnit)
      {
         var ret = new Amount();
         ret.Unit = toUnit;
         ret.SizeLow = amount.SizeLow.HasValue ? (float?) Convert(amount.SizeLow.Value, amount.Unit, toUnit) : null;
         ret.SizeHigh = Convert(amount.SizeHigh, amount.Unit, toUnit);

         return ret;
      }
   }
}