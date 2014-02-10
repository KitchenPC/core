using System;
using KitchenPC.Menus;

namespace KitchenPC.Data.DTO
{
   public class Menus
   {
      public Guid MenuId { get; set; }
      public Guid UserId { get; set; }
      public String Title { get; set; }
      public DateTime CreatedDate { get; set; }

      public static Menu ToMenu(Menus dtoMenu)
      {
         return new Menu
         {
            Id = dtoMenu.MenuId,
            Title = dtoMenu.Title
         };
      }
   }
}