using System.Collections.Generic;
using KitchenPC.Recipes;
using NUnit.Framework;

namespace KitchenPC.UnitTests
{
   [TestFixture]
   public class TagsTest
   {
      [Test]
      public void TestRecipeTag()
      {
         var t1 = RecipeTag.Dinner;
         var t2 = RecipeTag.Dinner;
         var t3 = RecipeTag.Lunch;
         RecipeTag t4 = null;
         RecipeTag t5 = null;

         Assert.IsTrue(t1 == t2);
         Assert.IsTrue(t1 != t3);
         Assert.IsTrue(t1 != null);
         Assert.IsTrue(t4 == null);
         Assert.IsTrue(t4 == t5);

         Assert.IsTrue(t1.Equals(t2));
      }

      [Test]
      public void TestRecipeTags()
      {
         var tags1 = RecipeTag.GlutenFree | RecipeTag.NoAnimals | RecipeTag.NoMeat | RecipeTag.NoPork;
         var tags2 = RecipeTag.LowCalorie | RecipeTag.LowFat;
         var tags3 = RecipeTag.Lunch | RecipeTag.GlutenFree | RecipeTag.Dinner;
         RecipeTags tags4 = 15; //Same as tags1
         RecipeTags tags5 = null;
         RecipeTags tags6 = null;

         //Test counts
         Assert.AreEqual(4, tags1.Length);
         Assert.AreEqual(2, tags2.Length);
         Assert.AreEqual(4, tags4.Length);

         //Test Bitwise operators
         Assert.IsTrue((tags1 & RecipeTag.GlutenFree) > 0); //Has GlutenFree tag
         Assert.IsTrue((tags1 & RecipeTag.NoAnimals) > 0); //Has NoAnimals tag
         Assert.IsTrue((tags1 & RecipeTag.NoMeat) > 0); //Has NoMeat tag
         Assert.IsTrue((tags1 & RecipeTag.NoPork) > 0); //Has NoPork tag

         Assert.IsTrue((tags1 & RecipeTag.LowSugar) == 0); //Does NOT have LowSugar tag
         Assert.IsTrue((tags1 & RecipeTag.LowCarb) == 0); //Does NOT have LowCarb tag

         Assert.IsFalse((tags1 & tags2) > 0); //No overlap
         Assert.IsTrue((tags1 & tags3) > 0); //Both share GlutenFree tag

         //Test comparison operators
         Assert.IsFalse(tags1 == tags2);
         Assert.IsTrue(tags1 == tags4);

         Assert.IsFalse(tags1 == null);
         Assert.IsFalse(tags1 == tags5);
         Assert.IsTrue(tags1 != null);
         Assert.IsTrue(tags5 == null);
         Assert.IsTrue(tags5 == tags6);

         //Test indexing
         Assert.IsTrue(tags1[0] == RecipeTag.GlutenFree);
         Assert.IsTrue(tags1[1] != RecipeTag.GlutenFree);

         //Test Iteration
         var tags = new List<RecipeTag>(2);
         tags.AddRange(tags2);

         Assert.AreEqual(2, tags.Count);
         Assert.AreEqual(tags[0], RecipeTag.LowCalorie);
         Assert.AreEqual(tags[1], RecipeTag.LowFat);

         //Test ToString
         Assert.AreEqual("Gluten Free, No Animals, No Meat, No Pork", tags1.ToString());
         Assert.AreEqual("Gluten Free No Animals No Meat No Pork", tags1.ToString(" "));

         //Test From
         Assert.AreEqual("No Pork", RecipeTags.From(RecipeTag.NoPork).ToString());
         Assert.AreEqual("No Pork, Lunch", RecipeTags.From(RecipeTag.NoPork, RecipeTag.Lunch).ToString());
      }

      [Test]
      public void TestParsing()
      {
         RecipeTag dinner = "Dinner";
         RecipeTag lunch = "Lunch";

         Assert.AreEqual(RecipeTag.Dinner, dinner);
         Assert.AreEqual(RecipeTag.Lunch, lunch);

         string strDinner = RecipeTag.Dinner;
         string strLunch = RecipeTag.Lunch;

         Assert.AreEqual("Dinner", strDinner);
         Assert.AreEqual("Lunch", strLunch);

         var tags = RecipeTags.Parse("No Red Meat, Dinner, Lunch");

         Assert.AreEqual(3, tags.Length);
         Assert.AreEqual("No Red Meat, Dinner, Lunch", tags.ToString());
      }
   }
}