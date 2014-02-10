using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Ingredients;
using KitchenPC.NLP;
using KitchenPC.Recipes;

namespace KitchenPC.Context.Fluent
{
   /// <summary>Provides the ability to fluently express recipe related actions, such as loading recipes, finding recipes and sharing recipes.</summary>
   public class RecipeAction
   {
      readonly IKPCContext context;

      public RecipeAction(IKPCContext context)
      {
         this.context = context;
      }

      public RecipeLoader Load(Recipe recipe)
      {
         return new RecipeLoader(context, recipe);
      }

      public RecipeRater Rate(Recipe recipe, Rating rating)
      {
         return new RecipeRater(context, recipe, rating);
      }

      public RecipeFinder Search(RecipeQuery query)
      {
         return new RecipeFinder(context, query);
      }

      public RecipeFinder Search(Func<RecipeQueryBuilder, RecipeQueryBuilder> searchBuilder)
      {
         var builder = new RecipeQueryBuilder(new RecipeQuery());
         var query = searchBuilder(builder).Query;

         return Search(query);
      }

      public RecipeCreator Create
      {
         get
         {
            return new RecipeCreator(context);
         }
      }
   }

   /// <summary>Provides the ability to load one or more recipes.</summary>
   public class RecipeLoader
   {
      readonly IKPCContext context;
      readonly IList<Recipe> recipesToLoad;

      bool withCommentCount;
      bool withUserRating;
      bool withCookbookStatus;
      bool withMethod;
      bool withPermalink;

      public RecipeLoader WithCommentCount
      {
         get
         {
            withCommentCount = true;
            return this;
         }
      }

      public RecipeLoader WithUserRating
      {
         get
         {
            withUserRating = true;
            return this;
         }
      }

      public RecipeLoader WithCookbookStatus
      {
         get
         {
            withCookbookStatus = true;
            return this;
         }
      }

      public RecipeLoader WithMethod
      {
         get
         {
            withMethod = true;
            return this;
         }
      }

      public RecipeLoader WithPermalink
      {
         get
         {
            withPermalink = true;
            return this;
         }
      }

      public RecipeLoader(IKPCContext context, Recipe recipe)
      {
         this.context = context;
         this.recipesToLoad = new List<Recipe>() {recipe};
      }

      public RecipeLoader Load(Recipe recipe)
      {
         recipesToLoad.Add(recipe);
         return this;
      }

      public IList<Recipe> List()
      {
         var options = new ReadRecipeOptions
         {
            ReturnCommentCount = withCommentCount,
            ReturnCookbookStatus = withCookbookStatus,
            ReturnMethod = withMethod,
            ReturnPermalink = withPermalink,
            ReturnUserRating = withUserRating
         };

         return context.ReadRecipes(recipesToLoad.Select(r => r.Id).ToArray(), options);
      }
   }

   /// <summary>Provides the ability to rate a recipe.</summary>
   public class RecipeRater
   {
      readonly IKPCContext context;
      readonly Dictionary<Recipe, Rating> newRatings;

      public RecipeRater(IKPCContext context, Recipe recipe, Rating rating)
      {
         this.context = context;
         this.newRatings = new Dictionary<Recipe, Rating>();

         newRatings.Add(recipe, rating);
      }

      public RecipeRater Rate(Recipe recipe, Rating rating)
      {
         newRatings.Add(recipe, rating);
         return this;
      }

      public void Commit()
      {
         foreach (var newRating in newRatings)
         {
            context.RateRecipe(newRating.Key.Id, newRating.Value);
         }
      }
   }

   /// <summary>Provides the ability to search for recipe.</summary>
   public class RecipeFinder
   {
      readonly IKPCContext context;
      readonly RecipeQuery query;

      public RecipeFinder(IKPCContext context, RecipeQuery query)
      {
         this.context = context;
         this.query = query;
      }

      public SearchResults Results()
      {
         return context.RecipeSearch(query);
      }
   }

   /// <summary>Provides the ability to fluently build a search query.</summary>
   public class RecipeQueryBuilder
   {
      readonly RecipeQuery query;

      public RecipeQueryBuilder(RecipeQuery query)
      {
         this.query = query;
      }

      public RecipeQueryBuilder Keywords(string keywords)
      {
         query.Keywords = keywords;
         return this;
      }

      public RecipeQueryBuilder MinRating(Rating rating)
      {
         query.Rating = rating;
         return this;
      }

