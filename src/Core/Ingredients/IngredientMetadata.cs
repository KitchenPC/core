namespace KitchenPC.Ingredients
{
   public class IngredientMetadata
   {
      public bool? HasGluten;
      public bool? HasMeat;
      public bool? HasRedMeat;
      public bool? HasPork;
      public bool? HasAnimal;
      public short Spicy;
      public short Sweet;
      public float? FatPerUnit;
      public float? SugarPerUnit;
      public float? CaloriesPerUnit;
      public float? SodiumPerUnit;
      public float? CarbsPerUnit;

      public IngredientMetadata()
      {
      }

      public IngredientMetadata(bool? hasgluten, bool? hasmeat, bool? hasredmeat, bool? haspork, bool? hasanimal, byte spicy, byte sweet, float? fatperunit, float? sugarperunit, float? caloriesperunit, float? sodiumperunit, float? carbsperunit)
      {
         HasGluten = hasgluten;
         HasMeat = hasmeat;
         HasRedMeat = hasredmeat;
         HasPork = haspork;
         HasAnimal = hasanimal;
         Spicy = spicy;
         Sweet = sweet;
         FatPerUnit = fatperunit;
         SugarPerUnit = sugarperunit;
         CaloriesPerUnit = caloriesperunit;
         SodiumPerUnit = sodiumperunit;
         CarbsPerUnit = carbsperunit;
      }
   }
}