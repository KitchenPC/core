using KitchenPC.Ingredients;
using NUnit.Framework;

namespace KitchenPC.UnitTests
{
   [TestFixture]
   internal class UnitConverterTest
   {
      [Test]
      public void TestUnitConverter()
      {
         //CanConvert
         Assert.AreEqual(false, UnitConverter.CanConvert(Units.Unit, Units.Cup));
         Assert.AreEqual(false, UnitConverter.CanConvert(Units.Pound, Units.Cup));
         Assert.AreEqual(true, UnitConverter.CanConvert(Units.Teaspoon, Units.Tablespoon));
         Assert.AreEqual(true, UnitConverter.CanConvert(Units.Gram, Units.Ounce));

         //Convert
         Assert.AreEqual(16.0f, UnitConverter.Convert(2.0f, Units.Cup, Units.FluidOunce));
         Assert.AreEqual(32.0f, UnitConverter.Convert(2.0f, Units.Pound, Units.Ounce));
      }

      [Test]
      public void TestNormalizeAmount()
      {
         var amt1 = new Amount(32, Units.Ounce); //Should be 2lbs
         var amt2 = new Amount(9, Units.Teaspoon); //Should be 3 Tablespoons
         var amt3 = new Amount(16, Units.FluidOunce); //Should be 2 cups
         var amt4 = new Amount(2, Units.Pint); //Should be 1 qt
         var amt5 = new Amount(16, Units.Quart); //Should be 4gal

         Assert.AreEqual(new Amount(2, Units.Pound), Amount.Normalize(amt1, 1));
         Assert.AreEqual(new Amount(3, Units.Tablespoon), Amount.Normalize(amt2, 1));
         Assert.AreEqual(new Amount(2, Units.Cup), Amount.Normalize(amt3, 1));
         Assert.AreEqual(new Amount(1, Units.Quart), Amount.Normalize(amt4, 1));
         Assert.AreEqual(new Amount(4, Units.Gallon), Amount.Normalize(amt5, 1));
      }

      [Test]
      public void TestUnitRangeConverter()
      {
         var amt1 = new Amount(1f, 2f, Units.Cup); //8 - 16 fl oz
         var amt2 = new Amount(8f, 12f, Units.Ounce); //0.5 - 0.75 lb
         var amt3 = new Amount(0.5f, 2f, Units.Pint); //.25 - 1qt

         Assert.AreEqual(new Amount(8f, 16f, Units.FluidOunce), UnitConverter.Convert(amt1, Units.FluidOunce));
         Assert.AreEqual(new Amount(0.5f, 0.75f, Units.Pound), UnitConverter.Convert(amt2, Units.Pound));
         Assert.AreEqual(new Amount(.25f, 1f, Units.Quart), UnitConverter.Convert(amt3, Units.Quart));
      }

