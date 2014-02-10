using System;

namespace KitchenPC.Ingredients
{
   public class Ingredient
   {
      public Guid Id;
      public String Name;
      public UnitType ConversionType;
      public String UnitName;
      public Weight UnitWeight;

      public IngredientMetadata Metadata;

      public static Ingredient FromId(Guid ingredientId)
      {
         return new Ingredient
         {
            Id = ingredientId
         };
      }

      public Ingredient(Guid id, String name)
      {
         Id = id;
         Name = name;
         Metadata = new IngredientMetadata();
      }

      public Ingredient(Guid id, String name, IngredientMetadata metadata)
      {
         Id = id;
         Name = name;
         Metadata = metadata;
      }

      public Ingredient() : this(Guid.Empty, String.Empty)
      {
      }

      public override string ToString()
      {
         return Name;
      }

      public override bool Equals(object obj)
      {
         var i = obj as Ingredient;
         return (i != null && this.Id == i.Id);
      }

      public override int GetHashCode()
      {
         return Id.GetHashCode();
      }
   }
}