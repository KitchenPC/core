using System;

namespace KitchenPC
{
   public class Weight : IComparable, IFormattable, IComparable<int>, IEquatable<int>
   {
      public int Value;

      public Weight()
      {
         Value = 0;
      }

      public Weight(int grams)
      {
         Value = grams;
      }

      public static implicit operator Weight(int grams)
      {
         return new Weight(grams);
      }

      public static implicit operator int(Weight weight)
      {
         if ((object) weight == null)
         {
            return 0;
         }
         else
         {
            return weight.Value;
         }
      }

      public static bool operator ==(Weight x, Weight y)
      {
         if (ReferenceEquals(x, y))
         {
            return true;
         }

         if ((object) x == null || ((object) y == null))
         {
            return false;
         }

         return x.Value == y.Value;
      }

      public static bool operator !=(Weight x, Weight y)
      {
         return !(x == y);
      }

      public int CompareTo(object obj)
      {
         if (obj is Weight)
         {
            return this.Value.CompareTo(((Weight) obj).Value);
         }
         else
         {
            return this.Value.CompareTo(obj);
         }
      }

      public int CompareTo(int other)
      {
         return this.Value.CompareTo(other);
      }

      public bool Equals(int other)
      {
         return this.Value.Equals(other);
      }

      public override bool Equals(object o)
      {
         if (o is Int32)
            return (this.Value == (Int32) o);
         else if (o is Weight)
            return (this.Value == ((Weight) o).Value);
         else return false;
      }

      public override string ToString()
      {
         return String.Format("{0:f} g.", Value);
      }

      public string ToString(string format, IFormatProvider formatProvider)
      {
         return ToString();
      }

      public override int GetHashCode()
      {
         return Value.GetHashCode();
      }

      public new bool Equals(object x, object y)
      {
         if (ReferenceEquals(x, y)) return true;

         if (x == null || y == null) return false;

         return x.Equals(y);
      }
   }
}