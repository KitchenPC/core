using System.Collections.Generic;
using System.Linq;
using KitchenPC.Data;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC.Context
{
   public class StaticFormLoader : ISynonymLoader<FormNode>
   {
      readonly DataStore store;

      public StaticFormLoader(DataStore store)
      {
         this.store = store;
      }

      public IEnumerable<FormNode> LoadSynonyms()
      {
         var formSyn = store.NlpFormSynonyms
            .OrderBy(p => p.Name)
            .Select(s => s.Name)
            .Distinct()
            .ToList();

         return new List<FormNode>(formSyn.Select(s => new FormNode(s)));
      }

      public Pairings LoadFormPairings()
      {
         var forms = store.GetIndexedIngredientForms();
         var formSyn = store.NlpFormSynonyms;
         var pairings = new Pairings();

         foreach (var syn in formSyn)
         {
            var f = forms[syn.FormId];

            var name = syn.Name;
            var ing = syn.IngredientId;
            var form = f.IngredientFormId;
            var convType = f.UnitType;
            var displayName = f.FormDisplayName;
            var unitName = f.UnitName;
            int convMultiplier = f.ConvMultiplier;
            var formAmt = f.FormAmount;
            var formUnit = f.FormUnit;
            var amount = new Amount(formAmt, formUnit);

            pairings.Add(
               new NameIngredientPair(name, ing),
               new IngredientForm(form, ing, convType, displayName, unitName, convMultiplier, amount));
         }

         return pairings;
      }
   }
}