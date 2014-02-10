using System;

namespace KitchenPC.NLP
{
   public class UnitNode
   {
      public string Name;
      public Units Unit;

      public UnitNode(string name, Units unit)
      {
         this.Name = name.Trim();
         this.Unit = unit;
      }

      public override string ToString()
      {
         if (String.IsNullOrEmpty(Name))
         {
            return Name;
         }
         else
         {
            return Unit.ToString();
         }
      }
   }
}