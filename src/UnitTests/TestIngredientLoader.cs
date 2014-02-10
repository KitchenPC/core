using System;
using System.Collections.Generic;
using KitchenPC.Ingredients;
using KitchenPC.NLP;

namespace KitchenPC.UnitTests
{
   internal class TestIngredientLoader : ISynonymLoader<IngredientNode>
   {
      //All ingredients
      public static Guid ING_EGGS = new Guid("948aeda5-ffff-41bd-af4e-71d1c740db76");
      public static Guid ING_BANANAS = new Guid("d54654d8-5d2d-4bda-80ab-82f5d7f008bb");
      public static Guid ING_MILK = new Guid("5a698842-54a9-4ed2-b6c3-aea1bcd157cd");
      public static Guid ING_FLOUR = new Guid("daa8fbf6-3347-41b9-826d-078cd321402e");
      public static Guid ING_CHEESE = new Guid("5ee315ea-7ef6-4fa5-809a-dc9931a01ed1");
      public static Guid ING_LETTUCE = new Guid("55344339-8d1d-4892-a117-ec8018a5e483");

      //Forms for ingredients
      public static Guid FORM_EGG_UNIT = new Guid("821049b8-5789-44dd-a256-824efcb54dab");
      public static Guid FORM_BANANA_UNIT = new Guid("67039fa6-a6a3-4c7e-af97-97cade3f2d19");
      public static Guid FORM_MILK_VOLUME = new Guid("353a5c8f-6a6d-4c7d-a23f-c17805430260");
      public static Guid FORM_FLOUR_VOLUME = new Guid("9bf38fd7-3cc4-4eb5-b2e8-bc77f0978994");
      public static Guid FORM_FLOUR_WEIGHT = new Guid("f9629de4-e51b-45a2-94b2-3e51ccaa5b75");

      public static Guid FORM_LETTUCE_HEAD = new Guid("cc59d558-43ce-48bc-b2fa-ad501d845966");
      public static Guid FORM_CHEESE_MELTED = new Guid("0784f3a5-6f55-44f9-9c55-be5687ca97d8");
      public static Guid FORM_CHEESE_SHREDDED = new Guid("a64c5ccd-6805-4734-8ff8-a1bff6e2b83b");
      public static Guid FORM_CHEESE_DICED = new Guid("58efca93-7c5e-49b3-a04f-e97fe52e9a92");

      public IEnumerable<IngredientNode> LoadSynonyms()
      {
         var eggPairings = new DefaultPairings()
         {
            Unit = new IngredientForm(FORM_EGG_UNIT, ING_EGGS, Units.Unit, null, "egg/eggs", 0, null)
         };

         var bananaPairings = new DefaultPairings()
         {
            Unit = new IngredientForm(FORM_BANANA_UNIT, ING_BANANAS, Units.Unit, null, "banana/bananas", 0, null)
         };

         var milkPairings = new DefaultPairings()
         {
            Volume = new IngredientForm(FORM_MILK_VOLUME, ING_MILK, Units.Cup, null, null, 0, null)
         };

         var flourPairings = new DefaultPairings()
         {
            Volume = new IngredientForm(FORM_FLOUR_VOLUME, ING_FLOUR, Units.Cup, null, null, 0, null),
            Weight = new IngredientForm(FORM_FLOUR_WEIGHT, ING_FLOUR, Units.Ounce, null, null, 0, null)
         };

         var cheese = new IngredientNode(ING_CHEESE, "cheddar cheese", UnitType.Weight, 0, DefaultPairings.Empty);
         var eggs = new IngredientNode(ING_EGGS, "eggs", UnitType.Unit, 0, eggPairings);
         var bananas = new IngredientNode(ING_BANANAS, "bananas", UnitType.Weight, 0, bananaPairings);
         var milk = new IngredientNode(ING_MILK, "milk", UnitType.Volume, 0, milkPairings);
         var flour = new IngredientNode(ING_FLOUR, "all-purpose flour", UnitType.Weight, 0, flourPairings);
         var lettuce = new IngredientNode(ING_LETTUCE, "lettuce", UnitType.Weight, 0, DefaultPairings.Empty);

         //Add in some test ingredients, but this will eventually come from a massive Synonyms database
         //DB will first load ShoppingIngredients and create root nodes for all of those, with default form data, then will load IngredientSynonyms for all aliases
         //TODO: Maybe there is a way to have ingredient nodes contain a singular and plural description so we don't need aliases for all the singulars
         IngredientNode[] ings =
         {
            //Load root nodes for ingredients
            cheese,
            eggs,
            bananas,
            milk,
            flour,
            lettuce,

            //Load any aliases for any ingredients
            new IngredientNode(eggs, "egg", null),
            new IngredientNode(bananas, "banana", null),
            new IngredientNode(bananas, "ripe banana", "ripe"),
            new IngredientNode(bananas, "ripe bananas", "ripe"),
            new IngredientNode(milk, "2% milk", null),
            new IngredientNode(flour, "flour", null)
         };

         return ings;
      }

      public Pairings LoadFormPairings()
      {
         throw new NotImplementedException(); //This will never be called on this type of loader
      }
   }
}