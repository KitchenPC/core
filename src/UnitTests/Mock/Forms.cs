using System;
using KitchenPC.Ingredients;

namespace KitchenPC.UnitTests.Mock
{
   internal static class Forms
   {
      // Salt
      public static IngredientForm SALT_WEIGHT = new IngredientForm(new Guid("ba80edaf-5708-4038-9364-2ed6d253f304"), new Guid("a6b0179f-6bf5-4ec4-b12b-a95f2f94fe91"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));
      public static IngredientForm SALT_VOLUME = new IngredientForm(new Guid("e99ced2d-a915-4950-8b28-4bb34d045973"), new Guid("a6b0179f-6bf5-4ec4-b12b-a95f2f94fe91"), Units.Cup, "", "", 1, new Amount(292, Units.Gram));
      public static IngredientForm SALT_DASH = new IngredientForm(new Guid("52fd5953-8ae8-4874-9d7c-e4a609071099"), new Guid("a6b0179f-6bf5-4ec4-b12b-a95f2f94fe91"), Units.Unit, "dash", "dash/dashes", 1, new Amount(1, Units.Gram));

      // Granulated Sugar
      public static IngredientForm GRANULATED_SUGAR_WEIGHT = new IngredientForm(new Guid("f3eacfc5-d0c8-4f46-89cc-4a26791d69e5"), new Guid("4de12110-87b1-4198-a9a9-2b32e45df0f0"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));
      public static IngredientForm GRANULATED_SUGAR_VOLUME = new IngredientForm(new Guid("4f3c3506-50ab-4751-85a4-74a6e52619da"), new Guid("4de12110-87b1-4198-a9a9-2b32e45df0f0"), Units.Cup, "", "", 1, new Amount(200, Units.Gram));
      public static IngredientForm GRANULATED_SUGAR_DASH = new IngredientForm(new Guid("aa49dc5d-0956-4735-8a6d-39a102693f7b"), new Guid("4de12110-87b1-4198-a9a9-2b32e45df0f0"), Units.Unit, "dash", "dash/dashes", 1, new Amount(1, Units.Gram));

      // Eggs
      public static IngredientForm EGGS_WHITES = new IngredientForm(new Guid("71a6481d-b5d6-43d6-bb3d-a1eea8e068f3"), new Guid("948aeda5-ffff-41bd-af4e-71d1c740db76"), Units.Cup, "whites", "", 1, new Amount(8, Units.Unit));
      public static IngredientForm EGGS_YOLKS = new IngredientForm(new Guid("27f0ad76-de26-46b3-bb99-66bb8101a131"), new Guid("948aeda5-ffff-41bd-af4e-71d1c740db76"), Units.Cup, "yolks", "", 1, new Amount(16, Units.Unit));
      public static IngredientForm EGGS_VOLUME = new IngredientForm(new Guid("8c1241be-c6d1-461c-8a41-9b49e5e4cadb"), new Guid("948aeda5-ffff-41bd-af4e-71d1c740db76"), Units.Cup, "", "", 1, new Amount(4, Units.Unit));
      public static IngredientForm EGGS_UNIT = new IngredientForm(new Guid("28c374f6-2bd7-4085-a053-f68d0565050a"), new Guid("948aeda5-ffff-41bd-af4e-71d1c740db76"), Units.Unit, "", "", 0, new Amount(0, Units.Gram));

      // All-Purpose Flour
      public static IngredientForm ALL_PURPOSE_FLOUR_SIFTED = new IngredientForm(new Guid("e17e1879-e9d8-443c-bbb5-fe6ba58e596b"), new Guid("daa8fbf6-3347-41b9-826d-078cd321402e"), Units.Cup, "sifted", "", 1, new Amount(135, Units.Gram));
      public static IngredientForm ALL_PURPOSE_FLOUR_UNSIFTED = new IngredientForm(new Guid("e736c49d-c937-484d-9a61-63e2987041ae"), new Guid("daa8fbf6-3347-41b9-826d-078cd321402e"), Units.Cup, "unsifted", "", 1, new Amount(125, Units.Gram));
      public static IngredientForm ALL_PURPOSE_FLOUR_WEIGHT = new IngredientForm(new Guid("d8195543-2186-4870-94a5-adb40315ff74"), new Guid("daa8fbf6-3347-41b9-826d-078cd321402e"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));

