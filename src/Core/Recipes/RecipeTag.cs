using System;

namespace KitchenPC.Recipes
{
   public class RecipeTag
   {
      public const int NUM_TAGS = 17;

      readonly int value; //Ordinal value of tag
      readonly int bitflag; //Bitmask value (power of 2)
      readonly string label; //Name of tag

      public int Value
      {
         get
         {
            return value;
         }
      }

      public int BitFlag
      {
         get
         {
            return bitflag;
         }
      }

      public RecipeTag()
      {
      }

      RecipeTag(int value, string label)
      {
         this.value = value;
         this.bitflag = 1 << value;
         this.label = label;
      }

      public static bool operator !=(RecipeTag x, RecipeTag y)
      {
         return !(x == y);
      }

      public static bool operator ==(RecipeTag x, RecipeTag y)
      {
         if (ReferenceEquals(x, y))
         {
            return true;
         }

         if ((object) x == null || ((object) y == null))
         {
            return false;
         }

         return x.value == y.value;
      }

      public static RecipeTags operator |(RecipeTag x, RecipeTag y)
      {
         return (RecipeTags) x.bitflag | y.bitflag;
      }

      public static implicit operator string(RecipeTag tags)
      {
         return tags.label;
      }

      public static implicit operator RecipeTag(string tag)
      {
         if (tag == GlutenFree.label) return GlutenFree;
         if (tag == NoAnimals.label) return NoAnimals;
         if (tag == NoMeat.label) return NoMeat;
         if (tag == NoPork.label) return NoPork;
         if (tag == NoRedMeat.label) return NoRedMeat;
         if (tag == Breakfast.label) return Breakfast;
         if (tag == Dessert.label) return Dessert;
         if (tag == Dinner.label) return Dinner;
         if (tag == Lunch.label) return Lunch;
         if (tag == LowCalorie.label) return LowCalorie;
         if (tag == LowCarb.label) return LowCarb;
         if (tag == LowFat.label) return LowFat;
         if (tag == LowSodium.label) return LowSodium;
         if (tag == LowSugar.label) return LowSugar;
         if (tag == Common.label) return Common;
         if (tag == Easy.label) return Easy;
         if (tag == Quick.label) return Quick;

         throw new ArgumentException("Cannot parse recipe tag: " + tag);
      }

      public override int GetHashCode()
      {
         return this.value;
      }

      public override string ToString()
      {
         return this.label;
      }

      public override bool Equals(object obj)
      {
         var tag = obj as RecipeTag;
         return (tag != null && tag.value == this.value);
      }

      public static RecipeTag GlutenFree = new RecipeTag(0, "Gluten Free");
      public static RecipeTag NoAnimals = new RecipeTag(1, "No Animals");
      public static RecipeTag NoMeat = new RecipeTag(2, "No Meat");
      public static RecipeTag NoPork = new RecipeTag(3, "No Pork");
      public static RecipeTag NoRedMeat = new RecipeTag(4, "No Red Meat");
      public static RecipeTag Breakfast = new RecipeTag(5, "Breakfast");
      public static RecipeTag Dessert = new RecipeTag(6, "Dessert");
      public static RecipeTag Dinner = new RecipeTag(7, "Dinner");
      public static RecipeTag Lunch = new RecipeTag(8, "Lunch");
      public static RecipeTag LowCalorie = new RecipeTag(9, "Low Calorie");
      public static RecipeTag LowCarb = new RecipeTag(10, "Low Carb");
      public static RecipeTag LowFat = new RecipeTag(11, "Low Fat");
      public static RecipeTag LowSodium = new RecipeTag(12, "Low Sodium");
      public static RecipeTag LowSugar = new RecipeTag(13, "Low Sugar");
      public static RecipeTag Common = new RecipeTag(14, "Common Ingredients");
      public static RecipeTag Easy = new RecipeTag(15, "Easy To Make");
      public static RecipeTag Quick = new RecipeTag(16, "Quick");
   }
}