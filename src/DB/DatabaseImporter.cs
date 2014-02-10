using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Data.DTO;
using log4net;
using NHibernate;

namespace KitchenPC.DB.Provisioning
{
   public class DatabaseImporter : IDisposable
   {
      readonly ISession session;
      public static ILog Log = LogManager.GetLogger(typeof (DatabaseImporter));

      public DatabaseImporter(ISession session)
      {
         this.session = session;
      }

      public void Import(IEnumerable<Data.DTO.Ingredients> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.Ingredients
               {
                  IngredientId = row.IngredientId,
                  FoodGroup = row.FoodGroup,
                  UsdaId = row.UsdaId,
                  UnitName = row.UnitName,
                  ManufacturerName = row.ManufacturerName,
                  ConversionType = row.ConversionType,
                  UnitWeight = row.UnitWeight,
                  DisplayName = row.DisplayName,
                  UsdaDesc = row.UsdaDesc
               };

               session.Save(dbRow, row.IngredientId);
            }

            Log.DebugFormat("Created {0} row(s) in Ingredients", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<IngredientForms> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.IngredientForms
               {
                  IngredientFormId = row.IngredientFormId,
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  ConvMultiplier = row.ConvMultiplier,
                  FormAmount = row.FormAmount,
                  UnitType = row.UnitType,
                  UnitName = row.UnitName,
                  FormUnit = row.FormUnit,
                  FormDisplayName = row.FormDisplayName
               };

               session.Save(dbRow, row.IngredientFormId);
            }

            Log.DebugFormat("Created {0} row(s) in IngredientForms", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<IngredientMetadata> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.IngredientMetadata
               {
                  IngredientMetadataId = row.IngredientMetadataId,
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  HasMeat = row.HasMeat,
                  CarbsPerUnit = row.CarbsPerUnit,
                  HasRedMeat = row.HasRedMeat,
                  SugarPerUnit = row.SugarPerUnit,
                  HasPork = row.HasPork,
                  FatPerUnit = row.FatPerUnit,
                  SodiumPerUnit = row.SodiumPerUnit,
                  CaloriesPerUnit = row.CaloriesPerUnit,
                  Spicy = row.Spicy,
                  Sweet = row.Sweet,
                  HasGluten = row.HasGluten,
                  HasAnimal = row.HasAnimal
               };

               session.Save(dbRow, row.IngredientMetadataId);
            }

            Log.DebugFormat("Created {0} row(s) in IngredientMetadata", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<NlpAnomalousIngredients> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.NlpAnomalousIngredients
               {
                  AnomalousIngredientId = row.AnomalousIngredientId,
                  Name = row.Name,
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  WeightForm = row.WeightFormId.HasValue ? Models.IngredientForms.FromId(row.WeightFormId.Value) : null,
                  VolumeForm = row.VolumeFormId.HasValue ? Models.IngredientForms.FromId(row.VolumeFormId.Value) : null,
                  UnitForm = row.UnitFormId.HasValue ? Models.IngredientForms.FromId(row.UnitFormId.Value) : null
               };

               session.Save(dbRow, row.AnomalousIngredientId);
            }

            Log.DebugFormat("Created {0} row(s) in NlpAnomalousIngredients", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<NlpDefaultPairings> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.NlpDefaultPairings
               {
                  DefaultPairingId = row.DefaultPairingId,
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  WeightForm = row.WeightFormId.HasValue ? Models.IngredientForms.FromId(row.WeightFormId.Value) : null,
                  VolumeForm = row.VolumeFormId.HasValue ? Models.IngredientForms.FromId(row.VolumeFormId.Value) : null,
                  UnitForm = row.UnitFormId.HasValue ? Models.IngredientForms.FromId(row.UnitFormId.Value) : null
               };

               session.Save(dbRow, row.DefaultPairingId);
            }