      // Salted Butter
      public static IngredientForm SALTED_BUTTER_MELTED = new IngredientForm(new Guid("8a242a66-ddaf-4b08-92d0-fe1ce5951143"), new Guid("ac48a403-b5d7-42f1-bafb-aed0c597f139"), Units.Cup, "melted", "", 1, new Amount(227, Units.Gram));
      public static IngredientForm SALTED_BUTTER_WEIGHT = new IngredientForm(new Guid("d3a30b99-65e2-46c9-9732-38d2d451951a"), new Guid("ac48a403-b5d7-42f1-bafb-aed0c597f139"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));
      public static IngredientForm SALTED_BUTTER_VOLUME = new IngredientForm(new Guid("f45715da-c3dc-47fe-ab45-485d83537b75"), new Guid("ac48a403-b5d7-42f1-bafb-aed0c597f139"), Units.Cup, "", "", 1, new Amount(227, Units.Gram));
      public static IngredientForm SALTED_BUTTER_STICKS = new IngredientForm(new Guid("7730b760-f341-4e57-8056-4c323e8c5a97"), new Guid("ac48a403-b5d7-42f1-bafb-aed0c597f139"), Units.Unit, "sticks", "stick/sticks", 1, new Amount(113, Units.Gram));

      // Vanilla Extract
      public static IngredientForm VANILLA_EXTRACT_SPLASH = new IngredientForm(new Guid("ebe3a38a-4a34-42c9-8d70-de80c5ab293f"), new Guid("6a51bee2-8a40-4c7e-bf91-c7a29e980ac6"), Units.Unit, "splash", "splash/splashes", 1, new Amount(1, Units.Teaspoon));
      public static IngredientForm VANILLA_EXTRACT_VOLUME = new IngredientForm(new Guid("9eaa9ffe-e7f4-46d7-8edf-e96ae24c33c3"), new Guid("6a51bee2-8a40-4c7e-bf91-c7a29e980ac6"), Units.Cup, "", "", 0, new Amount(208, Units.Gram));

      // Water
      public static IngredientForm WATER_VOLUME = new IngredientForm(new Guid("07591417-940b-4e95-942a-cd6fdc6ac4d7"), new Guid("cb44df2d-f27c-442a-bd6e-fd7bdd501f10"), Units.FluidOunce, "", "", 0, new Amount(29.6f, Units.Gram));

      // Black Pepper
      public static IngredientForm BLACK_PEPPER_VOLUME = new IngredientForm(new Guid("980daab2-c556-4d24-988c-6dedf32b661a"), new Guid("514c3e35-1c23-44ed-9438-7f41244b852f"), Units.Tablespoon, "", "", 1, new Amount(6, Units.Gram));
      public static IngredientForm BLACK_PEPPER_DASH = new IngredientForm(new Guid("48376453-3fb9-4ed2-b547-680ad56baf21"), new Guid("514c3e35-1c23-44ed-9438-7f41244b852f"), Units.Unit, "dash", "dash/dashes", 1, new Amount(1, Units.Gram));

      // Baking Powder
      public static IngredientForm BAKING_POWDER_VOLUME = new IngredientForm(new Guid("2f267408-173f-449e-bdf6-17f9d31f84dc"), new Guid("ab207e00-907c-44d9-8afe-a8931b899b0c"), Units.Teaspoon, "", "", 1, new Amount(5, Units.Gram));
      public static IngredientForm BAKING_POWDER_WEIGHT = new IngredientForm(new Guid("6fdf2754-9e7e-44d0-8fcb-f1d32ed0a9ca"), new Guid("ab207e00-907c-44d9-8afe-a8931b899b0c"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));

      // 2% Milk
      public static IngredientForm LOWFAT_MILK_VOLUME = new IngredientForm(new Guid("01422b84-6097-4aeb-bfe6-fedacb2efbee"), new Guid("5a698842-54a9-4ed2-b6c3-aea1bcd157cd"), Units.Cup, "", "", 0, new Amount(244, Units.Gram));

