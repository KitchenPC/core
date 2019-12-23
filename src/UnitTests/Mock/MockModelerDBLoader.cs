using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KitchenPC;
using KitchenPC.Modeler;
using KitchenPC.Recipes;

namespace KPCServer.UnitTests
{
    internal class MockModelerDBLoader : IModelerLoader
    {
        readonly RecipeBinding[] _dsRecipes;
        readonly IngredientBinding[] _dsIngredients;
        readonly RatingBinding[] _dsRatings;

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

        public IEnumerable<RecipeBinding> LoadRecipeGraph()
        {
            return _dsRecipes;
        }

        public IEnumerable<IngredientBinding> LoadIngredientGraph()
        {
            return _dsIngredients;
        }

        public IEnumerable<RatingBinding> LoadRatingGraph()
        {
            return _dsRatings;
        }
    }
}