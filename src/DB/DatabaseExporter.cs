using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using KitchenPC.Data;
using KitchenPC.Data.DTO;
using log4net;
using NHibernate;
using NHibernate.Persister.Entity;

namespace KitchenPC.DB.Provisioning
{
   public class DatabaseExporter : IDisposable, IProvisioner
   {
      readonly IStatelessSession session;
      public static ILog Log = LogManager.GetLogger(typeof (DatabaseExporter));

      public DatabaseExporter(IStatelessSession session)
      {
         this.session = session;
      }

      IEnumerable<D> ImportTableData<T, D>(Func<IDataReader, D> action) where T : new()
      {
         using (var cmd = session.Connection.CreateCommand())
         {
            var persister = session.GetSessionImplementation().GetEntityPersister(null, new T()) as ILockable;
            if (persister == null) throw new NullReferenceException();

            cmd.CommandType = CommandType.TableDirect;
            cmd.CommandText = persister.RootTableName;
            using (var reader = cmd.ExecuteReader())
            {
               while (reader.Read())
               {
                  yield return action(reader);
               }
            }
         }
      }

      public IngredientForms[] IngredientForms()
      {
         var list = ImportTableData<Models.IngredientForms, IngredientForms>(r => new IngredientForms
         {
            IngredientFormId = (Guid) r["IngredientFormId"],
            IngredientId = (Guid) r["IngredientId"],
            ConvMultiplier = (short) r["ConvMultiplier"],
            FormAmount = (float) r["FormAmount"],
            UnitType = Unit.Parse<Units>(r["UnitType"]),
            UnitName = r["UnitName"] as String,
            FormUnit = Unit.Parse<Units>(r["FormUnit"]),
            FormDisplayName = r["FormDisplayName"] as String
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from IngredientForms.", list.Count());
         return list;
      }

      public IngredientMetadata[] IngredientMetadata()
      {
         var list = ImportTableData<Models.IngredientMetadata, IngredientMetadata>(r => new IngredientMetadata
         {
            IngredientMetadataId = (Guid) r["IngredientMetadataId"],
            IngredientId = (Guid) r["IngredientId"],
            HasMeat = r["HasMeat"] as Boolean?,
            CarbsPerUnit = r["CarbsPerUnit"] as Single?,
            HasRedMeat = r["HasRedMeat"] as Boolean?,
            SugarPerUnit = r["SugarPerUnit"] as Single?,
            HasPork = r["HasPork"] as Boolean?,
            FatPerUnit = r["FatPerUnit"] as Single?,
            SodiumPerUnit = r["SodiumPerUnit"] as Single?,
            CaloriesPerUnit = r["CaloriesPerUnit"] as Single?,
            Spicy = (Int16) r["Spicy"],
            Sweet = (Int16) r["Sweet"],
            HasGluten = r["HasGluten"] as Boolean?,
            HasAnimal = r["HasAnimal"] as Boolean?,
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from IngredientMetadata.", list.Count());
         return list;
      }

      public Data.DTO.Ingredients[] Ingredients()
      {
         var list = ImportTableData<Models.Ingredients, Data.DTO.Ingredients>(r => new Data.DTO.Ingredients
         {
            IngredientId = (Guid) r["IngredientId"],
            UsdaId = r["UsdaId"] as String,
            FoodGroup = r["FoodGroup"] as String,
            DisplayName = r["DisplayName"] as String,
            ManufacturerName = r["ManufacturerName"] as String,
            ConversionType = Unit.Parse<UnitType>(r["ConversionType"]),
            UnitName = r["UnitName"] as String,
            UsdaDesc = r["UsdaDesc"] as String,
            UnitWeight = (Int32) r["UnitWeight"]
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from Ingredients.", list.Count());
         return list;
      }

      public NlpAnomalousIngredients[] NlpAnomalousIngredients()
      {
         var list = ImportTableData<Models.NlpAnomalousIngredients, NlpAnomalousIngredients>(r => new NlpAnomalousIngredients
         {
            AnomalousIngredientId = (Guid) r["AnomalousIngredientId"],
            Name = r["Name"] as String,
            IngredientId = (Guid) r["IngredientId"],
            WeightFormId = r["WeightFormId"] as Guid?,
            VolumeFormId = r["VolumeFormId"] as Guid?,
            UnitFormId = r["UnitFormId"] as Guid?
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from NlpAnomalousIngredients.", list.Count());
         return list;
      }

      public NlpDefaultPairings[] NlpDefaultPairings()
      {
         var list = ImportTableData<Models.NlpDefaultPairings, NlpDefaultPairings>(r => new NlpDefaultPairings
         {
            DefaultPairingId = (Guid) r["DefaultPairingId"],
            IngredientId = (Guid) r["IngredientId"],
            WeightFormId = r["WeightFormId"] as Guid?,
            VolumeFormId = r["VolumeFormId"] as Guid?,
            UnitFormId = r["UnitFormId"] as Guid?
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from NlpDefaultPairings.", list.Count());
         return list;
      }

      public NlpFormSynonyms[] NlpFormSynonyms()
      {
         var list = ImportTableData<Models.NlpFormSynonyms, NlpFormSynonyms>(r => new NlpFormSynonyms
         {
            FormSynonymId = (Guid) r["FormSynonymId"],
            IngredientId = (Guid) r["IngredientId"],
            FormId = (Guid) r["FormId"],
            Name = r["Name"] as String
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from NlpFormSynonyms.", list.Count());
         return list;
      }

      public NlpIngredientSynonyms[] NlpIngredientSynonyms()
      {
         var list = ImportTableData<Models.NlpIngredientSynonyms, NlpIngredientSynonyms>(r => new NlpIngredientSynonyms
         {
            IngredientSynonymId = (Guid) r["IngredientSynonymId"],
            IngredientId = (Guid) r["IngredientId"],
            Alias = r["Alias"] as String,
            Prepnote = r["Prepnote"] as String
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from NlpIngredientSynonyms.", list.Count());
         return list;
      }

      public NlpPrepNotes[] NlpPrepNotes()
      {
         var list = ImportTableData<Models.NlpPrepNotes, NlpPrepNotes>(r => new NlpPrepNotes
         {
            Name = r["Name"] as String
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from NlpPrepNotes.", list.Count());
         return list;
      }

      public NlpUnitSynonyms[] NlpUnitSynonyms()
      {
         var list = ImportTableData<Models.NlpUnitSynonyms, NlpUnitSynonyms>(r => new NlpUnitSynonyms
         {
            UnitSynonymId = (Guid) r["UnitSynonymId"],
            IngredientId = (Guid) r["IngredientId"],
            FormId = (Guid) r["FormId"],
            Name = r["Name"] as String
         }).ToArray();

         Log.DebugFormat("Read {0} row(s) from NlpUnitSynonyms.", list.Count());
         return list;
      }

      public List<Data.DTO.Recipes> Recipes()
      {
         var list = ImportTableData<Models.Recipes, Data.DTO.Recipes>(r => new Data.DTO.Recipes
         {
            RecipeId = (Guid) r["RecipeId"],
            CookTime = r["CookTime"] as Int16?,
            Steps = r["Steps"] as String,
            PrepTime = r["PrepTime"] as Int16?,
            Rating = (Int16) r["Rating"],
            Description = r["Description"] as String,
            Title = r["Title"] as String,
            Hidden = (bool) r["Hidden"],
            Credit = r["Credit"] as String,
            CreditUrl = r["CreditUrl"] as String,
            DateEntered = (DateTime) r["DateEntered"],
            ServingSize = (Int16) r["ServingSize"],
            ImageUrl = r["ImageUrl"] as String
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from Recipes.", list.Count());
         return list;
      }

      public List<RecipeMetadata> RecipeMetadata()
      {
         var list = ImportTableData<Models.RecipeMetadata, RecipeMetadata>(r => new RecipeMetadata
         {
            RecipeMetadataId = (Guid) r["RecipeMetadataId"],
            RecipeId = (Guid) r["RecipeId"],
            PhotoRes = (Int32) r["PhotoRes"],
            Commonality = (Single) r["Commonality"],
            UsdaMatch = (bool) r["UsdaMatch"],
            MealBreakfast = (bool) r["MealBreakfast"],
            MealLunch = (bool) r["MealLunch"],
            MealDinner = (bool) r["MealDinner"],
            MealDessert = (bool) r["MealDessert"],
            DietNomeat = (bool) r["DietNomeat"],
            DietGlutenFree = (bool) r["DietGlutenFree"],
            DietNoRedMeat = (bool) r["DietNoRedMeat"],
            DietNoAnimals = (bool) r["DietNoAnimals"],
            DietNoPork = (bool) r["DietNoPork"],
            NutritionTotalfat = (Int16) r["NutritionTotalfat"],
            NutritionTotalSodium = (Int16) r["NutritionTotalSodium"],
            NutritionLowSodium = (bool) r["NutritionLowSodium"],
            NutritionLowSugar = (bool) r["NutritionLowSugar"],
            NutritionLowCalorie = (bool) r["NutritionLowCalorie"],
            NutritionTotalSugar = (Int16) r["NutritionTotalSugar"],
            NutritionTotalCalories = (Int16) r["NutritionTotalCalories"],
            NutritionLowFat = (bool) r["NutritionLowFat"],
            NutritionLowCarb = (bool) r["NutritionLowCarb"],
            NutritionTotalCarbs = (Int16) r["NutritionTotalCarbs"],
            SkillQuick = (bool) r["SkillQuick"],
            SkillEasy = (bool) r["SkillEasy"],
            SkillCommon = (bool) r["SkillCommon"],
            TasteMildToSpicy = (Int16) r["TasteMildToSpicy"],
            TasteSavoryToSweet = (Int16) r["TasteSavoryToSweet"]
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from RecipeMetadata.", list.Count());
         return list;
      }

      public List<RecipeIngredients> RecipeIngredients()
      {
         var list = ImportTableData<Models.RecipeIngredients, RecipeIngredients>(r => new RecipeIngredients
         {
            RecipeIngredientId = (Guid) r["RecipeIngredientId"],
            RecipeId = (Guid) r["RecipeId"],
            IngredientId = (Guid) r["IngredientId"],
            IngredientFormId = r["IngredientFormId"] as Guid?,
            Unit = Unit.Parse<Units>(r["Unit"]),
            QtyLow = r["QtyLow"] as Single?,
            DisplayOrder = (Int16) r["DisplayOrder"],
            PrepNote = r["PrepNote"] as String,
            Qty = r["Qty"] as Single?,
            Section = r["Section"] as String
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from RecipeIngredients.", list.Count());
         return list;
      }

      public List<Favorites> Favorites()
      {
         var list = ImportTableData<Models.Favorites, Favorites>(r => new Favorites
         {
            FavoriteId = (Guid) r["FavoriteId"],
            UserId = (Guid) r["UserId"],
            RecipeId = (Guid) r["RecipeId"],
            MenuId = r["MenuId"] as Guid?
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from Favorites.", list.Count());
         return list;
      }

      public List<Data.DTO.Menus> Menus()
      {
         var list = ImportTableData<Models.Menus, Data.DTO.Menus>(r => new Data.DTO.Menus
         {
            MenuId = (Guid) r["MenuId"],
            UserId = (Guid) r["UserId"],
            Title = r["Title"] as String,
            CreatedDate = (DateTime) r["CreatedDate"]
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from Menus.", list.Count());
         return list;
      }

      public List<QueuedRecipes> QueuedRecipes()
      {
         var list = ImportTableData<Models.QueuedRecipes, QueuedRecipes>(r => new QueuedRecipes
         {
            QueueId = (Guid) r["QueueId"],
            UserId = (Guid) r["UserId"],
            RecipeId = (Guid) r["RecipeId"],
            QueuedDate = (DateTime) r["QueuedDate"]
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from QueuedRecipes.", list.Count());
         return list;
      }

      public List<RecipeRatings> RecipeRatings()
      {
         var list = ImportTableData<Models.RecipeRatings, RecipeRatings>(r => new RecipeRatings
         {
            RatingId = (Guid) r["RatingId"],
            UserId = (Guid) r["UserId"],
            RecipeId = (Guid) r["RecipeId"],
            Rating = (Int16) r["Rating"]
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from RecipeRatings.", list.Count());
         return list;
      }

      public List<Data.DTO.ShoppingLists> ShoppingLists()
      {
         var list = ImportTableData<Models.ShoppingLists, Data.DTO.ShoppingLists>(r => new Data.DTO.ShoppingLists
         {
            ShoppingListId = (Guid) r["ShoppingListId"],
            UserId = (Guid) r["UserId"],
            Title = r["Title"] as String
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from ShoppingLists.", list.Count());
         return list;
      }

      public List<ShoppingListItems> ShoppingListItems()
      {
         var list = ImportTableData<Models.ShoppingListItems, ShoppingListItems>(r => new ShoppingListItems
         {
            ItemId = (Guid) r["ItemId"],
            Raw = r["Raw"] as String,
            Qty = r["Qty"] as Single?,
            Unit = Unit.ParseNullable<Units>(r["Unit"]),
            UserId = (Guid) r["UserId"],
            IngredientId = r["IngredientId"] as Guid?,
            RecipeId = r["RecipeId"] as Guid?,
            ShoppingListId = r["ShoppingListId"] as Guid?,
            CrossedOut = (bool) r["CrossedOut"]
         }).ToList();

         Log.DebugFormat("Read {0} row(s) from ShoppingListItems.", list.Count());
         return list;
      }

      public void Dispose()
      {
         session.Dispose();
      }
   }
}