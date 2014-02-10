using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace KitchenPC.NLP
{
   public class Parser
   {
      public delegate void NoMatchEvent(NoMatch result, string usage);

      List<Template> templates;
      static readonly Regex reWhitespace = new Regex(@"[ ]{2,}", RegexOptions.Compiled);

      public static ILog Log = LogManager.GetLogger(typeof (Parser));
      public NoMatchEvent OnNoMatch;
      public TemplateStatistics Stats { get; private set; }

      static void ReplaceAccents(ref string input)
      {
         if (String.IsNullOrEmpty(input))
            return;

         input = Regex.Replace(input, @"[\xC0-\xC5\xE0-\xE5]", "a"); //Replace with "a"
         input = Regex.Replace(input, @"[\xC8-\xCB\xE8-\xEB]", "e"); //Replace with "e"
         input = Regex.Replace(input, @"[\xCC-\xCF\xEC-\xEF]", "i"); //Replace with "i"
         input = Regex.Replace(input, @"[\xD1\xF1]", "n"); //Replace with "n"
         input = Regex.Replace(input, @"[\xD2-\xD6\xF2-\xF6]", "o"); //Replace with "o"
         input = Regex.Replace(input, @"[\xD9-\xDC\xF9-\xFC]", "u"); //Replace with "u"
         input = Regex.Replace(input, @"[\xDD\xDF\xFF]", "y"); //Replace with "y"
      }

      public void LoadTemplates(params Template[] templates)
      {
         this.templates = new List<Template>(templates.Length);
         Stats = new TemplateStatistics();

         foreach (var t in templates)
         {
            NlpTracer.Trace(TraceLevel.Debug, "Loaded Template: {0}", t);
            this.templates.Add(t);
            Stats.RecordTemplate(t);
         }
      }

      public IEnumerable<Result> ParseAll(params String[] input)
      {
         return input.Select(Parse);
      }

      public Result Parse(string input)
      {
         ReplaceAccents(ref input);
         var normalizedInput = reWhitespace.Replace(input, " ").ToLower(); //Replace 2 or more spaces with a single space since whitespace doesn't really matter

         //Loop through all loaded templates looking for a match - return that match, or return null if unknown
         var bestResult = MatchResult.NoMatch;
         foreach (var t in templates)
         {
            var result = t.Parse(normalizedInput);
            if (result is Match)
            {
               Stats[t]++;
               return result;
            }
            else
            {
               if (result.Status > bestResult)
               {
                  bestResult = result.Status;
               }
            }
         }

         NlpTracer.Trace(TraceLevel.Info, "Could not find match for usage: {0}", input);
         var ret = new NoMatch(input, bestResult);
         if (this.OnNoMatch != null)
         {
            OnNoMatch(ret, input);
         }

         return ret; //TODO: Save best match to get error code
      }
   }
}