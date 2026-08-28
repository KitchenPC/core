using System.Linq;
using FluentNHibernate.Cfg;
using KitchenPC.DB.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KitchenPC.UnitTests;

[TestClass]
public class DatabaseMappings
{
   [TestMethod]
   public void IngredientTablesUseCanonicalNames()
   {
      var configuration = Fluently
         .Configure()
         .Mappings(mappings =>
            mappings.FluentMappings.Add<IngredientsMap>().Add<NlpDefaultPairingsMap>()
         )
         .BuildConfiguration();

      var ingredientTable = configuration.GetClassMapping(typeof(Ingredients)).Table.Name;
      var defaultPairingsTable = configuration
         .GetClassMapping(typeof(NlpDefaultPairings))
         .Table.Name;

      Assert.AreEqual("shoppingingredients", ingredientTable);
      Assert.AreEqual("shoppingingredientsfornlp", defaultPairingsTable);
      Assert.IsFalse(
         configuration.ClassMappings.Any(mapping => mapping.Table.Name == "ingredients"),
         "No persistence model should map to the legacy ingredients table."
      );
   }
}
