using System;
using System.IO;

namespace KitchenPC.NLP.Tokens
{
   public class FormToken : IGrammar
   {
      static FormSynonyms data;

      /// <summary>
      /// Reads stream to match it against a dictionary of all known forms
      /// </summary>
      /// <param name="stream"></param>
      /// <param name="matchdata"></param>
      /// <returns></returns>
      public bool Read(Stream stream, MatchData matchdata)
      {
         if (data == null)
         {
            data = new FormSynonyms();
         }

         FormNode node;
         matchdata.Form = null;
         var buffer = String.Empty;
         var matchPos = stream.Position;
         int curByte;

         while ((curByte = stream.ReadByte()) >= 0)
         {
            buffer += (char) curByte;
            var match = data.Parse(buffer, out node);
            if (match == MatchPrecision.None)
            {
               break; //No reason to continue reading stream, let's see what we have..
            }

            if (match == MatchPrecision.Exact)
            {
               matchPos = stream.Position;
               matchdata.Form = node;
            }
         }

         stream.Seek(matchPos, SeekOrigin.Begin);
         return (matchdata.Form != null);
      }
   }
}