      public RecipeQueryBuilder IncludeIngredients(params Ingredient[] ingredients)
      {
         query.Include = ingredients.Select(i => i.Id).ToArray();
         return this;
      }

      public RecipeQueryBuilder ExcludeIngredients(params Ingredient[] ingredients)
      {
         query.Exclude = ingredients.Select(i => i.Id).ToArray();
         return this;
      }

      public RecipeQueryBuilder Offset(int startOffset)
      {
         query.Offset = startOffset;
         return this;
      }

      public RecipeQueryBuilder Meal(MealFilter mealFilter)
      {
         query.Meal = mealFilter;
         return this;
      }

      public RecipeQueryBuilder HasPhoto(RecipeQuery.PhotoFilter photoFilter)
      {
         query.Photos = photoFilter;
         return this;
      }

      public RecipeQueryBuilder SortBy(RecipeQuery.SortOrder sortOrder, RecipeQuery.SortDirection direction = RecipeQuery.SortDirection.Ascending)
      {
         query.Sort = sortOrder;
         query.Direction = direction;
         return this;
      }

      public RecipeQueryBuilder MaxPrep(short minutes)
      {
         query.Time.MaxPrep = minutes;
         return this;
      }

      public RecipeQueryBuilder MaxCook(short minutes)
      {
         query.Time.MaxCook = minutes;
         return this;
      }

      public RecipeQueryBuilder MildToSpicy(RecipeQuery.SpicinessLevel scale)
      {
         query.Taste.MildToSpicy = scale;
         return this;
      }

      public RecipeQueryBuilder SavoryToSweet(RecipeQuery.SweetnessLevel scale)
      {
         query.Taste.SavoryToSweet = scale;
         return this;
      }

      public RecipeQueryBuilder GlutenFree
      {
         get
         {
            query.Diet.GlutenFree = true;
            return this;
         }
      }

      public RecipeQueryBuilder NoAnimals
      {
         get
         {
            query.Diet.NoAnimals = true;
            return this;
         }
      }

      public RecipeQueryBuilder NoMeat
      {
         get
         {
            query.Diet.NoMeat = true;
            return this;
         }
      }

      public RecipeQueryBuilder NoPork
      {
         get
         {
            query.Diet.NoPork = true;
            return this;
         }
      }

      public RecipeQueryBuilder NoRedMeat
      {
         get
         {
            query.Diet.NoRedMeat = true;
            return this;
         }
      }

      public RecipeQueryBuilder LowCalorie
      {
         get
         {
            query.Nutrition.LowCalorie = true;
            return this;
         }
      }

      public RecipeQueryBuilder LowCarb
      {
         get
         {
            query.Nutrition.LowCarb = true;
            return this;
         }
      }

      public RecipeQueryBuilder LowFat
      {
         get
         {
            query.Nutrition.LowFat = true;
            return this;
         }
      }

      public RecipeQueryBuilder LowSodium
      {
         get
         {
            query.Nutrition.LowSodium = true;
            return this;
         }
      }

      public RecipeQueryBuilder LowSugar
      {
         get
         {
            query.Nutrition.LowSugar = true;
            return this;
         }
      }

      public RecipeQueryBuilder Common
      {
         get
         {
            query.Skill.Common = true;
            return this;
         }
      }

      public RecipeQueryBuilder Easy
      {
         get
         {
            query.Skill.Easy = true;
            return this;
         }
      }

      public RecipeQueryBuilder Quick
      {
         get
         {
            query.Skill.Quick = true;
            return this;
         }
      }

      public RecipeQuery Query
      {
         get
         {
            return query;
         }
      }
   }

   /// <summary>Provides the ability to fluently build a search query.</summary>
   public class RecipeCreator
   {
      readonly IKPCContext context;
      readonly Recipe recipe;

      public RecipeCreator(IKPCContext context)
      {
         this.context = context;
         this.recipe = new Recipe();

         this.recipe.DateEntered = DateTime.Now;
      }

      public RecipeCreator WithTitle(string title)
      {
         recipe.Title = title;
         return this;
      }

      public RecipeCreator WithDescription(string desc)
      {
         recipe.Description = desc;
         return this;
      }

      public RecipeCreator WithCredit(string credit)
      {
         recipe.Credit = credit;
         return this;
      }

      public RecipeCreator WithCreditUrl(Uri creditUrl)
      {
         recipe.CreditUrl = creditUrl.ToString();
         return this;
      }

      public RecipeCreator WithMethod(string method)
      {
         recipe.Method = method;
         return this;
      }

