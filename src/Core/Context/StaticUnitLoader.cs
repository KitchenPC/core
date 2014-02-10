using System.Collections.Generic;
using System.Linq;
using KitchenPC.Data;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC.Context
{
   public class StaticUnitLoader : ISynonymLoader<UnitNode>
   {
      readonly DataStore store;

      public StaticUnitLoader(DataStore store)
      {
         this.store = store;
      }

      public IEnumerable<UnitNode> LoadSynonyms()
      {
         var unitSyn = store.NlpUnitSynonyms
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .Distinct()
            .ToList();

         return new List<CustomUnitNode>(unitSyn.Select(s => new CustomUnitNode(s)));
      }

      public Pairings LoadFormPairings()
      {
         var forms = store.GetIndexedIngredientForms();
         var unitSyn = store.NlpUnitSynonyms;
         var pairings = new Pairings();

         foreach (var syn in unitSyn)
         {
            var form = forms[syn.FormId];

            pairings.Add(new NameIngredientPair(
               syn.Name.Trim(),
               syn.IngredientId),
               new IngredientForm(
                  form.IngredientFormId,
                  form.IngredientId,
                  form.UnitType,
                  form.FormDisplayName,
                  form.UnitName,
                  form.ConvMultiplier,
                  new Amount(form.FormAmount, form.FormUnit)));
         }

         return pairings;
      }
   }
}