      // Light Brown Sugar
      public static IngredientForm LIGHT_BROWN_SUGAR_WEIGHT = new IngredientForm(new Guid("73f44023-4416-4972-9252-77ec3b264f22"), new Guid("2d1a807c-5102-4ef4-9620-5e951c8365f9"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));
      public static IngredientForm LIGHT_BROWN_SUGAR_UNPACKED = new IngredientForm(new Guid("5d9278c9-ed1d-473e-8e79-9fda0c61898e"), new Guid("2d1a807c-5102-4ef4-9620-5e951c8365f9"), Units.Cup, "unpacked", "", 1, new Amount(145, Units.Gram));
      public static IngredientForm LIGHT_BROWN_SUGAR_PACKED = new IngredientForm(new Guid("3d4b9d20-abe5-4786-aedc-5c5f80738b08"), new Guid("2d1a807c-5102-4ef4-9620-5e951c8365f9"), Units.Cup, "packed", "", 1, new Amount(220, Units.Gram));

      // Baking Soda
      public static IngredientForm BAKING_SODA_WEIGHT = new IngredientForm(new Guid("2bcf431a-d74f-4316-bfbc-08a4f878351a"), new Guid("63e799e8-8c12-4302-931e-f058923d97d1"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));
      public static IngredientForm BAKING_SODA_VOLUME = new IngredientForm(new Guid("71d5f27b-808f-4c2d-a7ba-52206674015f"), new Guid("63e799e8-8c12-4302-931e-f058923d97d1"), Units.Teaspoon, "", "", 1, new Amount(5, Units.Gram));

      // unsweetened baking chocolate squares
      public static IngredientForm UNSWEETENED_BAKING_CHOCOLATE_SQUARES_WEIGHT = new IngredientForm(new Guid("3dbc79a6-98ee-4394-94bc-3102f99ad35e"), new Guid("bef2bde5-d6b3-45d9-95e9-eb75e05721d4"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));
      public static IngredientForm UNSWEETENED_BAKING_CHOCOLATE_SQUARES_SQUARES = new IngredientForm(new Guid("7e32f810-d75a-4b21-a5f7-41d241218d25"), new Guid("bef2bde5-d6b3-45d9-95e9-eb75e05721d4"), Units.Unit, "squares", "1oz square/1oz squares", 1, new Amount(29, Units.Gram));
      public static IngredientForm UNSWEETENED_BAKING_CHOCOLATE_SQUARES_GRATED = new IngredientForm(new Guid("e2f6fd5c-e6f7-4123-aa51-e0ef75ab5759"), new Guid("bef2bde5-d6b3-45d9-95e9-eb75e05721d4"), Units.Cup, "grated", "", 1, new Amount(132, Units.Gram));

      // margarine (oleo)
      public static IngredientForm MARGARINE_MELTED = new IngredientForm(new Guid("71b047d2-39af-40de-a6d2-e5dcaa415d4b"), new Guid("5b42f415-8012-48b6-86e6-576a2f1dac83"), Units.Cup, "melted", "", 1, new Amount(227, Units.Gram));
      public static IngredientForm MARGARINE_VOLUME = new IngredientForm(new Guid("be9eb872-aa4f-45fc-8a45-4ce4fb5b43d0"), new Guid("5b42f415-8012-48b6-86e6-576a2f1dac83"), Units.Cup, "", "", 1, new Amount(229, Units.Gram));
      public static IngredientForm MARGARINE_WEIGHT = new IngredientForm(new Guid("ea42d06f-06cd-4c13-97da-980ed72561da"), new Guid("5b42f415-8012-48b6-86e6-576a2f1dac83"), Units.Ounce, "", "", 0, new Amount(0, Units.Gram));
      public static IngredientForm MARGARINE_STICKS = new IngredientForm(new Guid("2dcbba69-a098-48ca-a1ca-f5a12ad9abd8"), new Guid("5b42f415-8012-48b6-86e6-576a2f1dac83"), Units.Unit, "sticks", "stick/sticks", 1, new Amount(113, Units.Gram));
   }
}