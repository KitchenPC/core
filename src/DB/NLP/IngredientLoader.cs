using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.DB.Models;
using KitchenPC.Ingredients;
using KitchenPC.NLP;
using NHibernate.Criterion;

namespace KitchenPC.DB
{
   public class IngredientLoader : ISynonymLoader<IngredientNode>
   {
      readonly DatabaseAdapter adapter;

      public IngredientLoader(DatabaseAdapter adapter)
      {
         this.adapter = adapter;
      }

      public IEnumerable<IngredientNode> LoadSynonyms()
      {
         using (var session = adapter.GetStatelessSession())
         {
            var nodes = new Dictionary<Guid, IngredientNode>();

            var ingsForNlp = session.QueryOver<Models.Ingredients>().List();
            var pairingMap = session.QueryOver<NlpDefaultPairings>()
               .Fetch(prop => prop.WeightForm).Eager()
               .Fetch(prop => prop.VolumeForm).Eager()
               .Fetch(prop => prop.UnitForm).Eager()
               .List()
               .ToDictionary(p => p.Ingredient.IngredientId);

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
                  if (defaultPairing.WeightForm != null)
                  {
                     var wfAmount = new Amount(defaultPairing.WeightForm.FormAmount, defaultPairing.WeightForm.FormUnit);
                     pairings.Weight = new IngredientForm(defaultPairing.WeightForm.IngredientFormId, ingId, Units.Ounce, null, null, defaultPairing.WeightForm.ConvMultiplier, wfAmount);
                  }

                  if (defaultPairing.VolumeForm != null)
                  {
                     var vfAmount = new Amount(defaultPairing.VolumeForm.FormAmount, defaultPairing.VolumeForm.FormUnit);
                     pairings.Volume = new IngredientForm(defaultPairing.VolumeForm.IngredientFormId, ingId, Units.Cup, null, null, defaultPairing.VolumeForm.ConvMultiplier, vfAmount);
                  }

                  if (defaultPairing.UnitForm != null)
                  {
                     var ufAmount = new Amount(defaultPairing.UnitForm.FormAmount, defaultPairing.UnitForm.FormUnit);
                     pairings.Unit = new IngredientForm(defaultPairing.UnitForm.IngredientFormId, ingId, Units.Unit, null, null, defaultPairing.UnitForm.ConvMultiplier, ufAmount);
                  }
               }

               if (nodes.ContainsKey(ingId))
               {
                  Parser.Log.ErrorFormat("[NLP Loader] Duplicate ingredient key due to bad DB data: {0} ({1})", name, ingId);
               }
               else
               {
                  nodes.Add(ingId, new IngredientNode(ingId, name, convType, unitWeight, pairings));
               }
            }

            //Load synonyms
            var ingSynonyms = session.QueryOver<NlpIngredientSynonyms>().List();

            var ret = new List<IngredientNode>();
            foreach (var syn in ingSynonyms)
            {
               var ingId = syn.Ingredient.IngredientId;
               var alias = syn.Alias;
               var prepnote = syn.Prepnote;

               IngredientNode node;
               if (nodes.TryGetValue(ingId, out node)) //TODO: If this fails, maybe throw an exception?
               {
                  ret.Add(new IngredientNode(node, alias, prepnote));
               }
            }

            ret.AddRange(nodes.Values);

            return ret;
         }
      }

      public Pairings LoadFormPairings()
      {
         throw new NotImplementedException();
      }
   }
}