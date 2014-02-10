using System.Collections.Generic;
using System.Linq;
using KitchenPC.Recipes;

namespace KitchenPC.Context.Fluent
{
   /// <summary>Provides the ability to fluently express recipe queue related actions, such as loading queue and enqueuing/dequeuing recipes.</summary>
   public class QueueAction
   {
      readonly IKPCContext context;

      public QueueAction(IKPCContext context)
      {
         this.context = context;
      }

      public QueueLoader Load
      {
         get
         {
            return new QueueLoader(context);
         }
      }

      public RecipeEnqueuer Enqueue
      {
         get
         {
            return new RecipeEnqueuer(context);
         }
      }

      public RecipeDequeuer Dequeue
      {
         get
         {
            return new RecipeDequeuer(context);
         }
      }
   }

   /// <summary>Loads the recipe queue.</summary>
   public class QueueLoader
   {
      readonly IKPCContext context;

      public QueueLoader(IKPCContext context)
      {
         this.context = context;
      }

      public IList<RecipeBrief> List()
      {
         return context.GetRecipeQueue();
      }
   }

   /// <summary>Provides the ability to enqueue one or more recipes.</summary>
   public class RecipeEnqueuer
   {
      readonly IKPCContext context;
      readonly IList<Recipe> recipesQueue;

      public RecipeEnqueuer(IKPCContext context)
      {
         this.context = context;
         recipesQueue = new List<Recipe>();
      }

      public RecipeEnqueuer Recipe(Recipe recipe)
      {
         recipesQueue.Add(recipe);
         return this;
      }

      public void Commit()
      {
         if (recipesQueue.Any())
            context.EnqueueRecipes(recipesQueue.Select(r => r.Id).ToArray());
      }
   }

   /// <summary>Provides the ability to dequeue one or more recipes.</summary>
   public class RecipeDequeuer
   {
      readonly IKPCContext context;
      readonly IList<Recipe> toDequeue;
      bool dequeueAll;

      public RecipeDequeuer(IKPCContext context)
      {
         this.context = context;
         toDequeue = new List<Recipe>();
      }

      public RecipeDequeuer All
      {
         get
         {
            return new RecipeDequeuer(context)
            {
               dequeueAll = true
            };
         }
      }

      public RecipeDequeuer Recipe(Recipe recipe)
      {
         toDequeue.Add(recipe);
         return this;
      }

      public void Commit()
      {
         if (dequeueAll)
            context.DequeueRecipe();
         else
            context.DequeueRecipe(toDequeue.Select(r => r.Id).ToArray());
      }
   }
}