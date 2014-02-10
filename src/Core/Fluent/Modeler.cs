using System;
using System.Collections.Generic;
using System.Linq;
using KitchenPC.Ingredients;
using KitchenPC.Modeler;
using KitchenPC.Recipes;
using IngredientUsage = KitchenPC.Ingredients.IngredientUsage;

namespace KitchenPC.Context.Fluent
{
   /// <summary>Provides the ability to fluently express modeler related actions, such as generating or compiling a model.</summary>
   public class ModelerAction
   {
      readonly IKPCContext context;

      public ModelerAction(IKPCContext context)
      {
         this.context = context;
      }

      public ModelingSessionAction WithSession(ModelingSession session)
      {
         return new ModelingSessionAction(session);
      }

      public ModelingSessionAction WithProfile(IUserProfile profile)
      {
         return new ModelingSessionAction(context, profile);
      }

      public ModelingSessionAction WithProfile(Func<ProfileCreator, ProfileCreator> profileCreator)
      {
         var creator = profileCreator(new ProfileCreator());

         return new ModelingSessionAction(context, creator.Profile);
      }

      public ModelingSessionAction WithAnonymous
      {
         get
         {
            return new ModelingSessionAction(context, UserProfile.Anonymous);
         }
      }
   }

   public class ProfileCreator
   {
      Guid userid;
      readonly IList<RecipeRating> ratings;
      readonly IList<PantryItem> pantry;
      readonly IList<Guid> favIngs;
      RecipeTags favTags;
      readonly IList<Guid> blacklistIng;
      Guid avoidRecipe;
      RecipeTags allowedTags;

      public ProfileCreator()
      {
         ratings = new List<RecipeRating>();
         pantry = new List<PantryItem>();
         favIngs = new List<Guid>();
         blacklistIng = new List<Guid>();
      }

      public ProfileCreator WithUserId(Guid userid)
      {
         this.userid = userid;
         return this;
      }

      public ProfileCreator AddRating(RecipeRating rating)
      {
         ratings.Add(rating);
         return this;
      }

      public ProfileCreator AddRating(Recipe recipe, byte rating)
      {
         ratings.Add(new RecipeRating
         {
            RecipeId = recipe.Id,
            Rating = rating
         });

         return this;
      }

      public ProfileCreator AddPantryItem(PantryItem item)
      {
         pantry.Add(item);
         return this;
      }

      public ProfileCreator AddPantryItem(IngredientUsage usage)
      {
         pantry.Add(new PantryItem(usage));
         return this;
      }

      public ProfileCreator AddFavoriteIngredient(Ingredient ingredient)
      {
         favIngs.Add(ingredient.Id);
         return this;
      }

      public ProfileCreator FavoriteTags(RecipeTags tags)
      {
         favTags = tags;
         return this;
      }

      public ProfileCreator AddBlacklistedIngredient(Ingredient ingredient)
      {
         blacklistIng.Add(ingredient.Id);
         return this;
      }

      public ProfileCreator AvoidRecipe(Recipe recipe)
      {
         avoidRecipe = recipe.Id;
         return this;
      }

      public ProfileCreator AllowedTags(RecipeTags tags)
      {
         allowedTags = tags;
         return this;
      }

      public IUserProfile Profile
      {
         get
         {
            return new UserProfile
            {
               UserId = userid,
               Ratings = ratings.ToArray(),
               Pantry = pantry.Any() ? pantry.ToArray() : null, // Pantry must be null or more than 0 items
               FavoriteIngredients = favIngs.ToArray(),
               FavoriteTags = favTags,
               BlacklistedIngredients = blacklistIng.ToArray(),
               AvoidRecipe = avoidRecipe,
               AllowedTags = allowedTags
            };
         }
      }
   }

   public class ModelingSessionAction
   {
      readonly ModelingSession session;

      int recipes = 5;
      byte scale = 2;

      public ModelingSessionAction(ModelingSession session)
      {
         this.session = session;
      }

      public ModelingSessionAction(IKPCContext context, IUserProfile profile)
      {
         session = context.CreateModelingSession(profile);
      }

      public ModelingSessionAction NumRecipes(int recipes)
      {
         this.recipes = recipes;
         return this;
      }

      public ModelingSessionAction Scale(byte scale)
      {
         this.scale = scale;
         return this;
      }

      public Model Generate()
      {
         return session.Generate(recipes, scale);
      }

      public CompiledModel Compile()
      {
         var model = Generate();

         return session.Compile(model);
      }
   }
}