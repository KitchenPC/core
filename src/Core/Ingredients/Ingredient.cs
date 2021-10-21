using System;

namespace KitchenPC.Core.Ingredients;

public class Ingredient
{
    public Guid Id { get; set; }
    public String Name { get; set; }
    public UnitType ConversionType { get; set; }
    public String UnitName { get; set; }
    public Weight UnitWeight { get; set; }

    public IngredientMetadata Metadata { get; set; }

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