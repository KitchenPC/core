using System;
using KitchenPC.Core;
using KitchenPC.Core.Modeler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KitchenPC.UnitTests;

[TestClass]
public class IngredientBindingTest
{
   [TestMethod]
   public void MissingFormPreservesDirectlyConvertibleIngredient()
   {
      var binding = IngredientBinding.Create(
         Guid.NewGuid(),
         Guid.NewGuid(),
         1,
         Units.Pound,
         UnitType.Weight,
         0,
         null,
         null,
         null
      );

      Assert.AreEqual(16f, binding.Qty);
      Assert.AreEqual(Units.Ounce, binding.Unit);
   }

   [TestMethod]
   public void MissingFormPreservesIngredientWithUnknownAmount()
   {
      var ingredientId = Guid.NewGuid();
      var recipeId = Guid.NewGuid();

      var binding = IngredientBinding.Create(
         ingredientId,
         recipeId,
         1,
         Units.Cup,
         UnitType.Unit,
         0,
         null,
         null,
         null
      );

      Assert.AreEqual(ingredientId, binding.IngredientId);
      Assert.AreEqual(recipeId, binding.RecipeId);
      Assert.IsNull(binding.Qty);
      Assert.AreEqual(Units.Unit, binding.Unit);
   }
}
