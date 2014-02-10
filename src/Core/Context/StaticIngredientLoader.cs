using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Data;
using KitchenPC.Data.DTO;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC.Context
{
   public class StaticIngredientLoader : ISynonymLoader<NLP.IngredientNode>
   {
      readonly DataStore store;

      public StaticIngredientLoader(DataStore store)
      {
         this.store = store;
      }

      public IEnumerable<NLP.IngredientNode> LoadSynonyms()
      {
         var nodes = new Dictionary<Guid, NLP.IngredientNode>();

         var forms = store.GetIndexedIngredientForms();
         var ingsForNlp = store.Ingredients;
         var pairingMap = store.NlpDefaultPairings.ToDictionary(p => p.IngredientId);

         foreach (var ing in ingsForNlp)
         {
            var ingId = ing.IngredientId;
            var name = ing.DisplayName;
            var convType = ing.ConversionType;
            Weight unitWeight = ing.UnitWeight;
            var pairings = new DefaultPairings();

            NlpDefaultPairings defaultPairing;
            if (pairingMap.TryGetValue(ingId, out defaultPairing))
            {
               if (defaultPairing.WeightFormId.HasValue)
               {
                  var wf = forms[defaultPairing.WeightFormId.Value];
                  var wfAmount = new Amount(wf.FormAmount, wf.FormUnit);
                  pairings.Weight = new IngredientForm(wf.IngredientFormId, ingId, Units.Ounce, null, null, wf.ConvMultiplier, wfAmount);
               }

               if (defaultPairing.VolumeFormId.HasValue)
               {
                  var vf = forms[defaultPairing.VolumeFormId.Value];
                  var vfAmount = new Amount(vf.FormAmount, vf.FormUnit);
                  pairings.Volume = new IngredientForm(vf.IngredientFormId, ingId, Units.Cup, null, null, vf.ConvMultiplier, vfAmount);
               }

               if (defaultPairing.UnitFormId.HasValue)
               {
                  var uf = forms[defaultPairing.UnitFormId.Value];
                  var ufAmount = new Amount(uf.FormAmount, uf.FormUnit);
                  pairings.Unit = new IngredientForm(uf.IngredientFormId, ingId, Units.Unit, null, null, uf.ConvMultiplier, ufAmount);
               }
            }

            if (nodes.ContainsKey(ingId))
            {
               Parser.Log.ErrorFormat("[NLP Loader] Duplicate ingredient key due to bad DB data: {0} ({1})", name, ingId);
            }
            else
            {
               nodes.Add(ingId, new NLP.IngredientNode(ingId, name, convType, unitWeight, pairings));
            }
         }

         //Load synonyms
         var ingSynonyms = store.NlpIngredientSynonyms;

         var ret = new List<NLP.IngredientNode>();
         foreach (var syn in ingSynonyms)
         {
            var ingId = syn.IngredientId;
            var alias = syn.Alias;
            var prepnote = syn.Prepnote;

            NLP.IngredientNode node;
            if (nodes.TryGetValue(ingId, out node)) //TODO: If this fails, maybe throw an exception?
            {
               ret.Add(new NLP.IngredientNode(node, alias, prepnote));
            }
         }

         ret.AddRange(nodes.Values);

         return ret;
      }

      public Pairings LoadFormPairings()
      {
         throw new NotImplementedException();
      }
   }
}