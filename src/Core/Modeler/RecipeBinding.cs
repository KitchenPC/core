using System;
using KitchenPC.Core.Recipes;

namespace KitchenPC.Core.Modeler;

public struct RecipeBinding
{
   public Guid Id { get; set; }
   public Byte Rating { get; set; }
   public RecipeTags Tags { get; set; }
   public Boolean Hidden { get; set; }
}
