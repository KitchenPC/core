using System;
using System.IO;
using System.Text.RegularExpressions;

namespace KitchenPC.NLP.Tokens
{
   public class PrepToken : IGrammar
   {
      static PrepNotes data;

      public bool Read(Stream stream, MatchData matchData)
      {
         if (data == null)
         {
            data = new PrepNotes();
         }

         var buffer = String.Empty;
         PrepNode foundPrep = null;
         var fFound = false;
         var matchPos = stream.Position;
         int curByte;

         while ((curByte = stream.ReadByte()) >= 0)
         {
            buffer += (char) curByte;

            //Prep tokens can have leading commas or parens - so trim these off
            buffer = Regex.Replace(buffer, @"^\s*(,|-|\()\s*", "");
            buffer = Regex.Replace(buffer, @"\s*\)\s*$", "");

            PrepNode node;
            var match = data.Parse(buffer, out node);
            if (match == MatchPrecision.None)
            {
               break; //No reason to continue reading stream, let's see what we have..
            }

            if (match == MatchPrecision.Exact)
            {
               matchPos = stream.Position;
               foundPrep = node;
               fFound = true;
            }
         }

         if (foundPrep != null) //Add the prep at the end of the loop in case we found multiple preps along the way
         {
            matchData.Preps.Add(foundPrep);
         }

         stream.Seek(matchPos, SeekOrigin.Begin);
         return fFound;
      }
   }
}