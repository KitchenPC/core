using System;

namespace KitchenPC.Core.Context;

public class IngredientSource
{
   public Guid Id { get; private set; }
   public String DisplayName { get; private set; }

   public IngredientSource(Guid id, String displayname)
   {
      this.Id = id;
      this.DisplayName = displayname;
   }
}
