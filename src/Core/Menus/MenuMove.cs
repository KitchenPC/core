using System;

namespace KitchenPC.Menus
{
   public class MenuMove
   {
      public Guid? TargetMenu { get; set; }
      public Guid[] RecipesToMove { get; set; }
      public bool MoveAll { get; set; }
   }
}