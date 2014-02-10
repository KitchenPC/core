using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KitchenPC.Context;
using KitchenPC.Recipes;

namespace KitchenPC.Modeler
{
   /// <summary>
   /// Captures an instance of the database in memory for modeling without requiring DB hits.
   /// This "cache" only contains the exact information it needs and is optimized for modeling performance.
   /// </summary>
   public sealed partial class DBSnapshot
   {
      /// <summary>
      /// A proxy class to allow an IDBLoader to populate indexes within a DBSnapshot.  This is IDisposable since this object might use
      /// large amount of memory while building indexes, and a GC should be forced when object is disposed.
      /// </summary>
      class Indexer : IDisposable
      {
         readonly DBSnapshot snapshot;
         RatingGraph ratingGraph;

         public Indexer(DBSnapshot snapshot)
         {
            this.snapshot = snapshot;
            snapshot.recipeMap = new Dictionary<Guid, RecipeNode>();
            snapshot.ingredientMap = new Dictionary<Guid, IngredientNode>();
            snapshot.recipeList = new IEnumerable<RecipeNode>[RecipeTag.NUM_TAGS];
         }

         public void Index(IKPCContext context)
         {
            var timer = new Stopwatch();
            timer.Start();

            ratingGraph = new RatingGraph();
            var loader = context.ModelerLoader;

            foreach (var dataItem in loader.LoadRatingGraph())
            {
               var r = dataItem.Rating;
               var uid = dataItem.UserId;
               var rid = dataItem.RecipeId;

               if (r < 4) //Rating too low to worry about
               {
                  continue; //TODO: Might not be needed, DB should only query for 4 or 5 star ratings
               }

               ratingGraph.AddRating(r, uid, rid);
            }

            ModelingSession.Log.InfoFormat("Building Rating Graph took {0}ms.", timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();

            //Create empty recipe nodes without links
            snapshot.recipeMap = (from o in loader.LoadRecipeGraph()
               select new RecipeNode()
               {
                  RecipeId = o.Id,
                  Rating = o.Rating,
                  Tags = o.Tags,
                  Hidden = o.Hidden,
                  Ingredients = new List<IngredientUsage>()
               }).ToDictionary(k => k.RecipeId);

            ModelingSession.Log.InfoFormat("Building empty RecipeNodes took {0}ms.", timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();

            //Build tag index
            foreach (var r in snapshot.recipeMap.Values)
            {
               if (r.Hidden)
                  continue; //_recipeList does not include Hidden recipes so they don't get picked at random

               foreach (var tag in r.Tags)
               {
                  var nodes = snapshot.recipeList[tag.Value] as List<RecipeNode>;
                  if (nodes == null)
                     snapshot.recipeList[tag.Value] = nodes = new List<RecipeNode>();

                  nodes.Add(r);
               }
            }

            for (var i = 0; i < snapshot.recipeList.Length; i++)
            {
               var list = snapshot.recipeList[i] as List<RecipeNode>;
               if (list != null)
               {
                  snapshot.recipeList[i] = list.ToArray();
               }
               else //No recipes in DB use this tag
               {
                  snapshot.recipeList[i] = new RecipeNode[0];
               }
            }

            ModelingSession.Log.InfoFormat("Indexing recipes by tag took {0}ms.", timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();

            //Loop through ingredient usages and fill in vertices on graph
            //For each item: Create IngredientUsage and add to recipe, create IngredientNode (if necessary) and add recipe to IngredientNode
            foreach (var o in loader.LoadIngredientGraph())
            {
               var rid = o.RecipeId;
               var ingid = o.IngredientId;
               var qty = o.Qty;
               var unit = o.Unit;
               var convType = Unit.GetConvType(unit);

               List<RecipeNode>[] nodes;
               IngredientNode ingNode;
               var node = snapshot.recipeMap[rid];

               if (!snapshot.ingredientMap.TryGetValue(ingid, out ingNode)) //New ingredient, create node for it
               {
                  nodes = new List<RecipeNode>[RecipeTag.NUM_TAGS];
                  snapshot.ingredientMap.Add(ingid, ingNode = new IngredientNode()
                  {
                     IngredientId = ingid,
                     RecipesByTag = nodes,
                     ConvType = convType
                  });
               }
               else
               {
                  nodes = ingNode.RecipesByTag as List<RecipeNode>[];
               }

               //For each tag the recipe has, we need to create a link through ingNode.RecipesByTag to the recipe
               if (!node.Hidden) //Don't index Hidden recipes
               {
                  foreach (var tag in node.Tags)
                  {
                     if (nodes[tag.Value] == null)
                     {
                        nodes[tag.Value] = new List<RecipeNode>();
                     }

                     nodes[tag.Value].Add(node); //Add ingredient link to RecipeNode
                  }
               }

               var usages = node.Ingredients as List<IngredientUsage>; //Add ingredient usage to recipe
               usages.Add(new IngredientUsage()
               {
                  Amt = qty,
                  Ingredient = ingNode,
                  Unit = unit
               });
            }

            ModelingSession.Log.InfoFormat("Creating IngredientUsage vertices took {0}ms.", timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();

            //Create suggestion links for each recipe
            foreach (var r in snapshot.recipeMap.Values)
            {
               r.Suggestions = (from s in ratingGraph.GetSimilarRecipes(r.RecipeId) select snapshot.recipeMap[s]).ToArray();
            }

            ModelingSession.Log.InfoFormat("Building suggestions for each recipe took {0}ms.", timer.ElapsedMilliseconds);
            timer.Reset();
         }

         public void Dispose()
         {
            var timer = new Stopwatch();
            timer.Start();

            ratingGraph = null;

            //Free up memory/increase index accessing speed by converting List<> objects to arrays
            foreach (var r in snapshot.recipeMap.Values)
            {
               r.Ingredients = r.Ingredients.ToArray();
            }

            foreach (var i in snapshot.ingredientMap.Values)
            {
               var temp = new List<RecipeNode[]>();
               var usedTags = 0;

               for (var c = 0; c < RecipeTag.NUM_TAGS; c++)
               {
                  RecipeNode[] nodes = null;
                  if (i.RecipesByTag[c] != null)
                  {
                     nodes = i.RecipesByTag[c].ToArray();
                     usedTags += (1 << c);
                  }

                  temp.Add(nodes);
               }

               i.RecipesByTag = temp.ToArray();
               i.AvailableTags = usedTags;
            }

            GC.Collect(); //Force garbage collection now, since there might be several hundred megs of unreachable allocations

            timer.Stop();
            ModelingSession.Log.InfoFormat("Cleaning up Indexer took {0}ms.", timer.ElapsedMilliseconds);
         }
      }
   }

   public sealed partial class DBSnapshot
   {
      Dictionary<Guid, RecipeNode> recipeMap; //Recipe Index (will include hidden recipes)
      Dictionary<Guid, IngredientNode> ingredientMap; //Ingredient Index
      IEnumerable<RecipeNode>[] recipeList; //Ordinal recipe index keyed by tag (for picking random recipes)

      public int RecipeCount
      {
         get
         {
            return recipeMap.Keys.Count;
         }
      }

      public DBSnapshot(IKPCContext context)
      {
         var timer = new Stopwatch();
         timer.Start();

         using (var i = new Indexer(this))
         {
            i.Index(context);
         }

         timer.Stop();
         ModelingSession.Log.InfoFormat("Total time building snapshot was {0}ms.", timer.ElapsedMilliseconds);
      }

      public RecipeNode FindRecipe(Guid id)
      {
         return recipeMap.ContainsKey(id) ? recipeMap[id] : null;
      }

      public RecipeNode[] FindRecipesByTag(int tag)
      {
         return recipeList[tag] as RecipeNode[];
      }

      public IngredientNode FindIngredient(Guid id)
      {
         return ingredientMap.ContainsKey(id) ? ingredientMap[id] : null;
      }
   }
}