namespace KitchenPC.NLP
{
   public class MatchData
   {
      public IngredientNode Ingredient;
      public UnitNode Unit;
      public FormNode Form;
      public Amount Amount;
      public Preps Preps;

      public MatchData()
      {
         Preps = new Preps();
      }
   }
}