using NUnit.Framework;

namespace KitchenPC.UnitTests
{
   [TestFixture]
   public class UnitsTest
   {
      [Test]
      public void TestFractions()
      {
         Assert.AreEqual("10 1/2", Fractions.FromDecimal(10.5m));
         Assert.AreEqual("1 1/3", Fractions.FromDecimal(1.333m));
         Assert.AreEqual("1/4", Fractions.FromDecimal(.25m));
         Assert.AreEqual("1/3", Fractions.FromDecimal(.3334m));
         Assert.AreEqual("3", Fractions.FromDecimal(3m));
         Assert.AreEqual("0", Fractions.FromDecimal(0m));
         Assert.AreEqual("0.1", Fractions.FromDecimal(.123m));

         Assert.AreEqual(1, Fractions.ParseFraction("1.0"));
         Assert.AreEqual(1, Fractions.ParseFraction("1"));
         Assert.AreEqual(.5, Fractions.ParseFraction("1/2"));
         Assert.AreEqual(1.5, Fractions.ParseFraction("1 1/2"));
         Assert.AreEqual(1, Fractions.ParseFraction("2/2"));
         Assert.AreEqual(1m/3, Fractions.ParseFraction("1/3"));
         Assert.AreEqual(10.5, Fractions.ParseFraction("10 50/100"));
      }

      [Test]
      public void TestAmounts()
      {
         var amt1 = new Amount(0.5f, 1f, Units.Teaspoon);
         var amt2 = new Amount(0.75f, Units.Pound);

         //Test equals
         Assert.AreEqual(new Amount(0.5f, 1f, Units.Teaspoon), amt1);
         Assert.AreEqual(new Amount(0.75f, Units.Pound), amt2);

         //Test multiplication
         Assert.AreEqual(new Amount(1f, 2f, Units.Teaspoon), amt1*2);
         Assert.AreEqual(new Amount(1.5f, Units.Pound), amt2*2);

         //Test addition
         Assert.AreEqual(new Amount(2.5f, 3f, Units.Teaspoon), amt1 + 2);
         Assert.AreEqual(new Amount(1f, Units.Pound), amt2 + 0.25f);

         Assert.AreEqual(new Amount(1.5f, 4f, Units.Teaspoon), amt1 + new Amount(1f, 3f, Units.Teaspoon));
         Assert.AreEqual(new Amount(1.5f, 2f, Units.Teaspoon), amt1 + new Amount(1f, Units.Teaspoon));
         Assert.AreEqual(new Amount(1f, Units.Pound), amt2 + new Amount(.25f, Units.Pound));

         //Cross unit addition
         Assert.AreEqual(new Amount(1.5f, 4f, Units.Teaspoon), amt1 + new Amount(.5f, 1f, Units.Tablespoon));
         // Assert.AreEqual(new Amount(3.5f, 4f, Units.Teaspoon), amt1 + new Amount(1f, Units.Tablespoon)); //Adding 3tsp
         Assert.AreEqual(new Amount(1f, Units.Pound), amt2 + new Amount(4f, Units.Ounce));

         //Test division
         Assert.AreEqual(new Amount(.25f, .5f, Units.Teaspoon), amt1/2);
         Assert.AreEqual(new Amount(.25f, Units.Pound), amt2/3);

         //Test subtraction
         Assert.AreEqual(new Amount(.25f, .75f, Units.Teaspoon), amt1 - .25f);
         Assert.AreEqual(new Amount(.25f, Units.Pound), amt2 - 0.50f);
      }
   }
}