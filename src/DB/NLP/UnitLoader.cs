using System.Collections.Generic;
using System.Linq;
using KitchenPC.DB.Models;
using KitchenPC.Ingredients;
using KitchenPC.NLP;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace KitchenPC.DB
{
   public class UnitLoader : ISynonymLoader<UnitNode>
   {
      readonly DatabaseAdapter adapter;

      public UnitLoader(DatabaseAdapter adapter)
      {
         this.adapter = adapter;
      }

      public IEnumerable<UnitNode> LoadSynonyms()
      {
         using (var session = adapter.GetStatelessSession())
         {
            //Load synonyms
            var unitSyn = session.Query<NlpUnitSynonyms>()
               .OrderBy(p => p.Name)
               .Select(p => p.Name)
               .Distinct()
               .ToList();

            return new List<CustomUnitNode>(unitSyn.Select(s => new CustomUnitNode(s)));
         }
      }

      public Pairings LoadFormPairings()
      {
         using (var session = adapter.GetStatelessSession())
         {
            //Load all form pairings from db
            var unitSyn = session.QueryOver<NlpUnitSynonyms>()
               .Fetch(prop => prop.Form).Eager()
               .List();

            var pairings = new Pairings();
            foreach (var syn in unitSyn)
            {
               pairings.Add(new NameIngredientPair(
                  syn.Name.Trim(),
                  syn.Ingredient.IngredientId),
                  new IngredientForm(
                     syn.Form.IngredientFormId,
                     syn.Ingredient.IngredientId,
                     syn.Form.UnitType,
                     syn.Form.FormDisplayName,
                     syn.Form.UnitName,
                     syn.Form.ConvMultiplier,
                     new Amount(syn.Form.FormAmount, syn.Form.FormUnit)));
            }

            return pairings;
         }
      }
   }
}