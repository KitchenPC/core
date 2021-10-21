using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KitchenPC.Core;
using KitchenPC.Core.Modeler;
using KitchenPC.Core.Recipes;

namespace KitchenPC.UnitTests.Mock;

internal class MockModelerDBLoader : IModelerLoader
{
    private readonly RecipeBinding[] _dsRecipes;
    private readonly IngredientBinding[] _dsIngredients;
    private readonly RatingBinding[] _dsRatings;

    public MockModelerDBLoader(string filename)
    {
        var codePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var codeDir = new FileInfo(codePath).Directory.FullName;
        var fullPath = Path.Combine(codeDir, filename);

        var doc = XDocument.Load(fullPath);

        _dsRecipes = (from r in doc.Descendants("Recipe")
            select new RecipeBinding()
            {
                Id = new Guid(r.Attribute("Id").Value),
                Rating = Convert.ToByte(r.Attribute("Rating").Value),
                Tags = RecipeTags.Parse(r.Attribute("Tags").Value)
            }).ToArray();

        _dsIngredients = (from u in doc.Descendants("Usage")
            select new IngredientBinding()
            {
                RecipeId = new Guid(u.Attribute("RecipeId").Value),
                IngredientId = new Guid(u.Attribute("IngredientId").Value),
                Qty = Convert.ToSingle(u.Attribute("Qty").Value),
                Unit = Unit.Parse<Units>(u.Attribute("Unit").Value)
            }).ToArray();

        _dsRatings = (from r in doc.Descendants("Rating")
            select new RatingBinding()
            {
                UserId = new Guid(r.Attribute("UserId").Value),
                RecipeId = new Guid(r.Attribute("RecipeId").Value),
                Rating = Convert.ToByte(r.Attribute("Rating").Value)
            }).ToArray();
    }

    public IEnumerable<RecipeBinding> LoadRecipeGraph() => _dsRecipes;

    public IEnumerable<IngredientBinding> LoadIngredientGraph() => _dsIngredients;

    public IEnumerable<RatingBinding> LoadRatingGraph() => _dsRatings;
}
