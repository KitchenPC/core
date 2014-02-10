using System.Collections.Generic;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC.UnitTests
{
   internal class TestUnitLoader : ISynonymLoader<UnitNode>
   {
      public IEnumerable<UnitNode> LoadSynonyms()
      {
         //Add some test unit types, this will eventually come from database
         UnitNode[] units =
         {
            new CustomUnitNode("head"), new CustomUnitNode("heads")
         };

         return units;
      }

      public Pairings LoadFormPairings()
      {
         //TODO: This will come from a database of pairs that maps every possible unit to a default form of an ingredient
         var pairings = new Pairings();
         var pair = new NameIngredientPair("head", TestIngredientLoader.ING_LETTUCE);
         var form = new IngredientForm(TestIngredientLoader.FORM_LETTUCE_HEAD, TestIngredientLoader.ING_LETTUCE, Units.Unit, null, "head/heads", 0, null);
         pairings.Add(pair, form);

         return pairings;
      }
   }
}