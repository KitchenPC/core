using System;

namespace KitchenPC.Core.Ingredients;

public class IngredientForm
{
    public Guid FormId { get; set; }
    public Guid IngredientId { get; set; }
    public Units FormUnitType { get; set; }
    public string FormDisplayName { get; set; }
    public string FormUnitName { get; set; }
    public int ConversionMultiplier { get; set; }
    public Amount FormAmount { get; set; }

    public static IngredientForm FromId(Guid id)
    {
        return new IngredientForm
        {
            FormId = id
        };
    }

    public IngredientForm()
    {
    }

    public IngredientForm(Guid formid, Guid ingredientid, Units unittype, string displayname, string unitname, int convmultiplier, Amount amount)
    {
        FormId = formid;
        IngredientId = ingredientid;
        FormUnitType = unittype;
        FormDisplayName = displayname;
        FormUnitName = unitname;
        ConversionMultiplier = convmultiplier;
        FormAmount = amount;
    }

    public override string ToString()
    {
        return FormId.ToString();
    }
}
