using System;
using System.Collections.Generic;
using KitchenPC.Data;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC.Context
{
   public class StaticAnomalyLoader : ISynonymLoader<AnomalousNode>
   {
      readonly DataStore store;

      public StaticAnomalyLoader(DataStore store)
      {
         this.store = store;
      }

      public IEnumerable<AnomalousNode> LoadSynonyms()
      {
         var forms = store.GetIndexedIngredientForms();
         var ingredients = store.GetIndexedIngredients();
         var anomalies = store.NlpAnomalousIngredients;

         var ret = new List<AnomalousNode>();

         foreach (var anon in anomalies)
         {
            var ingredient = ingredients[anon.IngredientId];

            var name = anon.Name;
            var ing = anon.IngredientId;
            var ingName = ingredient.DisplayName;

            IngredientForm weightForm = null, volumeForm = null, unitForm = null;
            if (anon.WeightFormId.HasValue)
            {
               var wf = forms[anon.WeightFormId.Value];

               weightForm = new IngredientForm(
                  wf.IngredientFormId,
                  ing,
                  wf.UnitType,
                  wf.FormDisplayName,
                  wf.UnitName,
                  wf.ConvMultiplier,
                  new Amount(wf.FormAmount, wf.FormUnit));
            }

            if (anon.VolumeFormId.HasValue)
            {
               var vf = forms[anon.VolumeFormId.Value];

               volumeForm = new IngredientForm(
                  vf.IngredientFormId,
                  ing,
                  vf.UnitType,
                  vf.FormDisplayName,
                  vf.UnitName,
                  vf.ConvMultiplier,
                  new Amount(vf.FormAmount, vf.FormUnit));
            }

            if (anon.UnitFormId.HasValue)
            {
               var uf = forms[anon.UnitFormId.Value];

               unitForm = new IngredientForm(
                  uf.IngredientFormId,
                  ing,
                  uf.UnitType,
                  uf.FormDisplayName,
                  uf.UnitName,
                  uf.ConvMultiplier,
                  new Amount(uf.FormAmount, uf.FormUnit));
            }

            var pairings = new DefaultPairings() {Weight = weightForm, Volume = volumeForm, Unit = unitForm};
            var ingNode = new AnomalousIngredientNode(ing, ingName, UnitType.Unit, 0, pairings); //TODO: Must load conv type and unit weight
            ret.Add(new AnomalousNode(name, ingNode));
         }

         return ret;
      }

      public Pairings LoadFormPairings()
      {
         throw new NotImplementedException();
      }
   }
}