            Log.DebugFormat("Created {0} row(s) in NlpDefaultPairings", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<NlpFormSynonyms> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.NlpFormSynonyms
               {
                  FormSynonymId = row.FormSynonymId,
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  Form = Models.IngredientForms.FromId(row.FormId),
                  Name = row.Name
               };

               session.Save(dbRow, row.FormSynonymId);
            }

            Log.DebugFormat("Created {0} row(s) in NlpFormSynonyms", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<NlpIngredientSynonyms> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.NlpIngredientSynonyms
               {
                  IngredientSynonymId = row.IngredientSynonymId,
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  Alias = row.Alias,
                  Prepnote = row.Prepnote
               };

               session.Save(dbRow, row.IngredientSynonymId);
            }

            Log.DebugFormat("Created {0} row(s) in NlpIngredientSynonyms", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<NlpPrepNotes> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.NlpPrepNotes
               {
                  Name = row.Name
               };

               session.Save(dbRow);
            }

            Log.DebugFormat("Created {0} row(s) in NlpPrepNotes", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<NlpUnitSynonyms> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.NlpUnitSynonyms
               {
                  UnitSynonymId = row.UnitSynonymId,
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  Form = Models.IngredientForms.FromId(row.FormId),
                  Name = row.Name
               };

               session.Save(dbRow, row.UnitSynonymId);
            }

            Log.DebugFormat("Created {0} row(s) in NlpUnitSynonyms", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<Data.DTO.Recipes> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.Recipes
               {
                  RecipeId = row.RecipeId,
                  CookTime = row.CookTime,
                  Steps = row.Steps,
                  PrepTime = row.PrepTime,
                  Rating = row.Rating,
                  Description = row.Description,
                  Title = row.Title,
                  Hidden = row.Hidden,
                  Credit = row.Credit,
                  CreditUrl = row.CreditUrl,
                  DateEntered = row.DateEntered,
                  ServingSize = row.ServingSize,
                  ImageUrl = row.ImageUrl,
                  Ingredients = new List<Models.RecipeIngredients>()
               };

               session.Save(dbRow, row.RecipeId);
            }

            Log.DebugFormat("Created {0} row(s) in Recipes", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<RecipeIngredients> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.RecipeIngredients
               {
                  RecipeIngredientId = row.RecipeIngredientId,
                  Recipe = Models.Recipes.FromId(row.RecipeId),
                  Ingredient = Models.Ingredients.FromId(row.IngredientId),
                  IngredientForm = row.IngredientFormId.HasValue ? Models.IngredientForms.FromId(row.IngredientFormId.Value) : null,
                  Unit = row.Unit,
                  QtyLow = row.QtyLow,
                  DisplayOrder = row.DisplayOrder,
                  PrepNote = row.PrepNote,
                  Qty = row.Qty,
                  Section = row.Section
               };

               session.Save(dbRow, row.RecipeIngredientId);
            }

            Log.DebugFormat("Created {0} row(s) in RecipeIngredients", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<RecipeMetadata> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.RecipeMetadata
               {
                  RecipeMetadataId = row.RecipeMetadataId,
                  Recipe = Models.Recipes.FromId(row.RecipeId),
                  PhotoRes = row.PhotoRes,
                  Commonality = row.Commonality,
                  UsdaMatch = row.UsdaMatch,
                  MealBreakfast = row.MealBreakfast,
                  MealLunch = row.MealLunch,
                  MealDinner = row.MealDinner,
                  MealDessert = row.MealDessert,
                  DietNomeat = row.DietNomeat,
                  DietGlutenFree = row.DietGlutenFree,
                  DietNoRedMeat = row.DietNoRedMeat,
                  DietNoAnimals = row.DietNoAnimals,
                  DietNoPork = row.DietNoPork,
                  NutritionTotalfat = row.NutritionTotalfat,
                  NutritionTotalSodium = row.NutritionTotalSodium,
                  NutritionLowSodium = row.NutritionLowSodium,
                  NutritionLowSugar = row.NutritionLowSugar,
                  NutritionLowCalorie = row.NutritionLowCalorie,
                  NutritionTotalSugar = row.NutritionTotalSugar,
                  NutritionTotalCalories = row.NutritionTotalCalories,
                  NutritionLowFat = row.NutritionLowFat,
                  NutritionLowCarb = row.NutritionLowCarb,
                  NutritionTotalCarbs = row.NutritionTotalCarbs,
                  SkillQuick = row.SkillQuick,
                  SkillEasy = row.SkillEasy,
                  SkillCommon = row.SkillCommon,
                  TasteMildToSpicy = row.TasteMildToSpicy,
                  TasteSavoryToSweet = row.TasteSavoryToSweet
               };

               session.Save(dbRow, row.RecipeMetadataId);
            }

