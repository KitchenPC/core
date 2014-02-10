using System;
using System.Collections.Generic;

namespace KitchenPC.NLP
{
   public abstract class SynonymTree<T>
   {
      protected static AlphaTree<T> index;
      protected static Dictionary<string, T> synonymMap;

      protected static void IndexString(string value, T node)
      {
         var parsedIng = value.Trim().ToLower();

         if (synonymMap.ContainsKey(parsedIng)) //Uh oh
         {
            Parser.Log.Error(String.Format("The ingredient synonym '{0}' also exists as a root ingredient.", value));
         }
         else
         {
            synonymMap.Add(parsedIng, node);
         }

         var curNode = index.Head;
         for (var i = 0; i < parsedIng.Length; i++)
         {
            var c = parsedIng[i];
            if (curNode.HasLink(c) == false)
            {
               curNode = curNode.AddLink(c);
            }
            else
            {
               curNode = curNode.GetLink(c);
            }

            curNode.AddConnection(node);
         }
      }

      public MatchPrecision Parse(string substr, out T match)
      {
         substr = substr.TrimStart(' '); //Strip off any leading spaces, an empty string will always return "Partial" so processing will continue

         if (synonymMap.TryGetValue(substr, out match)) //If they pass in a complete ingredient name, return Exact
         {
            return MatchPrecision.Exact;
         }

         //Figure out if this is a partial match for an ingredient
         var node = index.Head;

         for (var i = 0; i < substr.Length; i++)
         {
            if (node.HasLink(substr[i]) == false) //No possible match here
               return MatchPrecision.None;

            node = node.GetLink(substr[i]);
         }

         //match = null;
         return MatchPrecision.Partial;
      }
   }
}