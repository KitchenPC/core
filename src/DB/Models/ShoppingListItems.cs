using System;
using FluentNHibernate.Mapping;
using KitchenPC.ShoppingLists;

namespace KitchenPC.DB.Models
{
   public class ShoppingListItems
   {
      public virtual Guid ItemId { get; set; }
      public virtual String Raw { get; set; }
      public virtual float? Qty { get; set; }
      public virtual Units? Unit { get; set; }
      public virtual Guid UserId { get; set; }
      public virtual Ingredients Ingredient { get; set; }
      public virtual Recipes Recipe { get; set; }
      public virtual ShoppingLists ShoppingList { get; set; }
      public virtual bool CrossedOut { get; set; }

      public virtual Amount Amount
      {
         get
         {
            return Qty.HasValue && Unit.HasValue ? new Amount(Qty.Value, Unit.Value) : null;
         }

         set
         {
            if (value == null || value.SizeHigh == 0)
            {
               Qty = null;
               Unit = null;
            }
            else
            {
               Qty = value.SizeHigh;
               Unit = value.Unit;
            }
         }
      }

      public ShoppingListItems()
      {
      }

      public ShoppingListItems(Guid id, Guid userid, String raw)
      {
         ItemId = id;
         UserId = userid;
         Raw = raw;
      }

      public ShoppingListItems(Guid id, Guid userid, Amount amt, Guid? ingredientId, Guid? recipeId)
      {
         ItemId = id;
         UserId = userid;
         Amount = amt;
         Ingredient = ingredientId.HasValue ? Ingredients.FromId(ingredientId.Value) : null;
         Recipe = recipeId.HasValue ? Recipes.FromId(recipeId.Value) : null;
      }

      public virtual ShoppingListItem AsShoppingListItem()
      {
         if (Ingredient != null)
         {
            return new ShoppingListItem(Ingredient.AsIngredient())
            {
               Id = ItemId,
               Amount = (Qty.HasValue && Unit.HasValue) ? new Amount(Qty.Value, Unit.Value) : null,
               Recipe = Recipe != null ? Recipe.AsRecipeBrief() : null,
               CrossedOut = CrossedOut
            };
         }

         return new ShoppingListItem(Raw)
         {
            Id = ItemId,
            CrossedOut = CrossedOut
         };
      }

      public static ShoppingListItems FromShoppingListItem(ShoppingListItem item)
      {
         return new ShoppingListItems
         {
            ItemId = item.Id.HasValue ? item.Id.Value : Guid.NewGuid(),
            Raw = item.Raw,
            Qty = item.Amount == null ? null : (float?) item.Amount.SizeHigh,
            Unit = item.Amount == null ? null : (Units?) item.Amount.Unit,
            Ingredient = item.Ingredient == null ? null : new Ingredients {IngredientId = item.Ingredient.Id},
            Recipe = item.Recipe == null ? null : new Recipes {RecipeId = item.Recipe.Id},
            CrossedOut = item.CrossedOut
         };
      }
   }

   public class ShoppingListItemsMap : ClassMap<ShoppingListItems>
   {
      public ShoppingListItemsMap()
      {
         Id(x => x.ItemId)
            .GeneratedBy.GuidComb()
            .UnsavedValue(Guid.Empty);

         Map(x => x.Raw).Length(50);
         Map(x => x.Qty);
         Map(x => x.Unit);
         Map(x => x.UserId).Not.Nullable().Index("IDX_ShoppingListItems_UserId");
         Map(x => x.CrossedOut).Not.Nullable();

         References(x => x.Recipe).Column("RecipeId");
         References(x => x.Ingredient).Column("IngredientId");
         References(x => x.ShoppingList).Column("ShoppingListId").Index("IDX_ShoppingListItems_ListId");
      }
   }
}