using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KitchenPC.Context
{
   public class IngredientParser
   {
      const int MIN_SUBSTR = 3; // TODO: Should this be configurable?

      Dictionary<Guid, String> nameLookup;
      AlphaTree index;

      public IEnumerable<IngredientNode> MatchIngredient(string query)
      {
         ConnectorVertex connections;
         if (FindSubstr(query, out connections))
         {
            if (connections != null)
            {
               return connections.Connections;
            }
         }

         return Enumerable.Empty<IngredientNode>();
      }

      public string GetIngredientById(Guid id)
      {
         if (nameLookup == null)
         {
            throw new IngredientMapNotInitializedException();
         }

         string r;
         if (nameLookup.TryGetValue(id, out r) == false)
         {
            throw new IngredientMapInvalidIngredientException();
         }

         return r;
      }

      public void CreateIndex(IEnumerable<IngredientSource> datasource)
      {
         nameLookup = new Dictionary<Guid, string>();
         index = new AlphaTree();

         foreach (var ing in datasource)
         {
            if (String.IsNullOrWhiteSpace(ing.DisplayName)) continue;

            ParseString(ing.DisplayName, ing.Id);
            nameLookup.Add(ing.Id, ing.DisplayName);
         }
      }

      void ParseString(string ing, Guid id)
      {
         var iStart = 0;
         var iLen = MIN_SUBSTR;
         var node = new IngredientNode(id, ing, 0);
         var parsedIng = Regex.Replace(ing.ToLower(), "[^a-z]", "");

         while (iStart + iLen <= parsedIng.Length)
         {
            var substr = parsedIng.Substring(iStart, iLen);
            IndexSubStr(substr, node);
            iLen++;

            if (iStart + iLen > parsedIng.Length)
            {
               iStart++;
               iLen = MIN_SUBSTR;
            }
         }
      }

      void IndexSubStr(string substr, IngredientNode node)
      {
         var curNode = index.Head;

         for (var i = 0; i < substr.Length; i++)
         {
            var c = substr[i];
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

      bool FindSubstr(string substr, out ConnectorVertex connections)
      {
         var node = index.Head;
         connections = null;
         substr = Regex.Replace(substr, "[^a-z]", "");

         for (var i = 0; i < substr.Length; i++)
         {
            if (node.HasLink(substr[i]) == false)
               return false;

            node = node.GetLink(substr[i]);
         }

         connections = node.connections;
         return true;
      }
   }
}