using System.Diagnostics;
using KitchenPC.NLP;
using NUnit.Framework;

namespace KitchenPC.UnitTests
{
   [TestFixture]
   internal class NlpTests
   {
      Parser parser;

      [TestFixtureSetUp]
      public void Setup()
      {
         //Initialize all the maps
         Trace.Write("Initializing NLP Grammar maps... ");

         IngredientSynonyms.InitIndex(new TestIngredientLoader());
         UnitSynonyms.InitIndex(new TestUnitLoader());
         FormSynonyms.InitIndex(new TestFormLoader());
         PrepNotes.InitIndex(new TestPrepLoader());
         NumericVocab.InitIndex();

         //Load parse templates
         parser = new Parser();
         Trace.Write("Loading parse templates... ");

         parser.LoadTemplates(
            "[ING]: [AMT] [UNIT]", //cheddar cheese: 5 cups
            "[AMT] [UNIT] [FORM] [ING]", //5 cups melted cheddar cheese
            "[AMT] [UNIT] [ING]", //5 cups cheddar cheese
            "[AMT] [UNIT] of [ING]", //5 cups of cheddar cheese
            "[AMT] [UNIT] of [FORM] [ING]", //two cups of shredded cheddar cheese
            "[AMT] [ING]", //5 eggs
            "[ING]: [AMT]", //eggs: 5
            "[FORM] [ING]: [AMT]", //shredded cheddar cheese: 1 cup
            "[FORM] [ING]: [AMT] [UNIT]", //shredded cheddar cheese: 1 cup

            "[ING]: [AMT] [UNIT], [PREP]", //cheddar cheese: 5 cups
            "[AMT] [UNIT] [FORM] [ING], [PREP]", //5 cups melted cheddar cheese
            "[AMT] [UNIT] [ING], [PREP]", //5 cups cheddar cheese
            "[AMT] [UNIT] of [ING], [PREP]", //5 cups of cheddar cheese
            "[AMT] [UNIT] of [FORM] [ING], [PREP]", //two cups of shredded cheddar cheese
            "[AMT] [ING], [PREP]", //5 eggs
            "[ING]: [AMT], [PREP]", //eggs: 5
            "[FORM] [ING]: [AMT], [PREP]", //shredded cheddar cheese: 1 cup
            "[FORM] [ING]: [AMT] [UNIT], [PREP]" //shredded cheddar cheese: 1 cup
            );
      }

      [Test]
      public void TestNLP()
      {
         //parser.Parse("shredded cheddar cheese: 5 cups");
         var result = parser.Parse("shredded cheddar cheese: 5 cups");

         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_CHEESE, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_CHEESE_SHREDDED, result.Usage.Form.FormId);
         Assert.AreEqual(5.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Cup, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //parser.Parse("1 cup melted cheddar cheese");
         result = parser.Parse("1 cup melted cheddar cheese");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_CHEESE, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_CHEESE_MELTED, result.Usage.Form.FormId);
         Assert.AreEqual(1.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Cup, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //parser.Parse("5 1/2 cups flour");
         result = parser.Parse("5 1/2 cups flour");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_FLOUR, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_FLOUR_VOLUME, result.Usage.Form.FormId);
         Assert.AreEqual(5.5f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Cup, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //parser.Parse("1 1/2 tsp of milk");
         result = parser.Parse("1 1/2 tsp of milk");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_MILK, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_MILK_VOLUME, result.Usage.Form.FormId);
         Assert.AreEqual(1.5f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Teaspoon, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //parser.Parse("six eggs");
         result = parser.Parse("six eggs");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_EGGS, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_EGG_UNIT, result.Usage.Form.FormId);
         Assert.AreEqual(6.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Unit, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //parser.Parse("an egg");
         result = parser.Parse("an egg");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_EGGS, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_EGG_UNIT, result.Usage.Form.FormId);
         Assert.AreEqual(1.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Unit, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //parser.Parse("a dozen ripe bananas");
         result = parser.Parse("a dozen ripe bananas");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_BANANAS, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_BANANA_UNIT, result.Usage.Form.FormId);
         Assert.AreEqual(12.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Unit, result.Usage.Amount.Unit);
         Assert.AreEqual("ripe", result.Usage.PrepNote);

         //parser.Parse("eggs: 3");
         result = parser.Parse("eggs: 3");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_EGGS, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_EGG_UNIT, result.Usage.Form.FormId);
         Assert.AreEqual(3.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Unit, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //parser.Parse("1 head of lettuce");
         result = parser.Parse("1 head of lettuce");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_LETTUCE, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_LETTUCE_HEAD, result.Usage.Form.FormId);
         Assert.AreEqual(1.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Unit, result.Usage.Amount.Unit);
         Assert.IsNull(result.Usage.PrepNote);

         //Test a few prep notes
         result = parser.Parse("1 head of lettuce, chopped");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_LETTUCE, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_LETTUCE_HEAD, result.Usage.Form.FormId);
         Assert.AreEqual(1.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Unit, result.Usage.Amount.Unit);
         Assert.AreEqual("chopped", result.Usage.PrepNote);

         result = parser.Parse("1 ripe banana, sliced");
         Assert.AreEqual(MatchResult.Match, result.Status);
         Assert.AreEqual(TestIngredientLoader.ING_BANANAS, result.Usage.Ingredient.Id);
         Assert.AreEqual(TestIngredientLoader.FORM_BANANA_UNIT, result.Usage.Form.FormId);
         Assert.AreEqual(1.0f, result.Usage.Amount.SizeHigh);
         Assert.AreEqual(Units.Unit, result.Usage.Amount.Unit);
         Assert.AreEqual("ripe//sliced", result.Usage.PrepNote);
      }
   }
}