using System.Collections.Generic;
using KitchenPC.Core;
using KitchenPC.Core.Ingredients;
using KitchenPC.Core.NLP;

namespace KitchenPC.UnitTests;

internal class TestUnitLoader : ISynonymLoader<UnitNode>
{
   //Add some test unit types, this will eventually come from database
   public IEnumerable<UnitNode> LoadSynonyms() =>
      new UnitNode[] { new CustomUnitNode("head"), new CustomUnitNode("heads") };

   public Pairings LoadFormPairings()
   {
      //TODO: This will come from a database of pairs that maps every possible unit to a default form of an ingredient
      var pairings = new Pairings();
      var pair = new NameIngredientPair("head", TestIngredientLoader.ING_LETTUCE);
      var form = new IngredientForm(
         TestIngredientLoader.FORM_LETTUCE_HEAD,
         TestIngredientLoader.ING_LETTUCE,
         Units.Unit,
         null,
         "head/heads",
         0,
         null
      );
      pairings.Add(pair, form);

      return pairings;
   }
}
