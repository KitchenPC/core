using System.Collections.Generic;
using System.Linq;
using KitchenPC.DB.Models;
using KitchenPC.Ingredients;
using KitchenPC.NLP;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace KitchenPC.DB
{
   public class FormLoader : ISynonymLoader<FormNode>
   {
      readonly DatabaseAdapter adapter;

      public FormLoader(DatabaseAdapter adapter)
      {
         this.adapter = adapter;
      }

      public IEnumerable<FormNode> LoadSynonyms()
      {
         using (var session = adapter.GetStatelessSession())
         {
            //Load synonyms
            var formSyn = session.Query<NlpFormSynonyms>()
               .OrderBy(p => p.Name)
               .Select(s => s.Name)
               .Distinct()
               .ToList();

            return new List<FormNode>(formSyn.Select(s => new FormNode(s)));
         }
      }

      public Pairings LoadFormPairings()
      {
         using (var session = adapter.GetStatelessSession())
         {
            var formSyn = session.QueryOver<NlpFormSynonyms>()
               .Fetch(prop => prop.Form).Eager()
               .List();

            //Load all form pairings from db
            var pairings = new Pairings();

            foreach (var syn in formSyn)
            {
               var name = syn.Name;
               var ing = syn.Ingredient.IngredientId;
               var form = syn.Form.IngredientFormId;
               var convType = syn.Form.UnitType;
               var displayName = syn.Form.FormDisplayName;
               var unitName = syn.Form.UnitName;
               int convMultiplier = syn.Form.ConvMultiplier;
               var formAmt = syn.Form.FormAmount;
               var formUnit = syn.Form.FormUnit;
               var amount = new Amount(formAmt, formUnit);

               pairings.Add(
                  new NameIngredientPair(name, ing),
                  new IngredientForm(form, ing, convType, displayName, unitName, convMultiplier, amount));
            }

            return pairings;
         }
      }
   }
}