      public RecipeCreator WithDateEntered(DateTime date)
      {
         recipe.DateEntered = date;
         return this;
      }

      public RecipeCreator WithPrepTime(short prepTime)
      {
         recipe.PrepTime = prepTime;
         return this;
      }

      public RecipeCreator WithCookTime(short cookTime)
      {
         recipe.CookTime = cookTime;
         return this;
      }

      public RecipeCreator WithRating(Rating rating)
      {
         recipe.AvgRating = (short) rating;
         return this;
      }

      public RecipeCreator WithServingSize(short servings)
      {
         recipe.ServingSize = servings;
         return this;
      }

      public RecipeCreator WithTags(RecipeTags tags)
      {
         recipe.Tags = tags;
         return this;
      }

      public RecipeCreator WithImage(Uri imageUri)
      {
         recipe.ImageUrl = imageUri.ToString();
         return this;
      }

      public RecipeCreator WithIngredients(Func<IngredientAdder, IngredientAdder> ingredientAdder)
      {
         var adder = ingredientAdder(new IngredientAdder(context, recipe));
         return this;
      }

      public RecipeCreator WithIngredients(string section, Func<IngredientAdder, IngredientAdder> ingredientAdder)
      {
         var adder = ingredientAdder(new IngredientAdder(context, recipe, section));
         return this;
      }

      public RecipeResult Commit()
      {
         return context.CreateRecipe(recipe);
      }
   }

   public class IngredientAdder
   {
      readonly IKPCContext context;
      readonly Recipe recipe;
      readonly String section;

      public IngredientAdder(IKPCContext context, Recipe recipe)
      {
         this.context = context;
         this.recipe = recipe;
      }

      public IngredientAdder(IKPCContext context, Recipe recipe, string section) : this(context, recipe)
      {
         this.section = section;
      }

      public IngredientAdder AddIngredientUsage(IngredientUsage usage)
      {
         if (!String.IsNullOrWhiteSpace(section))
            usage.Section = section;

         recipe.AddIngredient(usage);
         return this;
      }

      public IngredientAdder AddIngredientUsage(Func<IngredientUsageCreator, IngredientUsageCreator> createAction)
      {
         var creator = createAction(IngredientUsage.Create);
         var usage = creator.Usage;
         // Validate Ingredient
         usage.Ingredient = context.ReadIngredient(usage.Ingredient.Id);
         //

         if (usage.Form != null) // Verify form
         {
            var forms = context.ReadFormsForIngredient(usage.Ingredient.Id).Forms;
            var validatedForm = forms.FirstOrDefault(f => f.FormId == usage.Form.FormId);
            if (validatedForm == null)
               throw new InvalidFormException(usage.Ingredient, usage.Form);

            usage.Form = validatedForm;
         }

         return AddIngredientUsage(usage);
      }

      public IngredientAdder AddIngredient(Ingredient ingredient, Amount amount, string prepNote = null)
      {
         var usage = new IngredientUsage(ingredient, null, amount, prepNote);
         return AddIngredientUsage(usage);
      }

      public IngredientAdder AddIngredient(Ingredient ingredient, string prepNote = null)
      {
         return AddIngredient(ingredient, null, prepNote);
      }

      public IngredientAdder AddRaw(string raw)
      {
         var result = context.ParseIngredientUsage(raw);

         if (result is Match)
         {
            return AddIngredientUsage(result.Usage);
         }

         throw new CouldNotParseUsageException(result, raw);
      }
   }

   public class IngredientUsageCreator
   {
      public IngredientUsage Usage { get; private set; }

      public IngredientUsageCreator(IngredientUsage usage)
      {
         this.Usage = usage;
      }

      public IngredientUsageCreator WithIngredient(Ingredient ingredient)
      {
         Usage.Ingredient = ingredient;
         return this;
      }

      public IngredientUsageCreator WithForm(IngredientForm form)
      {
         Usage.Form = form;
         return this;
      }

      public IngredientUsageCreator WithAmount(Amount amount)
      {
         Usage.Amount = amount;
         return this;
      }

      public IngredientUsageCreator WithAmount(float size, Units unit)
      {
         Usage.Amount = new Amount(size, unit);
         return this;
      }

      public IngredientUsageCreator WithPrepNote(string prepNote)
      {
         Usage.PrepNote = prepNote;
         return this;
      }

      public IngredientUsageCreator InSection(string section)
      {
         Usage.Section = section;
         return this;
      }
   }
}