            Log.DebugFormat("Created {0} row(s) in RecipeMetadata", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<Data.DTO.Menus> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.Menus
               {
                  MenuId = row.MenuId,
                  UserId = row.UserId,
                  Title = row.Title,
                  CreatedDate = row.CreatedDate
               };

               session.Save(dbRow, row.MenuId);
            }

            Log.DebugFormat("Created {0} row(s) in Menus", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<Favorites> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.Favorites
               {
                  FavoriteId = row.FavoriteId,
                  UserId = row.UserId,
                  Recipe = Models.Recipes.FromId(row.RecipeId),
                  Menu = row.MenuId.HasValue ? Models.Menus.FromId(row.MenuId.Value) : null
               };

               session.Save(dbRow, row.FavoriteId);
            }

            Log.DebugFormat("Created {0} row(s) in Favorites", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<QueuedRecipes> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.QueuedRecipes
               {
                  QueueId = row.QueueId,
                  UserId = row.UserId,
                  Recipe = Models.Recipes.FromId(row.RecipeId),
                  QueuedDate = row.QueuedDate
               };

               session.Save(dbRow, row.QueueId);
            }

            Log.DebugFormat("Created {0} row(s) in QueuedRecipes", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<RecipeRatings> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.RecipeRatings
               {
                  RatingId = row.RatingId,
                  UserId = row.UserId,
                  Recipe = Models.Recipes.FromId(row.RecipeId),
                  Rating = row.Rating
               };

               session.Save(dbRow, row.RatingId);
            }

            Log.DebugFormat("Created {0} row(s) in RecipeRatings", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<Data.DTO.ShoppingLists> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbRow = new Models.ShoppingLists
               {
                  ShoppingListId = row.ShoppingListId,
                  UserId = row.UserId,
                  Title = row.Title
               };

               session.Save(dbRow, row.ShoppingListId);
            }

            Log.DebugFormat("Created {0} row(s) in ShoppingLists", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Import(IEnumerable<ShoppingListItems> data)
      {
         using (var transaction = session.BeginTransaction())
         {
            var d = data.ToArray();
            foreach (var row in d)
            {
               var dbItem = new Models.ShoppingListItems
               {
                  ItemId = row.ItemId,
                  Raw = row.Raw,
                  Qty = row.Qty,
                  Unit = row.Unit,
                  UserId = row.UserId,
                  Ingredient = row.IngredientId.HasValue ? Models.Ingredients.FromId(row.IngredientId.Value) : null,
                  Recipe = row.RecipeId.HasValue ? Models.Recipes.FromId(row.RecipeId.Value) : null,
                  ShoppingList = row.ShoppingListId.HasValue ? Models.ShoppingLists.FromId(row.ShoppingListId.Value) : null,
                  CrossedOut = row.CrossedOut
               };

               session.Save(dbItem, row.ItemId);
            }

            Log.DebugFormat("Created {0} row(s) in ShoppingListItems", d.Count());
            transaction.Commit();
            session.Flush();
         }
      }

      public void Dispose()
      {
         session.Dispose();
      }
   }
}