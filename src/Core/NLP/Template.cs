using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using KitchenPC.NLP.Tokens;

namespace KitchenPC.NLP
{
   public class Template : IComparable<string>, IEquatable<string>
   {
      readonly LinkedList<IGrammar> grammar;
      readonly string template;

      public string DefaultPrep { get; set; } //A template can set a default prep note if no other is found (to translate certain keywords into prep notes)
      public bool AllowPartial { get; set; } //A template can allow partial matches, such as "eggs"

      public Template(string template)
      {
         grammar = new LinkedList<IGrammar>();

         this.CompileTemplate(template);
         this.template = template;
      }

      public override string ToString()
      {
         return template;
      }

      void CompileTemplate(string template)
      {
         var reToken = new Regex(@"(\[[A-Z]+\])");

         //Break apart template into sets of IGrammar chunks, eg: [AMT] [UNIT] [ING] or [ING]: [AMT] [UNIT]
         var chunks = reToken.Split(template);
         foreach (var chunk in chunks)
         {
            if (chunk.Trim().Length == 0)
               continue;

            var token = reToken.IsMatch(chunk) ? GetParser(chunk) : new StaticToken(chunk);

            grammar.AddLast(token);
         }
      }

      public Result Parse(string usage)
      {
         var matchdata = new MatchData();
         var stream = new MemoryStream(Encoding.Default.GetBytes(usage), false);

         foreach (var g in grammar)
         {
            if (false == g.Read(stream, matchdata))
            {
               return new NoMatch(usage, MatchResult.NoMatch);
            }
         }

         //If we get here, we've satisfied the grammar of the template
         if (stream.ReadByte() >= 0) //There's still unread bytes in this stream but no more grammar, we don't have a perfect match
         {
            return new NoMatch(usage, MatchResult.NoMatch); //TODO: If no other templates match, this might be our best bet - store closest match and return that.
         }

         //Success!  Now we just have to build the full IngredientUsage from the collected data
         NlpTracer.Trace(TraceLevel.Info, "Usage \"{0}\" matches the grammar \"{1}\"", usage, this);
         var ret = Result.BuildResult(this, usage, matchdata);
         return ret;
      }

      static IGrammar GetParser(string tokenName)
      {
         switch (tokenName)
         {
            case "[AMT]":
               return new AmtToken();
            case "[UNIT]":
               return new UnitToken();
            case "[ING]":
               return new IngToken();
            case "[FORM]":
               return new FormToken();
            case "[PREP]":
               return new PrepToken();
            case "[ANOMALY]":
               return new AnomToken();
         }

         throw new UnknownTokenException(tokenName);
      }

      public int CompareTo(string other)
      {
         return String.Compare(this.template, other, StringComparison.Ordinal);
      }

      public bool Equals(string other)
      {
         return this.template.Equals(other);
      }

      public static implicit operator Template(string t)
      {
         return new Template(t);
      }

      public static implicit operator string(Template t)
      {
         return t.template;
      }
   }
}