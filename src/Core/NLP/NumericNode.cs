namespace KitchenPC.NLP
{
   public class NumericNode
   {
      public string Token;
      public float Value;

      public NumericNode(string token, float value)
      {
         Token = token;
         Value = value;
      }
   }
}