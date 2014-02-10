namespace KitchenPC.NLP
{
   public class AnomalousNode
   {
      public string Name;
      public AnomalousIngredientNode Ingredient;

      public AnomalousNode(string name, AnomalousIngredientNode ing)
      {
         Name = name;
         Ingredient = ing;
      }
   }
}