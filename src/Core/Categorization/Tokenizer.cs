using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KitchenPC.Recipes;

namespace KitchenPC.Categorization
{
   //Takes a Recipe object and returns an enumeration of Token objects
   public static class Tokenizer
   {
      static readonly Regex valid = new Regex(@"[a-z]", RegexOptions.IgnoreCase); //All tokens have to have at least one letter in them

      static IEnumerable<IToken> ParseText(string text)
      {
         var parts = Regex.Split(text, @"[^a-z0-9\'\$\-]", RegexOptions.IgnoreCase);
         return (from p in parts where valid.IsMatch(p) select new TextToken(p) as IToken);
      }

      public static IEnumerable<IToken> Tokenize(Recipe recipe)
      {
         var tokens = new List<IToken>();
         tokens.AddRange(ParseText(recipe.Title ?? ""));
         tokens.AddRange(ParseText(recipe.Description ?? ""));
         //tokens.AddRange(ParseText(recipe.Method ?? ""));
         //tokens.Add(new TimeToken(recipe.CookTime.GetValueOrDefault() + recipe.PrepTime.GetValueOrDefault()));
         tokens.AddRange(from i in recipe.Ingredients.NeverNull() select new IngredientToken(i.Ingredient) as IToken);

         return tokens;
      }
   }
}