using System;
using System.Collections.Generic;
using KitchenPC.Ingredients;

namespace KitchenPC.NLP
{
   public class FormSynonyms : SynonymTree<FormNode>
   {
      static readonly object MapInitLock = new object();
      static Pairings pairings;

      public static void InitIndex(ISynonymLoader<FormNode> loader)
      {
         lock (MapInitLock)
         {
            index = new AlphaTree<FormNode>();
            synonymMap = new Dictionary<string, FormNode>();

            foreach (var form in loader.LoadSynonyms())
            {
               IndexString(form.FormName, form);
            }

            pairings = loader.LoadFormPairings();
         }
      }

      public static bool TryGetFormForIngredient(string formname, Guid ing, out IngredientForm form)
      {
         form = null;
         FormNode node;
         if (false == synonymMap.TryGetValue(formname, out node))
         {
            return false;
         }

         var pair = new NameIngredientPair(formname, ing);
         return pairings.TryGetValue(pair, out form);
      }

      public static bool TryGetFormForPrep(Preps preps, IngredientNode ing, bool remove, out IngredientForm form)
      {
         //TODO: Do we need to check all preps, or just the one that was on the input
         foreach (var prep in preps)
         {
            var fMatch = TryGetFormForIngredient(prep.Prep, ing.Id, out form);
            if (!fMatch) continue;

            if (remove)
               preps.Remove(prep);

            return true;
         }

         form = null;
         return false;
      }
   }
}