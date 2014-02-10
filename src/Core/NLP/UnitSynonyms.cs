using System;
using System.Collections.Generic;
using KitchenPC.Ingredients;

namespace KitchenPC.NLP
{
   public class UnitSynonyms : SynonymTree<UnitNode>
   {
      static readonly object MapInitLock = new object();
      static Pairings pairings;

      public static void InitIndex(ISynonymLoader<UnitNode> loader)
      {
         lock (MapInitLock)
         {
            index = new AlphaTree<UnitNode>();
            synonymMap = new Dictionary<string, UnitNode>();

            //Hard code intrinsic unit types
            UnitNode[] units =
            {
               new UnitNode("tsp", Units.Teaspoon), new UnitNode("t.", Units.Teaspoon), new UnitNode("t", Units.Teaspoon), new UnitNode("teaspoon", Units.Teaspoon), new UnitNode("teaspoons", Units.Teaspoon),
               new UnitNode("tbl", Units.Tablespoon), new UnitNode("tbsp", Units.Tablespoon), new UnitNode("tablespoon", Units.Tablespoon), new UnitNode("tablespoons", Units.Tablespoon),
               new UnitNode("fl oz", Units.FluidOunce), new UnitNode("fluid ounce", Units.FluidOunce), new UnitNode("fluid ounces", Units.FluidOunce),
               new UnitNode("cup", Units.Cup), new UnitNode("cups", Units.Cup), new UnitNode("c.", Units.Cup), new UnitNode("c", Units.Cup),
               new UnitNode("pt", Units.Pint), new UnitNode("pint", Units.Pint), new UnitNode("pints", Units.Pint),
               new UnitNode("qt", Units.Quart), new UnitNode("qts", Units.Quart), new UnitNode("quart", Units.Quart), new UnitNode("quarts", Units.Quart),
               new UnitNode("gal", Units.Gallon), new UnitNode("gallon", Units.Gallon), new UnitNode("gallons", Units.Gallon),
               new UnitNode("gram", Units.Gram), new UnitNode("g.", Units.Gram), new UnitNode("g", Units.Gram), new UnitNode("grams", Units.Gram),
               new UnitNode("oz", Units.Ounce), new UnitNode("ounce", Units.Ounce), new UnitNode("ounces", Units.Ounce),
               new UnitNode("lb", Units.Pound), new UnitNode("lbs", Units.Pound), new UnitNode("pound", Units.Pound), new UnitNode("pounds", Units.Pound)
            };

            foreach (var unit in units)
            {
               IndexString(unit.Name, unit);
            }

            //Load custom unit types through the loader
            foreach (var unit in loader.LoadSynonyms())
            {
               IndexString(unit.Name.Trim(), unit);
            }

            //Load default pair data to map unit names to certain ingredient forms
            pairings = loader.LoadFormPairings();
         }
      }

      public static bool TryGetFormForIngredient(string formname, Guid ing, out IngredientForm form)
      {
         form = null;
         UnitNode unit;
         if (false == synonymMap.TryGetValue(formname, out unit))
         {
            return false;
         }

         var pair = new NameIngredientPair(formname, ing);
         return pairings.TryGetValue(pair, out form);
      }
   }
}