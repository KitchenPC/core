using System;
using System.IO;

namespace KitchenPC.NLP.Tokens
{
   public class UnitToken : IGrammar
   {
      static UnitSynonyms data;

      /// <summary>
      /// Reads stream to match it against a dictionary of all known units for an ingredient
      /// </summary>
      /// <param name="stream"></param>
      /// <param name="matchdata"></param>
      /// <returns></returns>
      public bool Read(Stream stream, MatchData matchdata)
      {
         if (data == null)
         {
            data = new UnitSynonyms();
         }

         UnitNode node;
         var fMatch = false;
         var buffer = String.Empty;
         var matchPos = stream.Position;
         int curByte;

         while ((curByte = stream.ReadByte()) >= 0)
         {
            buffer += (char) curByte;
            var match = data.Parse(buffer, out node);
            if (match == MatchPrecision.None)
            {
               stream.Seek(matchPos, SeekOrigin.Begin);
               break; //No reason to continue reading stream, let's see what we have..
            }

            if (match == MatchPrecision.Exact)
            {
               matchPos = stream.Position;
               fMatch = true;
               matchdata.Amount.Unit = node.Unit;
               matchdata.Unit = node;
            }
         }

         return fMatch;
      }
   }
}