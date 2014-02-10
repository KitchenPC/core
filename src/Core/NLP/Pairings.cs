using System.Collections.Generic;
using KitchenPC.Ingredients;

namespace KitchenPC.NLP
{
   public class Pairings
   {
      readonly IDictionary<NameIngredientPair, IngredientForm> pairs;

      public Pairings()
      {
         pairs = new Dictionary<NameIngredientPair, IngredientForm>();
      }

      public void Add(NameIngredientPair key, IngredientForm value)
      {
         pairs.Add(key, value);
      }

      public bool ContainsKey(NameIngredientPair key)
      {
         return pairs.ContainsKey(key);
      }

      public bool TryGetValue(NameIngredientPair key, out IngredientForm value)
      {
         return pairs.TryGetValue(key, out value);
      }

      public IngredientForm this[NameIngredientPair key]
      {
         get
         {
            return pairs[key];
         }
         set
         {
            pairs[key] = value;
         }
      }
   }
}