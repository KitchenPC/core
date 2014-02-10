using System.Collections.Generic;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC.UnitTests
{
   internal class TestFormLoader : ISynonymLoader<FormNode>
   {
      public IEnumerable<FormNode> LoadSynonyms()
      {
         FormNode[] nodes =
         {
            new FormNode("melted"),
            new FormNode("shredded"),
            new FormNode("diced")
         };

         return nodes;
      }

      public Pairings LoadFormPairings()
      {
         //TODO: This will come from a database of pairs that maps every possible unit to a default form of an ingredient
         var pairings = new Pairings();
         pairings.Add(new NameIngredientPair("melted", TestIngredientLoader.ING_CHEESE), new IngredientForm(TestIngredientLoader.FORM_CHEESE_MELTED, TestIngredientLoader.ING_CHEESE, Units.Cup, "melted", "", 0, null));
         pairings.Add(new NameIngredientPair("shredded", TestIngredientLoader.ING_CHEESE), new IngredientForm(TestIngredientLoader.FORM_CHEESE_SHREDDED, TestIngredientLoader.ING_CHEESE, Units.Cup, "shredded", "", 0, null));
         pairings.Add(new NameIngredientPair("diced", TestIngredientLoader.ING_CHEESE), new IngredientForm(TestIngredientLoader.FORM_CHEESE_DICED, TestIngredientLoader.ING_CHEESE, Units.Cup, "diced", "", 0, null));

         return pairings;
      }
   }
}