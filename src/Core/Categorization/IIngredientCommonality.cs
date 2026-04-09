using System;

namespace KitchenPC.Core.Categorization;

public interface IIngredientCommonality
{
   Guid IngredientId { get; }
   Single Commonality { get; }
}
