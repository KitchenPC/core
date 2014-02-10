using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.Recipes
{
   public class RecipeTags : IEnumerable<RecipeTag>
   {
      readonly int mask; //Bitmask value, where each bit indicates the tag in that ordinal position
      readonly List<RecipeTag> tags; //Enumerable list of RecipeTag objects

      public static RecipeTags None
      {
         get
         {
            return 0;
         }
      }

      public RecipeTags()
      {
         tags = new List<RecipeTag>(13);
         mask = 0;
      }

      public static RecipeTags Parse(string list)
      {
         var tags = from t in list.Split(',') select t.Trim();

         var ret = new RecipeTags();
         ret = tags.Aggregate(ret, (current, tag) => current | (RecipeTag) tag);

         return ret;
      }

      public static RecipeTags From(params RecipeTag[] tags)
      {
         var ret = new RecipeTags();
         ret.tags.AddRange(tags);

         return ret;
      }

      public int Length
      {
         get
         {
            return tags.Count;
         }
      }

      public RecipeTag this[int index]
      {
         get
         {
            return tags[index];
         }
      }

      public bool HasTag(RecipeTag tag)
      {
         return (this & tag) > 0;
      }

      public static RecipeTags operator |(RecipeTags x, RecipeTag y)
      {
         return x.mask | y.BitFlag;
      }

      public static int operator &(RecipeTags x, RecipeTag y)
      {
         return x.mask & y.BitFlag;
      }

      public static bool operator !=(RecipeTags x, RecipeTags y)
      {
         return !(x == y);
      }

      public static bool operator ==(RecipeTags x, RecipeTags y)
      {
         if (ReferenceEquals(x, y))
         {
            return true;
         }

         if ((object) x == null || ((object) y == null))
         {
            return false;
         }

         return x.mask == y.mask;
      }

      public static implicit operator int(RecipeTags tags)
      {
         return tags.mask;
      }

      public static implicit operator RecipeTags(int value)
      {
         return new RecipeTags(value);
      }

      RecipeTags(int mask)
      {
         this.mask = mask;
         tags = new List<RecipeTag>(13);

         if ((this & RecipeTag.GlutenFree) > 0) tags.Add(RecipeTag.GlutenFree);
         if ((this & RecipeTag.NoAnimals) > 0) tags.Add(RecipeTag.NoAnimals);
         if ((this & RecipeTag.NoMeat) > 0) tags.Add(RecipeTag.NoMeat);
         if ((this & RecipeTag.NoPork) > 0) tags.Add(RecipeTag.NoPork);
         if ((this & RecipeTag.NoRedMeat) > 0) tags.Add(RecipeTag.NoRedMeat);
         if ((this & RecipeTag.Breakfast) > 0) tags.Add(RecipeTag.Breakfast);
         if ((this & RecipeTag.Dessert) > 0) tags.Add(RecipeTag.Dessert);
         if ((this & RecipeTag.Dinner) > 0) tags.Add(RecipeTag.Dinner);
         if ((this & RecipeTag.Lunch) > 0) tags.Add(RecipeTag.Lunch);
         if ((this & RecipeTag.LowCalorie) > 0) tags.Add(RecipeTag.LowCalorie);
         if ((this & RecipeTag.LowCarb) > 0) tags.Add(RecipeTag.LowCarb);
         if ((this & RecipeTag.LowFat) > 0) tags.Add(RecipeTag.LowFat);
         if ((this & RecipeTag.LowSodium) > 0) tags.Add(RecipeTag.LowSodium);
         if ((this & RecipeTag.LowSugar) > 0) tags.Add(RecipeTag.LowSugar);
         if ((this & RecipeTag.Common) > 0) tags.Add(RecipeTag.Common);
         if ((this & RecipeTag.Easy) > 0) tags.Add(RecipeTag.Easy);
         if ((this & RecipeTag.Quick) > 0) tags.Add(RecipeTag.Quick);
      }

      public override bool Equals(object obj)
      {
         var t = obj as RecipeTags;
         return (t != null && this.mask == t.mask);
      }

      public override string ToString()
      {
         return ToString(", ");
      }

      public string ToString(string delimiter)
      {
         var labels = (from n in tags select n.ToString()).ToArray();
         return String.Join(delimiter, labels);
      }

      public override int GetHashCode()
      {
         return mask;
      }

      public IEnumerator<RecipeTag> GetEnumerator()
      {
         return tags.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return tags.GetEnumerator();
      }

      //Required to serialize class
      public void Add(object obj)
      {
         throw new NotImplementedException();
      }
   }
}