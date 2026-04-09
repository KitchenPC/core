namespace KitchenPC.Core.Ingredients;

public class IngredientMetadata
{
   public bool? HasGluten { get; set; }
   public bool? HasMeat { get; set; }
   public bool? HasRedMeat { get; set; }
   public bool? HasPork { get; set; }
   public bool? HasAnimal { get; set; }
   public short Spicy { get; set; }
   public short Sweet { get; set; }
   public float? FatPerUnit { get; set; }
   public float? SugarPerUnit { get; set; }
   public float? CaloriesPerUnit { get; set; }
   public float? SodiumPerUnit { get; set; }
   public float? CarbsPerUnit { get; set; }

   public IngredientMetadata() { }

   public IngredientMetadata(
      bool? hasgluten,
      bool? hasmeat,
      bool? hasredmeat,
      bool? haspork,
      bool? hasanimal,
      byte spicy,
      byte sweet,
      float? fatperunit,
      float? sugarperunit,
      float? caloriesperunit,
      float? sodiumperunit,
      float? carbsperunit
   )
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