      [Test]
      public void TestFormConverter()
      {
         //Form conversions (Unit ingredients)
         var unitIng = new Ingredient() {ConversionType = UnitType.Unit, UnitWeight = 200}; //Ingredient sold by units (unit weighs 200g)
         var unitIng_UnitForm = new IngredientForm() {FormAmount = new Amount(50, Units.Gram), ConversionMultiplier = 1, FormUnitType = Units.Unit}; //Form expressed in units (unit in this form weighs 50g)
         var unitIng_WeightForm = new IngredientForm() {ConversionMultiplier = 1, FormUnitType = Units.Ounce}; //Form expressed by weight
         var unitIng_VolForm = new IngredientForm() {FormUnitType = Units.Cup, ConversionMultiplier = 1, FormAmount = new Amount(20, Units.Gram)}; //Each cup weighs 20g)

         var unitIng_UnitUsage = new IngredientUsage() {Amount = new Amount(4, Units.Unit), Form = unitIng_UnitForm, Ingredient = unitIng};
         var unitIng_WeightUsage = new IngredientUsage() {Amount = new Amount(300, Units.Gram), Form = unitIng_WeightForm, Ingredient = unitIng};
         var unitIng_VolUsage = new IngredientUsage() {Amount = new Amount(160, Units.Tablespoon), Form = unitIng_VolForm, Ingredient = unitIng}; //10 cups

         var unitIng_UnitAmt = FormConversion.GetNativeAmountForUsage(unitIng, unitIng_UnitUsage);
         var unitIng_WeightAmt = FormConversion.GetNativeAmountForUsage(unitIng, unitIng_WeightUsage);
         var unitIng_VolAmt = FormConversion.GetNativeAmountForUsage(unitIng, unitIng_VolUsage);

         Assert.AreEqual(1.0f, unitIng_UnitAmt.SizeHigh); //4 units in this form should convert to 1 unit of ingredient
         Assert.AreEqual(Units.Unit, unitIng_UnitAmt.Unit);

         Assert.AreEqual(2.0f, unitIng_WeightAmt.SizeHigh); //300g of this form should convert to 1.5 units of ingredient, however we round up to whole units
         Assert.AreEqual(Units.Unit, unitIng_WeightAmt.Unit);

         //TODO: Fix
         //Assert.AreEqual(1.0f, unitIng_VolAmt.SizeHigh); //10 cups of this form should convert to 1 unit of ingredient
         //Assert.AreEqual(Units.Unit, unitIng_VolAmt.Unit);

         //Form conversions (Volume ingredients)
         var volIng = new Ingredient() {ConversionType = UnitType.Volume}; //Ingredient sold by volume
         var volIng_UnitForm = new IngredientForm() {FormUnitType = Units.Unit, ConversionMultiplier = 1, FormAmount = new Amount(5, Units.Teaspoon)};
         var volIng_WeightForm = new IngredientForm() {FormUnitType = Units.Ounce, ConversionMultiplier = 1, FormAmount = new Amount(2, Units.Teaspoon)};

         var volIng_UnitUsage = new IngredientUsage() {Amount = new Amount(2, Units.Unit), Form = volIng_UnitForm, Ingredient = volIng};
         var volIng_WeightUsage = new IngredientUsage() {Amount = new Amount(0.25f, Units.Pound), Form = volIng_WeightForm, Ingredient = volIng}; //4oz

         var volIng_UnitAmt = FormConversion.GetNativeAmountForUsage(volIng, volIng_UnitUsage);
         var volIng_WeightAmt = FormConversion.GetNativeAmountForUsage(volIng, volIng_WeightUsage);

         Assert.AreEqual(10.0f, volIng_UnitAmt.SizeHigh);
         Assert.AreEqual(Units.Teaspoon, volIng_UnitAmt.Unit);

         Assert.AreEqual(8.0f, volIng_WeightAmt.SizeHigh);
         Assert.AreEqual(Units.Teaspoon, volIng_WeightAmt.Unit);

         //Form conversions (Weight ingredients)
         var weightIng = new Ingredient() {ConversionType = UnitType.Weight}; //Ingredient sold by weight
         var weightIng_UnitForm = new IngredientForm() {ConversionMultiplier = 1, FormUnitType = Units.Unit, FormAmount = new Amount(100, Units.Gram)};
         var weightIng_VolForm = new IngredientForm() {ConversionMultiplier = 1, FormUnitType = Units.Cup, FormAmount = new Amount(50, Units.Gram)};

         var weightIng_UnitUsage = new IngredientUsage() {Amount = new Amount(5, Units.Unit), Form = weightIng_UnitForm, Ingredient = weightIng};
         var weightIng_VolUsage = new IngredientUsage() {Amount = new Amount(144, Units.Teaspoon), Form = weightIng_VolForm, Ingredient = weightIng}; //3 cups

         var weightIng_UnitAmt = FormConversion.GetNativeAmountForUsage(weightIng, weightIng_UnitUsage);
         var weightIng_VolAmt = FormConversion.GetNativeAmountForUsage(weightIng, weightIng_VolUsage);

         Assert.AreEqual(500.0f, weightIng_UnitAmt.SizeHigh);
         Assert.AreEqual(Units.Gram, weightIng_UnitAmt.Unit);

         Assert.AreEqual(150.0f, weightIng_VolAmt.SizeHigh);
         Assert.AreEqual(Units.Gram, weightIng_VolAmt.Unit);
      }
   }
}