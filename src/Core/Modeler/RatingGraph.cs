using System;
using System.Collections.Generic;
using System.Linq;

namespace KitchenPC.Modeler
{

   #region Support classes for RatingGraph

   partial class RatingGraph
   {
      class RecipeNode
      {
         public Guid Key { get; set; }
         public List<Rating> Ratings { get; set; }
      }

      class UserNode
      {
         public Guid Key { get; set; }
         public List<Rating> Ratings { get; set; }
      }

      class Rating
      {
         public RecipeNode Recipe { get; set; }
         public UserNode User { get; set; }
         public short Weight { get; set; }
      }
   }

   #endregion

   partial class RatingGraph
   {
      readonly Dictionary<Object, RecipeNode> _recipeIndex;
      readonly Dictionary<Object, UserNode> _userIndex;

      public RatingGraph()
      {
         _recipeIndex = new Dictionary<object, RecipeNode>();
         _userIndex = new Dictionary<object, UserNode>();
      }

      public void AddRating(short r, Guid uid, Guid rid)
      {
         RecipeNode rnode;
         UserNode unode;

         if (_recipeIndex.ContainsKey(rid) == false)
         {
            rnode = new RecipeNode();
            rnode.Key = rid;
            rnode.Ratings = new List<Rating>();
            _recipeIndex.Add(rid, rnode);
         }
         else
         {
            rnode = _recipeIndex[rid];
         }

         if (_userIndex.ContainsKey(uid) == false)
         {
            unode = new UserNode();
            unode.Key = uid;
            unode.Ratings = new List<Rating>();
            _userIndex.Add(uid, unode);
         }
         else
         {
            unode = _userIndex[uid];
         }

         var v = new Rating();
         v.Weight = r;
         v.Recipe = rnode;
         v.User = unode;

         rnode.Ratings.Add(v);
         unode.Ratings.Add(v);
      }

      public Guid[] GetSimilarRecipes(Guid recipeid)
      {
         if (_recipeIndex.ContainsKey(recipeid) == false)
         {
            return new Guid[] {};
         }

         var rnode = _recipeIndex[recipeid];
         var results = new Dictionary<Guid, int>();
         foreach (var r1 in rnode.Ratings)
         {
            var unode = r1.User;
            foreach (var r2 in unode.Ratings)
            {
               if (r2.Recipe.Key == recipeid)
                  continue;

               if (results.ContainsKey(r2.Recipe.Key) == false)
               {
                  results.Add(r2.Recipe.Key, 1);
               }
               else
               {
                  results[r2.Recipe.Key]++;
               }
            }
         }

         //For every pair in graph, calculate the Jaccard similarity coefficient (Number of overlapping ingredients divided by total distinct ingredients in both)

         var r = (from k in results.Keys
            orderby results[k] descending
            select k);

         return r.ToArray();
      }
   }
}