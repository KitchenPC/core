using System;

namespace KitchenPC.Core.Menus;

public class MenuResult
{
   public bool MenuCreated { get; set; }
   public bool MenuUpdated { get; set; }

   public Guid? NewMenuId { get; set; }
}
