using System;

namespace KitchenPC.Categorization
{
   public class AnalyzerResult
   {
      public Category FirstPlace { get; private set; }
      public Category SecondPlace { get; private set; }

      public AnalyzerResult(Category first, Category second)
      {
         FirstPlace = first;
         SecondPlace = second;
      }

      public override string ToString()
      {
         return SecondPlace == Category.None ? FirstPlace.ToString()
            : String.Format("{0}/{1}", FirstPlace, SecondPlace);
      }
   }
}