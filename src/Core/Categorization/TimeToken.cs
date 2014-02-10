namespace KitchenPC.Categorization
{
   internal class TimeToken : IToken
   {
      enum Classification
      {
         Quick,
         Medium,
         Long,
         SuperLong
      };

      readonly Classification classification;

      public TimeToken(int minutes)
      {
         if (minutes < 10) classification = Classification.Quick;
         else if (minutes < 30) classification = Classification.Medium;
         else if (minutes <= 60) classification = Classification.Long;
         else classification = Classification.SuperLong;
      }

      public override bool Equals(object obj)
      {
         var t1 = obj as TimeToken;
         return (t1 != null && t1.classification.Equals(classification));
      }

      public override int GetHashCode()
      {
         return classification.GetHashCode();
      }

      public override string ToString()
      {
         return classification.ToString();
      }
   }
}