using System.Collections.Generic;
using System.Linq;
using KitchenPC.Core;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.NLP;
using KitchenPC.DB.Models;
using NHibernate;

namespace KitchenPC.DB.NLP;

public class UnitLoader(DatabaseAdapter adapter) : ISynonymLoader<UnitNode>
{
    public IEnumerable<UnitNode> LoadSynonyms()
    {
        using var session = adapter.GetStatelessSession();
        //Load synonyms
        var unitSyn = session.Query<NlpUnitSynonyms>()
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .Distinct()
            .ToList();

        return new List<CustomUnitNode>(unitSyn.Select(s => new CustomUnitNode(s)));
    }

    public Pairings LoadFormPairings()
    {
        using var session = adapter.GetStatelessSession();

        //Load all form pairings from db
        var unitSyn = session.QueryOver<NlpUnitSynonyms>()
            .Fetch(SelectMode.Fetch, prop => prop.Form)
            .List();

        var pairings = new Pairings();
        foreach (var syn in unitSyn)
        {
            pairings.Add(new NameIngredientPair(syn.Name.Trim(), syn.Ingredient.IngredientId),
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